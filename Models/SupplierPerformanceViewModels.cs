using System.ComponentModel.DataAnnotations;

namespace O2Ban.EnterprisePilot.Models;

public class SupplierPerformanceIndexViewModel
{
    public int? SupplierId { get; set; }
    public List<ProcurementOrder> Orders { get; set; } = new();
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int EvaluatedOrders { get; set; }
}

public class DeliveryUpdateViewModel
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = "";
    public decimal OrderedQuantity { get; set; }
    [Required] public string DeliveryStatus { get; set; } = "در انتظار تحویل";
    [DataType(DataType.Date)] public DateTime? ActualDeliveryDate { get; set; }
    [Range(typeof(decimal), "0", "999999999")] public decimal ReceivedQuantity { get; set; }
    [Range(typeof(decimal), "0", "999999999")] public decimal AcceptedQuantity { get; set; }
    [Range(typeof(decimal), "0", "999999999")] public decimal RejectedQuantity { get; set; }
    [MaxLength(1200)] public string DeliveryNotes { get; set; } = "";
}

public class SupplierPerformanceEvaluationViewModel
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public decimal OrderedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    [Range(1, 5)] public int QualityRating { get; set; } = 3;
    [Range(1, 5)] public int CommunicationRating { get; set; } = 3;
    [Range(1, 5)] public int DocumentationRating { get; set; } = 3;
    [MaxLength(1500)] public string Comments { get; set; } = "";
}

public class SupplierPerformanceProfileViewModel
{
    public Supplier Supplier { get; set; } = new();
    public int TotalOrders { get; set; }
    public int EvaluatedOrders { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? OnTimeRate { get; set; }
    public decimal? AcceptanceRate { get; set; }
    public decimal? AverageQualityRating { get; set; }
    public List<ProcurementOrder> RecentOrders { get; set; } = new();
    public List<SupplierPerformanceEvaluation> RecentEvaluations { get; set; } = new();
}
