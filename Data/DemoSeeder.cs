using Microsoft.EntityFrameworkCore;
using O2Ban.ValidationWorkspace.Models;
namespace O2Ban.ValidationWorkspace.Data;
public static class DemoSeeder {
 public static async Task SeedAsync(AppDb db) {
  if (!await db.Users.AnyAsync()) {
   string h(string p)=>PasswordHashing.Hash(p);
   db.Users.AddRange(new AppUser{Username="expert1",FullName="کارشناس آزمایشی ۱",Role="کارشناس خرید",PasswordHash=h("O2ban@2026")},new AppUser{Username="expert2",FullName="کارشناس آزمایشی ۲",Role="کارشناس خرید",PasswordHash=h("O2ban@2026")},new AppUser{Username="expert3",FullName="کارشناس آزمایشی ۳",Role="کارشناس خرید",PasswordHash=h("O2ban@2026")},new AppUser{Username="expert4",FullName="کارشناس آزمایشی ۴",Role="کارشناس خرید",PasswordHash=h("O2ban@2026")},new AppUser{Username="admin",FullName="مدیر محیط آزمایشی",Role="مدیر سامانه",PasswordHash=h("O2ban@2026")});
  }
  if (!await db.Companies.AnyAsync()) db.Companies.AddRange(
   new Company{Name="صنایع قطعه‌سازی آذرخش (نمونه)",Tier="Tier 1",City="تهران",ContactName="مریم نادری",ContactRole="مدیر خرید",RelationshipStatus="شناسایی‌شده",Notes="تمام اطلاعات این رکورد ساختگی است."},
   new Company{Name="پارس‌موتوران نمونه",Tier="Tier 2",City="قزوین",ContactName="رضا کریمی",ContactRole="کارشناس تدارکات",RelationshipStatus="در انتظار تماس",Notes="داده نمایشی مستقل."},
   new Company{Name="فن‌آوران خودرو شرق (نمونه)",Tier="Tier 2",City="مشهد",ContactName="سارا احمدی",ContactRole="مدیر تدارکات",RelationshipStatus="مصاحبه برنامه‌ریزی‌شده",Notes="داده ساختگی."},
   new Company{Name="توسعه صنعت آریا (نمونه)",Tier="Tier 1",City="اصفهان",ContactName="کیان رستگار",ContactRole="مدیرعامل",RelationshipStatus="شناسایی‌شده",Notes="اطلاعات کاملاً ساختگی برای آزمون."},
   new Company{Name="پیشگامان ساخت شرق (نمونه)",Tier="Tier 2",City="تبریز",ContactName="نگار موسوی",ContactRole="کارشناس خرید",RelationshipStatus="در انتظار تماس",Notes="اطلاعات کاملاً ساختگی برای آزمون."});
  if (!await db.Suppliers.AnyAsync()) db.Suppliers.AddRange(
   new Supplier{Name="تأمین صنعت سپهر (نمونه)",City="تهران",Capabilities="ملزومات عمومی MRO، فیلتر صنعتی، روانکار",Products="فیلتر صنعتی، فیلتر روغن، گریس",ContactName="علی رضایی",PaymentTerms="نقدی و ۳۰ روزه",LeadTimeDays=5,EvidenceStatus="تأییدشده توسط شرکت",InitialScore=78,Notes="داده ساختگی؛ تأیید مستقل نشده."},
   new Supplier{Name="پایا تجهیز البرز (نمونه)",City="کرج",Capabilities="قطعات مکانیکی، یاتاقان، تسمه",Products="بلبرینگ، تسمه صنعتی، کاسه‌نمد",ContactName="نیما صالحی",PaymentTerms="نقدی",LeadTimeDays=3,EvidenceStatus="اعلام‌شده",InitialScore=62,Notes="داده ساختگی."},
   new Supplier{Name="پیشرو پنوماتیک شرق (نمونه)",City="قزوین",Capabilities="پنوماتیک و هیدرولیک، اتصالات صنعتی",Products="شیلنگ، اتصالات پنوماتیک، شیر",ContactName="الهام مرادی",PaymentTerms="۳۰ روزه",LeadTimeDays=7,EvidenceStatus="نیازمند به‌روزرسانی",InitialScore=55,Notes="داده ساختگی."},
   new Supplier{Name="الکتروصنعت نوین (نمونه)",City="تهران",Capabilities="ملزومات برق صنعتی و تأسیسات",Products="کابل، کلید صنعتی، اتصالات برق",ContactName="امیر محمدی",PaymentTerms="نقدی و چکی",LeadTimeDays=4,EvidenceStatus="اعلام‌شده",InitialScore=58,Notes="داده ساختگی."},
   new Supplier{Name="پارس روانکار آزمایشی",City="اراک",Capabilities="روانکار صنعتی، روغن هیدرولیک و گریس",Products="روغن صنعتی، گریس نسوز",ContactName="آرمان شریفی",PaymentTerms="۳۰ روزه",LeadTimeDays=6,EvidenceStatus="نیازمند به‌روزرسانی",InitialScore=52,Notes="داده ساختگی؛ صرفاً برای آزمون سناریو."},
   new Supplier{Name="تجهیز گستر نمونه",City="تبریز",Capabilities="تجهیزات ایمنی و ابزار دقیق",Products="دستکش صنعتی، سنسور فشار",ContactName="هانیه کریمی",PaymentTerms="نقدی",LeadTimeDays=8,EvidenceStatus="اعلام‌شده",InitialScore=49,Notes="داده ساختگی."},
   new Supplier{Name="صنایع بسته‌بندی فرضی",City="قزوین",Capabilities="بسته‌بندی صنعتی و مواد مصرفی تولید",Products="فیلم استرچ، تسمه بسته‌بندی",ContactName="مهدی نوری",PaymentTerms="نقدی و ۱۵ روزه",LeadTimeDays=5,EvidenceStatus="دارای تناقض",InitialScore=43,Notes="داده ساختگی؛ وضعیت تناقض برای آزمون گردش بررسی."});
  if (!await db.Products.AnyAsync()) db.Products.AddRange(
   new Product{Code="MRO-001",Name="فیلتر صنعتی",Category="ملزومات نگهداری",Unit="عدد",TechnicalDescription="نوع، ابعاد، درجه فیلتراسیون و سازگاری باید مشخص شود."},
   new Product{Code="MRO-002",Name="بلبرینگ صنعتی",Category="اقلام مکانیکی",Unit="عدد",TechnicalDescription="کد فنی، ابعاد، کلاس لقی و برند."},
   new Product{Code="MRO-003",Name="شیلنگ پنوماتیک",Category="پنوماتیک و هیدرولیک",Unit="متر",TechnicalDescription="قطر داخلی، فشار کاری، جنس و دمای کاری."},
   new Product{Code="MRO-004",Name="کابل برق صنعتی",Category="برق و تأسیسات",Unit="متر",TechnicalDescription="سطح مقطع، جنس هادی، ولتاژ و استاندارد."},
   new Product{Code="MRO-005",Name="روغن هیدرولیک",Category="روانکار صنعتی",Unit="لیتر",TechnicalDescription="گرید ویسکوزیته، استاندارد و سازگاری با تجهیز."},
   new Product{Code="MRO-006",Name="دستکش ایمنی صنعتی",Category="ایمنی و حفاظت فردی",Unit="جفت",TechnicalDescription="جنس، سطح حفاظت، سایز و استاندارد مرتبط."},
   new Product{Code="MRO-007",Name="سنسور فشار",Category="ابزار دقیق",Unit="عدد",TechnicalDescription="محدوده فشار، نوع خروجی، دقت و اتصال."},
   new Product{Code="MRO-008",Name="فیلم استرچ",Category="بسته‌بندی صنعتی",Unit="کیلوگرم",TechnicalDescription="ضخامت، عرض، کشش و نوع مصرف."});
  if (!await db.Requirements.AnyAsync()) db.Requirements.Add(new PurchaseRequirement{Title="تأمین فیلتر صنعتی",Category="ملزومات نگهداری",Description="فیلتر صنعتی برای سرویس دوره‌ای تجهیزات؛ مشخصات دقیق پیش از سفارش باید تأیید شود.",Quantity=20,Unit="عدد",DeliveryLocation="تهران",RequiredWithinDays=7,PaymentTerms="اعتباری",Priority="زیاد",Status="جدید",CreatedBy="کاربر نمونه",CreatedAt=DateTime.UtcNow.AddDays(-1)});
  if (!await db.Rfqs.AnyAsync()) db.Rfqs.Add(new Rfq{Code="RFQ-DEMO-001",Title="استعلام فیلتر صنعتی",RequirementTitle="تأمین فیلتر صنعتی",InvitedSuppliers="تأمین صنعت سپهر (نمونه)، پایا تجهیز البرز (نمونه)",Deadline=DateTime.UtcNow.AddDays(5),Status="در انتظار پیشنهاد",CreatedBy="کاربر نمونه",CreatedAt=DateTime.UtcNow.AddHours(-8)});
  if (!await db.Offers.AnyAsync()) db.Offers.AddRange(new Offer{RfqCode="RFQ-DEMO-001",SupplierName="تأمین صنعت سپهر (نمونه)",UnitPrice=1250000,Quantity=20,DeliveryDays=5,PaymentTerms="۳۰ روزه",TechnicalFit="نیازمند تأیید",ValidUntil=DateTime.UtcNow.AddDays(4),Notes="قیمت و شرایط کاملاً فرضی."},new Offer{RfqCode="RFQ-DEMO-001",SupplierName="پایا تجهیز البرز (نمونه)",UnitPrice=1180000,Quantity=20,DeliveryDays=3,PaymentTerms="نقدی",TechnicalFit="نیازمند تأیید",ValidUntil=DateTime.UtcNow.AddDays(3),Notes="قیمت و شرایط کاملاً فرضی."});
  if (!await db.Evidence.AnyAsync()) db.Evidence.AddRange(new Evidence{SupplierName="تأمین صنعت سپهر (نمونه)",Claim="عرضه فیلترهای صنعتی",Source="اظهار شرکت",Status="تأییدشده توسط شرکت",ReviewDate=DateTime.UtcNow.AddDays(-20),Notes="تأیید مستقل انجام نشده."},new Evidence{SupplierName="پایا تجهیز البرز (نمونه)",Claim="تأمین بلبرینگ و تسمه صنعتی",Source="کاتالوگ نمونه",Status="اعلام‌شده",Notes="نیازمند بررسی."});
  await db.SaveChangesAsync();
 }
}
static class PasswordHashing { public static string Hash(string p) { var s=System.Security.Cryptography.RandomNumberGenerator.GetBytes(16); var h=System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(p,s,120000,System.Security.Cryptography.HashAlgorithmName.SHA256,32); return Convert.ToBase64String(s)+"."+Convert.ToBase64String(h); } }
