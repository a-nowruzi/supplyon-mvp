using O2Ban.EnterprisePilot.Models;
namespace O2Ban.EnterprisePilot.Services;
/// <summary>Rule-based decision support. Never selects a supplier automatically.</summary>
public static class OfferComparisonEngine
{
 public static List<OfferComparisonRow> Compare(PurchaseRequirement requirement,IReadOnlyCollection<SupplierOffer> latestOffers,DateTime today)
 {
  var maxAllowedDays=Math.Max(0,(requirement.NeededBy.Date-today.Date).Days);
  var rows=latestOffers.Select(o=>new OfferComparisonRow{
   OfferId=o.Id,SupplierId=o.SupplierId,SupplierName=o.Supplier?.Name??"تأمین‌کننده نامشخص",
   EvidenceStatus=string.IsNullOrWhiteSpace(o.Supplier?.EvidenceStatus)?"نامشخص":o.Supplier!.EvidenceStatus,
   UnitPrice=o.UnitPrice,TotalPrice=o.UnitPrice*requirement.Quantity,DeliveryDays=o.DeliveryDays,
   PaymentTerms=string.IsNullOrWhiteSpace(o.PaymentTerms)?"ثبت نشده":o.PaymentTerms,ValidUntil=o.ValidUntil,
   IsExpired=o.ValidUntil.Date<today.Date,TechnicalStatus=string.IsNullOrWhiteSpace(o.TechnicalStatus)?"نامشخص":o.TechnicalStatus,
   IsTechnicallyRejected=o.TechnicalStatus.Contains("رد",StringComparison.OrdinalIgnoreCase)||o.TechnicalStatus.Contains("نامنطبق",StringComparison.OrdinalIgnoreCase),
   IsDeliveryLate=o.DeliveryDays<=0||o.DeliveryDays>maxAllowedDays,Version=o.Version
  }).ToList();
  var active=rows.Where(x=>!x.IsExpired&&!x.IsTechnicallyRejected&&!x.IsDeliveryLate&&x.UnitPrice>0).ToList();
  var minPrice=active.Count==0?0:active.Min(x=>x.TotalPrice);var maxPrice=active.Count==0?0:active.Max(x=>x.TotalPrice);
  var minDelivery=active.Count==0?0:active.Min(x=>x.DeliveryDays);var maxDelivery=active.Count==0?0:active.Max(x=>x.DeliveryDays);
  foreach(var r in rows){
   var eligible=!r.IsExpired&&!r.IsTechnicallyRejected&&!r.IsDeliveryLate&&r.UnitPrice>0;
   r.PriceScore=!eligible||minPrice<=0?0:maxPrice==minPrice?30:(int)Math.Round(30m*minPrice/r.TotalPrice);
   r.DeliveryScore=!eligible||minDelivery<=0?0:maxDelivery==minDelivery?20:(int)Math.Round(20m*minDelivery/r.DeliveryDays);
   r.TechnicalScore=TechnicalScore(r.TechnicalStatus);r.EvidenceScore=EvidenceScore(r.EvidenceStatus);
   r.ValidityScore=r.IsExpired?0:r.ValidUntil.Date<=today.Date.AddDays(3)?5:10;
   if(r.IsExpired)r.Warnings.Add("اعتبار پیشنهاد منقضی شده است.");
   if(r.IsTechnicallyRejected)r.Warnings.Add("پیشنهاد از نظر فنی رد شده؛ امتیاز قیمت و تحویل برای آن محاسبه نمی‌شود.");
   else if(r.TechnicalScore<30)r.Warnings.Add("انطباق فنی هنوز توسط نقش مجاز تأیید نشده است.");
   if(r.IsDeliveryLate)r.Warnings.Add($"زمان تحویل پیشنهادی از موعد نیاز خرید بیشتر است (حداکثر {maxAllowedDays} روز).");
   if(r.EvidenceScore<10)r.Warnings.Add("مدارک یا شواهد تأمین‌کننده به‌طور مستقل کامل/تأیید نشده است.");
   if(r.PaymentTerms=="ثبت نشده")r.Warnings.Add("شرایط پرداخت ثبت نشده است.");else r.Warnings.Add("شرایط پرداخت باید با سیاست خریدار تطبیق داده شود؛ در امتیاز عددی دخالت داده نشده است.");
   r.Assessment=r.IsExpired?"منقضی‌شده":r.IsTechnicallyRejected?"رد فنی":r.IsDeliveryLate?"خارج از موعد موردنیاز":r.TotalScore>=80?"گزینه قابل بررسی":r.TotalScore>=60?"نیازمند بررسی بیشتر":"اطلاعات ناکافی / ریسک بالاتر";
  }
  return rows.OrderByDescending(x=>x.TotalScore).ThenBy(x=>x.TotalPrice).ToList();
 }
 private static int TechnicalScore(string s)=>s switch{"تأیید فنی"=>30,"نیازمند بررسی"=>15,"رد فنی"=>0,_=>0};
 private static int EvidenceScore(string s){if(s is "تأیید مستقل" or "مبتنی بر معامله")return 10;if(s is "استخراج‌شده از سند" or "مستندات دریافت‌شده")return 7;if(s=="اظهار تأمین‌کننده")return 4;return 0;}
}
