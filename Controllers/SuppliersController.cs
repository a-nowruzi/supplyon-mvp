using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _db;
    public SuppliersController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index(string? q) => View(await _db.Suppliers.Where(x => q == null || x.Name.Contains(q) || x.Capabilities.Contains(q)).OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> Search(string? q, string? itemCode, string? category, string? industrySegment, decimal? quantity, int? requirementId)
    {
        var model = new SupplierSearchViewModel
        {
            Query = q ?? "",
            ItemCode = itemCode ?? "",
            Category = category ?? "",
            IndustrySegment = industrySegment ?? "",
            Quantity = quantity,
            RequirementId = requirementId
        };

        if (requirementId.HasValue)
        {
            var requirement = await _db.PurchaseRequirements.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == requirementId.Value);
            if (requirement == null) return NotFound();
            model.RequirementCode = requirement.RequestCode;
            model.ItemName = requirement.Item?.Name;
            model.Quantity ??= requirement.Quantity;
            if (string.IsNullOrWhiteSpace(model.Query))
                model.Query = $"{requirement.Item?.Name} {requirement.Specifications} {requirement.Item?.TechnicalDescription}".Trim();
            if (string.IsNullOrWhiteSpace(model.ItemCode)) model.ItemCode = requirement.Item?.ItemCode ?? "";
            if (string.IsNullOrWhiteSpace(model.Category)) model.Category = requirement.Item?.Category ?? "";
            if (string.IsNullOrWhiteSpace(model.IndustrySegment)) model.IndustrySegment = requirement.Item?.IndustrySegment ?? "";
        }

        var hasCriteria = !string.IsNullOrWhiteSpace(model.Query) || !string.IsNullOrWhiteSpace(model.ItemCode)
            || !string.IsNullOrWhiteSpace(model.Category) || !string.IsNullOrWhiteSpace(model.IndustrySegment);
        if (hasCriteria)
        {
            var suppliers = await _db.Suppliers.AsNoTracking().ToListAsync();
            model.TotalSuppliers = suppliers.Count;
            model.Results = O2Ban.EnterprisePilot.Services.SupplierSearchEngine.Search(
                suppliers, model.Query, model.ItemCode, model.Category, model.IndustrySegment);
            var supplierIds = model.Results.Select(x => x.Supplier.Id).ToList();
            var historicalScores = await _db.SupplierPerformanceEvaluations
                .Where(x => supplierIds.Contains(x.Order!.SupplierId))
                .Select(x => new { SupplierId = x.Order!.SupplierId, x.OverallScore })
                .ToListAsync();
            var performanceBySupplier = historicalScores.GroupBy(x => x.SupplierId)
                .ToDictionary(g => g.Key, g => new { Average = Math.Round(g.Average(x => x.OverallScore), 2), Count = g.Count() });
            foreach (var result in model.Results)
            {
                if (performanceBySupplier.TryGetValue(result.Supplier.Id, out var history))
                {
                    result.HistoricalPerformanceScore = history.Average;
                    result.CompletedEvaluatedOrders = history.Count;
                    result.PerformanceHistoryLabel = $"{history.Count} سفارش ارزیابی‌شده";
                }
            }
        }
        return View(model);
    }

    [Authorize(Roles="Admin,Procurement")] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin,Procurement")]
    public async Task<IActionResult> Create(Supplier model)
    {
        if (!ModelState.IsValid) return View(model);
        model.IsDemoData = false; _db.Add(model); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}
