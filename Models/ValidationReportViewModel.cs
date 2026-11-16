namespace O2Ban.EnterprisePilot.Models;

public class ValidationReportViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int ActiveUsers { get; set; }
    public int RequirementsCreated { get; set; }
    public int OffersSubmitted { get; set; }
    public int SuppliersRegistered { get; set; }
    public int EvidenceTotal { get; set; }
    public int EvidencePending { get; set; }
    public int EvidenceVerified { get; set; }
    public int EvidenceRejected { get; set; }
    public int FeedbackCount { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersDelivered { get; set; }
    public int OrdersEvaluated { get; set; }
    public decimal? AverageSupplierPerformance { get; set; }
    public double? AverageRating { get; set; }
    public List<FeedbackAreaSummary> FeedbackByArea { get; set; } = new();
    public List<EvidenceStatusSummary> EvidenceByStatus { get; set; } = new();
    public List<RecentFeedbackRow> RecentFeedback { get; set; } = new();
}
public class FeedbackAreaSummary { public string Area { get; set; } = ""; public int Count { get; set; } public double AverageRating { get; set; } }
public class EvidenceStatusSummary { public string Status { get; set; } = ""; public int Count { get; set; } }
public class RecentFeedbackRow { public string Area { get; set; } = ""; public int Rating { get; set; } public string Comment { get; set; } = ""; public DateTime CreatedAt { get; set; } }
