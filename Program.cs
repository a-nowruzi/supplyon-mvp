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
    return canConnect ? Results.Ok(new { status = "ready", database = "MariaDB" }) : Results.StatusCode(503);
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
    var allowedEvidence = new[] { "اعلام‌شده", "تأییدشده توسط شرکت", "نیازمند به‌روزرسانی", "دارای تناقض", "مشکوک", "ردشده" };
    if (!allowedEvidence.Contains(x.EvidenceStatus)) return Results.BadRequest(new { message = "وضعیت شواهد انتخاب‌شده معتبر نیست." });
    if (x.NationalId is not null && await db.Suppliers.AnyAsync(s => s.NationalId == x.NationalId))
        return Results.Conflict(new { message = "تأمین‌کننده‌ای با این شناسه ملی قبلاً ثبت شده است." });
    x.CreatedAt = DateTime.UtcNow;
    db.Suppliers.Add(x);
    await db.SaveChangesAsync();
    return Results.Created($"/api/suppliers/{x.Id}", x);
});
api.MapGet("/products", async (AppDb db) => Results.Ok(await db.Products.OrderBy(x => x.Name).ToListAsync()));
api.MapPost("/products", async (Product x, AppDb db) => { x.Id = 0; db.Products.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/products/{x.Id}", x); });
api.MapGet("/requirements", async (AppDb db) => Results.Ok(await db.Requirements.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/requirements", async (PurchaseRequirement x, ClaimsPrincipal p, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; x.CreatedBy = p.Identity?.Name ?? "کاربر"; x.Status = "جدید"; db.Requirements.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/requirements/{x.Id}", x); });
api.MapGet("/matches/{requirementId:int}", async (int requirementId, AppDb db) => {
    var r = await db.Requirements.FindAsync(requirementId); if (r is null) return (IResult)Results.NotFound();
    var suppliers = await db.Suppliers.ToListAsync();
    var ranked = suppliers.Select(s => new { supplier = s, score = MatchScore(r, s) }).OrderByDescending(x => x.score).Take(10).Select(x => new { x.supplier.Id, x.supplier.Name, x.supplier.City, x.supplier.Capabilities, x.supplier.EvidenceStatus, x.supplier.PaymentTerms, x.supplier.LeadTimeDays, score = x.score, reasons = MatchReasons(r, x.supplier) });
    return (IResult)Results.Ok(ranked);
});
api.MapGet("/rfqs", async (AppDb db) => Results.Ok(await db.Rfqs.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/rfqs", async (Rfq x, ClaimsPrincipal p, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; x.CreatedBy = p.Identity?.Name ?? "کاربر"; x.Status = "در انتظار پیشنهاد"; if (string.IsNullOrWhiteSpace(x.Code)) x.Code = $"RFQ-{DateTime.UtcNow:yyMMddHHmmss}"; db.Rfqs.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/rfqs/{x.Id}", x); });
api.MapGet("/offers", async (AppDb db) => Results.Ok(await db.Offers.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/offers", async (Offer x, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; db.Offers.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/offers/{x.Id}", x); });
api.MapGet("/evidence", async (AppDb db) => Results.Ok(await db.Evidence.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/evidence", async (Evidence x, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; db.Evidence.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/evidence/{x.Id}", x); });
api.MapGet("/feedback", async (AppDb db) => Results.Ok(await db.Feedback.OrderByDescending(x => x.CreatedAt).ToListAsync()));
api.MapPost("/feedback", async (Feedback x, ClaimsPrincipal p, AppDb db) => { x.Id = 0; x.CreatedAt = DateTime.UtcNow; x.UserName = p.Identity?.Name ?? "کاربر"; db.Feedback.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/feedback/{x.Id}", x); });
api.MapGet("/reports", async (AppDb db) => Results.Ok(new {
    users = await db.Users.CountAsync(), companies = await db.Companies.CountAsync(), suppliers = await db.Suppliers.CountAsync(), requirements = await db.Requirements.CountAsync(), rfqs = await db.Rfqs.CountAsync(), offers = await db.Offers.CountAsync(), feedback = await db.Feedback.CountAsync(),
    avgSupplierScore = await db.Suppliers.AverageAsync(x => (double?)x.InitialScore) ?? 0,
    evidenceByStatus = await db.Suppliers.GroupBy(x => x.EvidenceStatus).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(),
    feedbackByType = await db.Feedback.GroupBy(x => x.Type).Select(g => new { type = g.Key, count = g.Count() }).ToListAsync()
}));

app.MapFallbackToFile("index.html");
app.Run();

static int MatchScore(PurchaseRequirement r, Supplier s) {
    var text = $"{s.Capabilities} {s.Products} {s.Name}".ToLowerInvariant(); var terms = ($"{r.Title} {r.Category} {r.Description}").ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct();
    var hits = terms.Count(t => t.Length > 2 && text.Contains(t)); var score = Math.Min(60, hits * 15);
    if (s.EvidenceStatus == "تأییدشده توسط شرکت") score += 15; else if (s.EvidenceStatus == "اعلام‌شده") score += 8;
    if (s.LeadTimeDays > 0 && r.RequiredWithinDays > 0) score += s.LeadTimeDays <= r.RequiredWithinDays ? 15 : 4;
    if (!string.IsNullOrWhiteSpace(s.PaymentTerms)) score += 10;
    return Math.Min(100, score);
}
static string[] MatchReasons(PurchaseRequirement r, Supplier s) { var a = new List<string>(); if (s.Capabilities.Contains(r.Category, StringComparison.OrdinalIgnoreCase) || s.Products.Contains(r.Category, StringComparison.OrdinalIgnoreCase)) a.Add("ارتباط با گروه کالایی"); if (s.EvidenceStatus != "اعلام‌نشده") a.Add("دارای وضعیت شواهد ثبت‌شده"); if (s.LeadTimeDays > 0 && r.RequiredWithinDays > 0) a.Add(s.LeadTimeDays <= r.RequiredWithinDays ? "زمان تحویل در محدوده نیاز" : "زمان تحویل نیازمند بررسی"); if (a.Count == 0) a.Add("تطبیق اولیه متنی؛ نیازمند بررسی انسانی"); return a.ToArray(); }

record LoginRequest(string Username, string Password, bool RememberMe = false);
record ChangePasswordRequest(string CurrentPassword, string NewPassword);
static class PasswordUtil {
    public static string Hash(string password) { var salt = RandomNumberGenerator.GetBytes(16); var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, 32); return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash); }
    public static bool Verify(string password, string encoded) { try { var p = encoded.Split('.'); var salt = Convert.FromBase64String(p[0]); var expected = Convert.FromBase64String(p[1]); var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, expected.Length); return CryptographicOperations.FixedTimeEquals(expected, actual); } catch { return false; } }
}
