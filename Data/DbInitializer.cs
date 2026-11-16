using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Models;
namespace O2Ban.EnterprisePilot.Data;
public static class DbInitializer
{
 public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
 {
  var email=configuration["O2Ban:BootstrapAdmin:Email"]; var password=configuration["O2Ban:BootstrapAdmin:Password"];
  if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("مقدارهای O2Ban:BootstrapAdmin:Email و O2Ban:BootstrapAdmin:Password را در appsettings.json تنظیم کنید.");
  var db=services.GetRequiredService<ApplicationDbContext>(); await db.Database.EnsureCreatedAsync();
  var roles=services.GetRequiredService<RoleManager<IdentityRole>>();
  foreach(var role in new[]{"Admin","Procurement"}) if(!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
  var users=services.GetRequiredService<UserManager<ApplicationUser>>();
  var existingAdmins=await users.GetUsersInRoleAsync("Admin");
  if(!string.IsNullOrWhiteSpace(email)&&!string.IsNullOrWhiteSpace(password)) {
   var admin=await users.FindByEmailAsync(email);
   if(admin is null) { admin=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,DisplayName="مدیر سامانه",Department="مدیریت محصول"}; var result=await users.CreateAsync(admin,password); if(!result.Succeeded) throw new InvalidOperationException("ساخت مدیر اولیه ناموفق بود: "+string.Join("; ",result.Errors.Select(e=>e.Description))); }
   if(!await users.IsInRoleAsync(admin,"Admin")) await users.AddToRoleAsync(admin,"Admin");
  }
  var seed=configuration.GetValue<bool>("O2Ban:SeedDemoData");
  if(!seed)return;
  if(!await db.CatalogItems.AnyAsync()){ db.CatalogItems.AddRange(
   new CatalogItem{ItemCode="DEMO-PACK-001",Name="کارتن بسته‌بندی صنعتی (نمونه)",IndustrySegment="داده نمایشی",Category="بسته‌بندی",TechnicalDescription="داده ساختگی؛ برای معامله واقعی استفاده نشود"},
   new CatalogItem{ItemCode="DEMO-PUMP-002",Name="پمپ انتقال سیال صنعتی (نمونه)",IndustrySegment="داده نمایشی",Category="تجهیزات مکانیکی",TechnicalDescription="داده ساختگی؛ مشخصات نیازمند تکمیل"},
   new CatalogItem{ItemCode="DEMO-CABLE-003",Name="کابل برق صنعتی (نمونه)",IndustrySegment="داده نمایشی",Category="برق و الکتریکال",TechnicalDescription="داده ساختگی برای آزمون جست‌وجو"}); await db.SaveChangesAsync(); }
  if(!await db.Suppliers.AnyAsync()){ db.Suppliers.AddRange(
   new Supplier{Name="تأمین‌کننده نمونه الف (ساختگی)",City="تهران",Capabilities="داده نمایشی بسته‌بندی",EvidenceStatus="اظهار تأمین‌کننده",IsDemoData=true},
   new Supplier{Name="تأمین‌کننده نمونه ب (ساختگی)",City="قزوین",Capabilities="داده نمایشی تجهیزات مکانیکی",EvidenceStatus="نیازمند بررسی",IsDemoData=true},
   new Supplier{Name="تأمین‌کننده نمونه ج (ساختگی)",City="اصفهان",Capabilities="داده نمایشی تجهیزات الکتریکی",EvidenceStatus="اظهار تأمین‌کننده",IsDemoData=true}); await db.SaveChangesAsync(); }
 }
}
