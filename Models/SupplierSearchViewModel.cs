namespace O2Ban.EnterprisePilot.Models;

public class SupplierSearchViewModel
{
    public string Query { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public string Category { get; set; } = "";
    public string IndustrySegment { get; set; } = "";
    public decimal? Quantity { get; set; }
    public int? RequirementId { get; set; }
    public string? RequirementCode { get; set; }
    public string? ItemName { get; set; }
    public List<SupplierSearchResult> Results { get; set; } = new();
    public int TotalSuppliers { get; set; }
}

public class SupplierSearchResult
{
    public Supplier Supplier { get; set; } = new();
    public int RelevanceScore { get; set; }
    public string RelevanceLabel { get; set; } = "";
    public List<string> MatchReasons { get; set; } = new();
    public List<string> DataGaps { get; set; } = new();
    public string EvidenceInterpretation { get; set; } = "";
    public decimal? HistoricalPerformanceScore { get; set; }
    public int CompletedEvaluatedOrders { get; set; }
    public string PerformanceHistoryLabel { get; set; } = "فاقد سابقه ارزیابی";
}
