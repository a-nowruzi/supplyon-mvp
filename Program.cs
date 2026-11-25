using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using O2Ban.ValidationWorkspace.Data;
using O2Ban.ValidationWorkspace.Models;

var builder = WebApplication.CreateBuilder(args);
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "MariaDb";
builder.Services.AddDbContext<AppDb>(options =>
{
    if (databaseProvider.Equals("MariaDb", StringComparison.OrdinalIgnoreCase) || databaseProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = builder.Configuration.GetConnectionString("MariaDb")
            ?? throw new InvalidOperationException("ConnectionStrings:MariaDb تنظیم نشده است.");
        options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 4, 0)),
            mysql => mysql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
    }
    else
    {
        throw new InvalidOperationException("فقط MariaDB برای این نسخه پشتیبانی و آزمون می‌شود.");
    }
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
    o.Cookie.Name = "O2Ban.Validation.Auth"; o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.LoginPath = "/"; o.ExpireTimeSpan = TimeSpan.FromHours(8); o.SlidingExpiration = true;
    o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
var app = builder.Build();

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (DbUpdateException ex)
    {
        app.Logger.LogError(ex, "Database write failed for {Path}", context.Request.Path);
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = "ذخیره اطلاعات انجام نشد. احتمالاً اطلاعات تکراری یا ناسازگار است؛ مقادیر ورودی را بررسی کنید." });
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled request error for {Path}", context.Request.Path);
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "خطای داخلی رخ داد. شناسه پیگیری را با مدیر سامانه در میان بگذارید." });
        }
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    await db.Database.EnsureCreatedAsync();
    // Additive, idempotent lookup-table upgrade for databases created by v0.2/v0.3.
    await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS `LookupOptions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Group` varchar(60) NOT NULL,
        `Value` varchar(120) NOT NULL,
        `SortOrder` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT 1,
        PRIMARY KEY (`Id`),
        UNIQUE KEY `IX_LookupOptions_Group_Value` (`Group`, `Value`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;");
    await LookupSeeder.SeedAsync(db);
    if (builder.Configuration.GetValue("DemoData:Enabled", true))
        await DemoSeeder.SeedAsync(db);
}
if (!app.Environment.IsDevelopment()) { app.UseHsts(); }
// TLS is terminated by the Linux host's reverse proxy; the container serves private HTTP internally.
app.UseDefaultFiles(); app.UseStaticFiles(); app.UseAuthentication(); app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", async (AppDb db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    if (!canConnect) return (IResult)Results.StatusCode(503);
    var lookupCount = await db.LookupOptions.CountAsync(x => x.IsActive);
    if (lookupCount < 49) return (IResult)Results.Problem("اطلاعات پایه کامل نیست.", statusCode: 503);
    return (IResult)Results.Ok(new { status = "ready", database = "MariaDB", activeLookupOptions = lookupCount });
});

app.MapPost("/api/auth/login", async (LoginRequest req, HttpContext ctx, AppDb db) => {
    var user = await db.Users.FirstOrDefaultAsync(x => x.Username == req.Username && x.IsActive);
    if (user is null || !PasswordUtil.Verify(req.Password, user.PasswordHash)) return (IResult)Results.Unauthorized();
    var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Name, user.FullName), new(ClaimTypes.Role, user.Role), new("org", user.OrganizationName) };
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = req.RememberMe });
    return (IResult)Results.Ok(new { name = user.FullName, role = user.Role, organization = user.OrganizationName });
});
app.MapPost("/api/auth/logout", async (HttpContext ctx) => { await ctx.SignOutAsync(); return Results.Ok(); });
app.MapPost("/api/auth/change-password", async (ChangePasswordRequest req, ClaimsPrincipal principal, AppDb db) => {
    var idText = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(idText, out var id)) return (IResult)Results.Unauthorized();
    var user = await db.Users.FindAsync(id);
    if (user is null || !PasswordUtil.Verify(req.CurrentPassword, user.PasswordHash)) return (IResult)Results.BadRequest(new { message = "رمز فعلی صحیح نیست." });
    if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 12) return (IResult)Results.BadRequest(new { message = "رمز جدید باید حداقل ۱۲ کاراکتر داشته باشد." });
    user.PasswordHash = PasswordUtil.Hash(req.NewPassword); await db.SaveChangesAsync(); return (IResult)Results.Ok(new { message = "رمز عبور تغییر کرد." });
}).RequireAuthorization();
app.MapGet("/api/auth/me", (ClaimsPrincipal p) => Results.Ok(new { name = p.Identity?.Name, role = p.FindFirst(ClaimTypes.Role)?.Value, organization = p.FindFirst("org")?.Value })).RequireAuthorization();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapGet("/lookups", async (AppDb db) =>
{
    var rows = await db.LookupOptions.Where(x => x.IsActive)
        .OrderBy(x => x.Group).ThenBy(x => x.SortOrder).ToListAsync();
    return Results.Ok(rows.GroupBy(x => x.Group)
        .ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToArray()));
});
api.MapGet("/dashboard", async (AppDb db) => Results.Ok(new {
    companies = await db.Companies.CountAsync(), suppliers = await db.Suppliers.CountAsync(), products = await db.Products.CountAsync(), requirements = await db.Requirements.CountAsync(), openRfqs = await db.Rfqs.CountAsync(x => x.Status != "بسته"), offers = await db.Offers.CountAsync(), feedback = await db.Feedback.CountAsync(),
    recentRequirements = await db.Requirements.OrderByDescending(x => x.CreatedAt).Take(5).Select(x => new { x.Id, x.Title, x.Category, x.Status, x.CreatedAt }).ToListAsync(),
    recentRfqs = await db.Rfqs.OrderByDescending(x => x.CreatedAt).Take(5).Select(x => new { x.Id, x.Code, x.Title, x.Status, x.Deadline }).ToListAsync()
}));

api.MapGet("/companies", async (AppDb db) => Results.Ok(await db.Companies.OrderBy(x => x.Name).ToListAsync()));
api.MapPost("/companies", async (Company x, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; db.Companies.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/companies/{x.Id}", x); });
api.MapGet("/suppliers", async (AppDb db) => Results.Ok(await db.Suppliers.OrderBy(x => x.Name).ToListAsync()));
api.MapPost("/suppliers", async (Supplier x, AppDb db) =>
{
    x.Id = 0;
    x.Name = x.Name?.Trim() ?? "";
    x.NationalId = string.IsNullOrWhiteSpace(x.NationalId) ? null : x.NationalId.Trim();
    x.City = x.City?.Trim() ?? "";
    x.ContactName = x.ContactName?.Trim() ?? "";
    x.Capabilities = x.Capabilities?.Trim() ?? "";
    x.Products = x.Products?.Trim() ?? "";
    x.PaymentTerms = x.PaymentTerms?.Trim() ?? "";
    x.Notes = x.Notes?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.Name)) return Results.BadRequest(new { message = "نام شرکت تأمین‌کننده الزامی است." });
    if (x.Name.Length > 180) return Results.BadRequest(new { message = "نام شرکت حداکثر ۱۸۰ نویسه می‌تواند باشد." });
    if (string.IsNullOrWhiteSpace(x.Capabilities)) return Results.BadRequest(new { message = "ثبت حداقل یک توانمندی الزامی است." });
    if (x.Capabilities.Length > 2000) return Results.BadRequest(new { message = "شرح توانمندی‌ها بیش از حد مجاز است." });
    if (x.NationalId is not null && (x.NationalId.Length != 11 || !x.NationalId.All(char.IsDigit)))
        return Results.BadRequest(new { message = "شناسه ملی باید دقیقاً ۱۱ رقم باشد یا خالی بماند." });
    if (x.LeadTimeDays < 0) return Results.BadRequest(new { message = "زمان تحویل نمی‌تواند منفی باشد." });
    var allowedEvidence = await db.LookupOptions.Where(o => o.Group == "evidence-status" && o.IsActive).Select(o => o.Value).ToListAsync();
    if (!allowedEvidence.Contains(x.EvidenceStatus)) return Results.BadRequest(new { message = "وضعیت شواهد انتخاب‌شده معتبر نیست." });
    if (x.NationalId is not null && await db.Suppliers.AnyAsync(s => s.NationalId == x.NationalId))
        return Results.Conflict(new { message = "تأمین‌کننده‌ای با این شناسه ملی قبلاً ثبت شده است." });
    x.CreatedAt = DateTime.UtcNow;
    db.Suppliers.Add(x);
    await db.SaveChangesAsync();
    return Results.Created($"/api/suppliers/{x.Id}", x);
});
api.MapGet("/products", async (AppDb db) => Results.Ok(await db.Products.OrderBy(x => x.Name).ToListAsync()));
api.MapPost("/products", async (Product x, AppDb db) => {
    x.Id = 0; x.Code = x.Code?.Trim() ?? ""; x.Name = x.Name?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.Code) || string.IsNullOrWhiteSpace(x.Name)) return (IResult)Results.BadRequest(new { message = "کد و نام کالا الزامی است." });
    if (x.Code.Length > 60 || x.Name.Length > 180) return (IResult)Results.BadRequest(new { message = "طول کد یا نام کالا بیش از حد مجاز است." });
    if (string.IsNullOrWhiteSpace(x.Category) || !await db.LookupOptions.AnyAsync(o => o.Group == "product-category" && o.Value == x.Category && o.IsActive))
        return (IResult)Results.BadRequest(new { message = "گروه کالایی باید از فهرست اطلاعات پایه انتخاب شود." });
    if (string.IsNullOrWhiteSpace(x.Unit) || !await db.LookupOptions.AnyAsync(o => o.Group == "unit" && o.Value == x.Unit && o.IsActive))
        return (IResult)Results.BadRequest(new { message = "واحد اندازه‌گیری باید از فهرست اطلاعات پایه انتخاب شود." });
    if (await db.Products.AnyAsync(p => p.Code == x.Code)) return (IResult)Results.Conflict(new { message = "کالایی با این کد قبلاً ثبت شده است." });
    db.Products.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/products/{x.Id}", x);
});
api.MapGet("/requirements", async (AppDb db) => Results.Ok(await db.Requirements.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/requirements", async (PurchaseRequirement x, ClaimsPrincipal p, AppDb db) => {
    x.Id = 0; x.Title = x.Title?.Trim() ?? ""; x.Category = x.Category?.Trim() ?? ""; x.Description = x.Description?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.Category) || string.IsNullOrWhiteSpace(x.Description)) return (IResult)Results.BadRequest(new { message = "عنوان، گروه کالا و شرح فنی درخواست الزامی است." });
    if (x.Quantity <= 0 || x.RequiredWithinDays <= 0) return (IResult)Results.BadRequest(new { message = "مقدار و مهلت تأمین باید بیشتر از صفر باشند." });
    if (string.IsNullOrWhiteSpace(x.Unit) || string.IsNullOrWhiteSpace(x.DeliveryLocation)) return (IResult)Results.BadRequest(new { message = "واحد و محل تحویل الزامی است." });
    if (!await db.LookupOptions.AnyAsync(o => o.Group == "product-category" && o.Value == x.Category && o.IsActive))
        return (IResult)Results.BadRequest(new { message = "گروه کالایی انتخاب‌شده معتبر نیست." });
    if (!await db.LookupOptions.AnyAsync(o => o.Group == "unit" && o.Value == x.Unit && o.IsActive))
        return (IResult)Results.BadRequest(new { message = "واحد انتخاب‌شده معتبر نیست." });
    if (!await db.LookupOptions.AnyAsync(o => o.Group == "priority" && o.Value == x.Priority && o.IsActive))
        return (IResult)Results.BadRequest(new { message = "اولویت درخواست معتبر نیست." });
    x.CreatedAt = DateTime.UtcNow; x.CreatedBy = p.Identity?.Name ?? "کاربر"; x.Status = "جدید"; db.Requirements.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/requirements/{x.Id}", x);
});
api.MapGet("/matches/{requirementId:int}", async (int requirementId, AppDb db) => {
    var r = await db.Requirements.FindAsync(requirementId);
    if (r is null) return (IResult)Results.NotFound();
    var ranked = (await db.Suppliers.ToListAsync())
        .Select(s => EvaluateMatch(r, s))
        .OrderByDescending(x => x.Eligible)
        .ThenByDescending(x => x.Score)
        .Take(15)
        .Select(x => new {
            x.Supplier.Id, x.Supplier.Name, x.Supplier.City, x.Supplier.Capabilities,
            x.Supplier.EvidenceStatus, x.Supplier.PaymentTerms, x.Supplier.LeadTimeDays,
            score = x.Score, eligible = x.Eligible, reasons = x.Reasons,
            warnings = x.Warnings, breakdown = x.Breakdown,
            scoreDisclaimer = "امتیاز صرفاً غربالگری اولیه بر اساس داده‌های ثبت‌شده است؛ تأیید فنی، تجاری یا حقوقی محسوب نمی‌شود."
        });
    return (IResult)Results.Ok(ranked);
});
api.MapGet("/rfqs", async (AppDb db) => Results.Ok(await db.Rfqs.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/rfqs", async (Rfq x, ClaimsPrincipal p, AppDb db) => {
    x.Id = 0; x.Title = x.Title?.Trim() ?? ""; x.RequirementTitle = x.RequirementTitle?.Trim() ?? ""; x.InvitedSuppliers = x.InvitedSuppliers?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.RequirementTitle) || string.IsNullOrWhiteSpace(x.InvitedSuppliers)) return (IResult)Results.BadRequest(new { message = "عنوان، درخواست مرتبط و حداقل یک تأمین‌کننده دعوت‌شده الزامی است." });
    if (x.Deadline <= DateTime.UtcNow) return (IResult)Results.BadRequest(new { message = "مهلت پاسخ RFQ باید در آینده باشد." });
    x.CreatedAt = DateTime.UtcNow; x.CreatedBy = p.Identity?.Name ?? "کاربر"; x.Status = "در انتظار پیشنهاد"; if (string.IsNullOrWhiteSpace(x.Code)) x.Code = $"RFQ-{DateTime.UtcNow:yyMMddHHmmss}";
    if (await db.Rfqs.AnyAsync(r => r.Code == x.Code)) return (IResult)Results.Conflict(new { message = "کد RFQ تکراری است." });
    db.Rfqs.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/rfqs/{x.Id}", x);
});
api.MapGet("/offers", async (AppDb db) => Results.Ok(await db.Offers.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/offers", async (Offer x, AppDb db) => {
    x.Id = 0; x.RfqCode = x.RfqCode?.Trim() ?? ""; x.SupplierName = x.SupplierName?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.RfqCode) || string.IsNullOrWhiteSpace(x.SupplierName)) return (IResult)Results.BadRequest(new { message = "کد RFQ و نام تأمین‌کننده الزامی است." });
    if (!await db.Rfqs.AnyAsync(r => r.Code == x.RfqCode)) return (IResult)Results.BadRequest(new { message = "کد RFQ در سامانه پیدا نشد." });
    if (!await db.Suppliers.AnyAsync(s => s.Name == x.SupplierName)) return (IResult)Results.BadRequest(new { message = "تأمین‌کننده در فهرست سامانه پیدا نشد؛ ابتدا رکورد آن را بررسی کنید." });
    if (x.UnitPrice <= 0 || x.Quantity <= 0 || x.DeliveryDays <= 0) return (IResult)Results.BadRequest(new { message = "قیمت، مقدار و زمان تحویل باید بیشتر از صفر باشند." });
    if (x.ValidUntil <= DateTime.UtcNow) return (IResult)Results.BadRequest(new { message = "تاریخ اعتبار پیشنهاد باید در آینده باشد." });
    x.CreatedAt = DateTime.UtcNow; db.Offers.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/offers/{x.Id}", x);
});
api.MapGet("/evidence", async (AppDb db) => Results.Ok(await db.Evidence.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/evidence", async (Evidence x, AppDb db) => {
    x.Id = 0; x.SupplierName = x.SupplierName?.Trim() ?? ""; x.Claim = x.Claim?.Trim() ?? ""; x.Notes = x.Notes?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(x.SupplierName) || string.IsNullOrWhiteSpace(x.Claim)) return (IResult)Results.BadRequest(new { message = "نام تأمین‌کننده و شرح ادعا الزامی است." });
    if (!await db.Suppliers.AnyAsync(s => s.Name == x.SupplierName)) return (IResult)Results.BadRequest(new { message = "تأمین‌کننده در سامانه پیدا نشد." });
    var sources = new[] { "اظهار شرکت", "کاتالوگ", "سند بارگذاری‌شده", "منبع رسمی", "بازرسی مستقل", "شواهد معامله" };
    var statuses = await db.LookupOptions.Where(o => o.Group == "evidence-status" && o.IsActive).Select(o => o.Value).ToListAsync();
    if (!sources.Contains(x.Source) || !statuses.Contains(x.Status)) return (IResult)Results.BadRequest(new { message = "منبع یا وضعیت شواهد معتبر نیست." });
    x.CreatedAt = DateTime.UtcNow; db.Evidence.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/evidence/{x.Id}", x);
});
api.MapGet("/feedback", async (AppDb db) => Results.Ok(await db.Feedback.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/feedback", async (Feedback x, ClaimsPrincipal p, AppDb db) => {
    x.Id = 0; x.Description = x.Description?.Trim() ?? "";
    var types = new[] { "مشکل", "پیشنهاد", "نقص اطلاعات", "خطا", "رضایت" };
    var modules = new[] { "عمومی", "شرکت‌ها", "تأمین‌کنندگان", "کاتالوگ", "درخواست خرید", "کشف تأمین‌کننده", "RFQ", "پیشنهادها", "شواهد", "گزارش‌ها" };
    if (!types.Contains(x.Type) || !modules.Contains(x.Module)) return (IResult)Results.BadRequest(new { message = "نوع بازخورد یا بخش نرم‌افزار معتبر نیست." });
    if (x.Rating < 1 || x.Rating > 5) return (IResult)Results.BadRequest(new { message = "امتیاز باید بین ۱ تا ۵ باشد." });
    if (string.IsNullOrWhiteSpace(x.Description) || x.Description.Length < 10) return (IResult)Results.BadRequest(new { message = "شرح بازخورد باید حداقل ۱۰ نویسه باشد." });
    x.CreatedAt = DateTime.UtcNow; x.UserName = p.Identity?.Name ?? "کاربر"; db.Feedback.Add(x); await db.SaveChangesAsync(); return (IResult)Results.Created($"/api/feedback/{x.Id}", x);
});
api.MapGet("/reports", async (AppDb db) => Results.Ok(new {
    users = await db.Users.CountAsync(), companies = await db.Companies.CountAsync(), suppliers = await db.Suppliers.CountAsync(), requirements = await db.Requirements.CountAsync(), rfqs = await db.Rfqs.CountAsync(), offers = await db.Offers.CountAsync(), feedback = await db.Feedback.CountAsync(),
    avgSupplierScore = await db.Suppliers.AverageAsync(x => (double?)x.InitialScore) ?? 0,
    evidenceByStatus = await db.Suppliers.GroupBy(x => x.EvidenceStatus).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(),
    feedbackByType = await db.Feedback.GroupBy(x => x.Type).Select(g => new { type = g.Key, count = g.Count() }).ToListAsync()
}));

app.MapFallbackToFile("index.html");
app.Run();

static MatchEvaluation EvaluateMatch(PurchaseRequirement r, Supplier s)
{
    var reasons = new List<string>();
    var warnings = new List<string>();
    var breakdown = new Dictionary<string, int>();
    var reqText = NormalizeText($"{r.Title} {r.Category} {r.Description}");
    var supplierText = NormalizeText($"{s.Capabilities} {s.Products} {s.Name}");
    var stop = new HashSet<string>(new[] { "برای", "باید", "است", "عدد", "مورد", "نیاز", "خرید", "تأمین", "تامین", "قطعات", "قطعه", "تولیدی", "غیرتولیدی", "داخلی", "خارجی", "خودرو", "خودروی", "با", "از", "در", "و", "یا", "به", "تا", "این", "آن", "یک", "های", "شود", "شده", "مربوط", "شرح", "مشخصات", "فنی" });
    var terms = reqText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length >= 3 && !stop.Contains(x)).Distinct().ToArray();
    var hits = terms.Count(t => supplierText.Contains(t, StringComparison.Ordinal));
    var capabilityScore = terms.Length == 0 ? 15 : (int)Math.Round(45d * hits / terms.Length);
    capabilityScore = Math.Clamp(capabilityScore, 0, 45);
    breakdown["ارتباط کالایی"] = capabilityScore;
    if (hits > 0) reasons.Add($"هم‌پوشانی متنی در {hits} واژه از {terms.Length} واژه کلیدی");
    else warnings.Add("هم‌پوشانی کالایی در متن ثبت‌شده پیدا نشد؛ طبقه‌بندی دستی لازم است.");

    var leadScore = 0;
    if (r.RequiredWithinDays > 0 && s.LeadTimeDays > 0)
    {
        if (s.LeadTimeDays <= r.RequiredWithinDays) { leadScore = 20; reasons.Add("زمان تحویل اعلامی در محدوده مهلت درخواست است."); }
        else { leadScore = 3; warnings.Add("زمان تحویل اعلامی از مهلت موردنیاز بیشتر است."); }
    }
    else warnings.Add("زمان تحویل برای مقایسه کافی نیست.");
    breakdown["زمان تحویل"] = leadScore;

    var evidenceScore = s.EvidenceStatus switch
    {
        "تأییدشده توسط شرکت" => 20,
        "دارای مدرک" => 15,
        "اعلام‌شده" => 8,
        "نیازمند به‌روزرسانی" => 4,
        _ => 0
    };
    breakdown["وضعیت شواهد"] = evidenceScore;
    if (s.EvidenceStatus is "مشکوک" or "ردشده") warnings.Add("وضعیت شواهد پرریسک است؛ این تأمین‌کننده نباید بدون بررسی مستقل انتخاب شود.");
    else if (s.EvidenceStatus == "دارای تناقض") warnings.Add("بین شواهد یا اطلاعات ثبت‌شده تناقض وجود دارد.");
    else if (s.EvidenceStatus == "تأییدشده توسط شرکت") warnings.Add("تأیید توسط خود شرکت به معنی اعتبارسنجی مستقل نیست.");
    else if (s.EvidenceStatus == "دارای مدرک") warnings.Add("وجود مدرک به‌تنهایی اصالت، اعتبار یا انطباق آن را اثبات نمی‌کند.");

    var paymentScore = string.IsNullOrWhiteSpace(s.PaymentTerms) ? 0 : 10;
    breakdown["اطلاعات پرداخت"] = paymentScore;
    if (paymentScore > 0) reasons.Add("شرایط پرداخت برای مذاکره اولیه ثبت شده است.");
    else warnings.Add("شرایط پرداخت ثبت نشده است.");

    var scopeScore = 0;
    var reqScope = r.Title.Contains("داخلی") ? "داخلی" : r.Title.Contains("خارجی") ? "خارجی" : "";
    var supplierScope = s.Notes.Contains("داخلی /", StringComparison.Ordinal) ? "داخلی" : s.Notes.Contains("خارجی /", StringComparison.Ordinal) ? "خارجی" : "";
    var scopeMismatch = reqScope.Length > 0 && supplierScope.Length > 0 && reqScope != supplierScope;
    if (reqScope.Length == 0 || supplierScope.Length == 0) scopeScore = 2;
    else if (!scopeMismatch) { scopeScore = 5; reasons.Add("دامنه داخلی/خارجی با سناریوی درخواست هم‌راستاست."); }
    else warnings.Add("دامنه داخلی/خارجی تأمین‌کننده با درخواست هم‌خوان نیست.");
    breakdown["تناسب دامنه"] = scopeScore;

    var total = breakdown.Values.Sum();
    var blocked = s.EvidenceStatus is "مشکوک" or "ردشده";
    var eligible = !blocked && !scopeMismatch;
    if (blocked) warnings.Add("این رکورد از پیشنهادهای قابل‌اتکا کنار گذاشته شده است.");
    if (warnings.Count == 0) warnings.Add("با وجود امتیاز اولیه، بررسی انسانی و استعلام مستقیم الزامی است.");
    return new MatchEvaluation(s, total, eligible, reasons.ToArray(), warnings.Distinct().ToArray(), breakdown);
}

static string NormalizeText(string value) => value.ToLowerInvariant()
    .Replace('ي', 'ی').Replace('ك', 'ک').Replace('ۀ', 'ه').Replace('ة', 'ه')
    .Replace('‌', ' ').Replace('-', ' ').Replace('/', ' ').Replace('،', ' ').Replace(',', ' ')
    .Replace('.', ' ').Replace('؛', ' ').Replace(':', ' ');

record MatchEvaluation(Supplier Supplier, int Score, bool Eligible, string[] Reasons, string[] Warnings, Dictionary<string, int> Breakdown);

record LoginRequest(string Username, string Password, bool RememberMe = false);
record ChangePasswordRequest(string CurrentPassword, string NewPassword);
static class PasswordUtil {
    public static string Hash(string password) { var salt = RandomNumberGenerator.GetBytes(16); var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, 32); return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash); }
    public static bool Verify(string password, string encoded) { try { var p = encoded.Split('.'); var salt = Convert.FromBase64String(p[0]); var expected = Convert.FromBase64String(p[1]); var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, expected.Length); return CryptographicOperations.FixedTimeEquals(expected, actual); } catch { return false; } }
}
