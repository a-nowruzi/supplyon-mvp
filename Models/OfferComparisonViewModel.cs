namespace O2Ban.EnterprisePilot.Models;

public class OfferComparisonViewModel
{
    public PurchaseRequirement Requirement { get; set; } = new();
    public List<OfferComparisonRow> Offers { get; set; } = new();
    public int OfferCount => Offers.Count;
    public int ExpiredCount => Offers.Count(x => x.IsExpired);
}

public class OfferComparisonRow
{
    public int OfferId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = "";
    public string EvidenceStatus { get; set; } = "نامشخص";
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int DeliveryDays { get; set; }
    public string PaymentTerms { get; set; } = "ثبت نشده";
    public DateTime ValidUntil { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDeliveryLate { get; set; }
    public bool IsTechnicallyRejected { get; set; }
    public string TechnicalStatus { get; set; } = "نیازمند بررسی";
    public int Version { get; set; }
    public int PriceScore { get; set; }
    public int DeliveryScore { get; set; }
    public int TechnicalScore { get; set; }
    public int EvidenceScore { get; set; }
    public int ValidityScore { get; set; }
    public int TotalScore => PriceScore + DeliveryScore + TechnicalScore + EvidenceScore + ValidityScore;
    public string Assessment { get; set; } = "نیازمند بررسی";
    public List<string> Warnings { get; set; } = new();
}
