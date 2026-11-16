using System.ComponentModel.DataAnnotations;

namespace O2Ban.EnterprisePilot.Models;

public class CatalogItem
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string ItemCode { get; set; } = "";
    [Required, MaxLength(180)] public string Name { get; set; } = "";
    [MaxLength(100)] public string IndustrySegment { get; set; } = "";
    [MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(500)] public string TechnicalDescription { get; set; } = "";
    [MaxLength(30)] public string Unit { get; set; } = "عدد";
}

public class Supplier
{
    public int Id { get; set; }
    [Required, MaxLength(180)] public string Name { get; set; } = "";
    [MaxLength(30)] public string NationalId { get; set; } = "";
    [MaxLength(120)] public string City { get; set; } = "";
    [MaxLength(250)] public string Capabilities { get; set; } = "";
    [MaxLength(40)] public string EvidenceStatus { get; set; } = "اظهار تأمین‌کننده";
    [MaxLength(250)] public string ContactName { get; set; } = "";
    public bool IsDemoData { get; set; } = true;
}

public class PurchaseRequirement
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string RequestCode { get; set; } = "";
    public int CatalogItemId { get; set; }
    public CatalogItem? Item { get; set; }
    public decimal Quantity { get; set; }
    public DateTime NeededBy { get; set; }
    [MaxLength(500)] public string Specifications { get; set; } = "";
    [MaxLength(30)] public string Status { get; set; } = "ثبت اولیه";
    public string CreatedByUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SupplierOffer
{
    public int Id { get; set; }
    public int PurchaseRequirementId { get; set; }
    public PurchaseRequirement? Requirement { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal UnitPrice { get; set; }
    public int DeliveryDays { get; set; }
    [MaxLength(180)] public string PaymentTerms { get; set; } = "";
    public DateTime ValidUntil { get; set; }
    [MaxLength(30)] public string TechnicalStatus { get; set; } = "نیازمند بررسی";
    [MaxLength(500)] public string Notes { get; set; } = "";
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UserFeedback
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    [MaxLength(60)] public string Area { get; set; } = "";
    [Range(1, 5)] public int Rating { get; set; }
    [Required, MaxLength(1500)] public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class EvidenceRecord
{
    public int Id { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    [Required, MaxLength(50)] public string EvidenceType { get; set; } = "مدرک فنی";
    [Required, MaxLength(40)] public string SourceType { get; set; } = "اظهار تأمین‌کننده";
    [Required, MaxLength(40)] public string Status { get; set; } = "در انتظار بررسی";
    [MaxLength(1200)] public string Description { get; set; } = "";
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int? PurchaseRequirementId { get; set; }
    public PurchaseRequirement? PurchaseRequirement { get; set; }
    [MaxLength(260)] public string OriginalFileName { get; set; } = "";
    [MaxLength(100)] public string StoredFileName { get; set; } = "";
    [MaxLength(120)] public string ContentType { get; set; } = "";
    public long FileSizeBytes { get; set; }
    [MaxLength(450)] public string UploadedByUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(450)] public string ReviewedByUserId { get; set; } = "";
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(1000)] public string ReviewComment { get; set; } = "";
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>ثبت سفارش آزمایشی حاصل از پذیرش یک پیشنهاد تأییدشده.</summary>
public class ProcurementOrder
{
    public int Id { get; set; }
    [Required, MaxLength(40)] public string OrderCode { get; set; } = "";
    public int PurchaseRequirementId { get; set; }
    public PurchaseRequirement? Requirement { get; set; }
    public int SupplierOfferId { get; set; }
    public SupplierOffer? Offer { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    [Required, MaxLength(40)] public string DeliveryStatus { get; set; } = "در انتظار تحویل";
    [MaxLength(1200)] public string DeliveryNotes { get; set; } = "";
    [MaxLength(450)] public string CreatedByUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public SupplierPerformanceEvaluation? Evaluation { get; set; }
}

/// <summary>ارزیابی عملکرد مبتنی بر یک سفارش تحویل‌شده؛ هر سفارش فقط یک ارزیابی نهایی دارد.</summary>
public class SupplierPerformanceEvaluation
{
    public int Id { get; set; }
    public int ProcurementOrderId { get; set; }
    public ProcurementOrder? Order { get; set; }
    [Range(1, 5)] public int QualityRating { get; set; }
    [Range(1, 5)] public int CommunicationRating { get; set; }
    [Range(1, 5)] public int DocumentationRating { get; set; }
    [MaxLength(1500)] public string Comments { get; set; } = "";
    [MaxLength(450)] public string EvaluatedByUserId { get; set; } = "";
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public decimal OverallScore { get; set; }
    public int DeliveryScore { get; set; }
    public decimal FulfillmentRate { get; set; }
}
