using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microting.EntityFrameworkCore.MySql.Infrastructure;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    if (builder.Environment.IsProduction())
    {
        // Coolify assigns proxy container addresses dynamically. The app container
        // must not publish its port directly when accepting forwarded headers.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
    else
    {
        options.KnownProxies.Add(IPAddress.Loopback);
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
    }
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection تنظیم نشده است.");
if (builder.Environment.IsProduction() && (connectionString.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Password=;", StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("Connection string امن Production تنظیم نشده است.");
var mariaDbVersion = new MariaDbServerVersion(new Version(11, 4, 0));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, mariaDbVersion, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 12; options.Password.RequireDigit = true; options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true; options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5; options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Account/Login"; options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.Name = "__Host-O2Ban.Auth"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.SlidingExpiration = true; options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
var app = builder.Build();
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.Use(async (context, next) => { if (string.Equals(context.Request.Path.Value, "/Identity/Account/Register", StringComparison.OrdinalIgnoreCase)) { context.Response.Redirect("/Identity/Account/Login"); return; } await next(); });
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapGet("/health", async (ApplicationDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ok" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable)).AllowAnonymous();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); app.MapRazorPages();
using (var scope = app.Services.CreateScope()) await DbInitializer.InitializeAsync(scope.ServiceProvider, app.Environment);
app.Run();
