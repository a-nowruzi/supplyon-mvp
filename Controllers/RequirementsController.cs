using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class RequirementsController : Controller
{
    private readonly ApplicationDbContext _db;
    public RequirementsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.PurchaseRequirements.Include(x => x.Item).OrderByDescending(x => x.CreatedAt).ToListAsync());
    public async Task<IActionResult> Create()
    {
        ViewBag.Items = new SelectList(await _db.CatalogItems.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        return View();
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseRequirement model)
    {
        model.RequestCode = "REQ-" + DateTime.UtcNow.ToString("yyMMddHHmmss");
        ModelState.Remove(nameof(model.RequestCode));
        ModelState.Remove(nameof(model.CreatedByUserId));
        model.CreatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        model.CreatedAt = DateTime.UtcNow;
        if (!ModelState.IsValid)
        {
            ViewBag.Items = new SelectList(await _db.CatalogItems.ToListAsync(), "Id", "Name", model.CatalogItemId);
            return View(model);
        }
        _db.Add(model); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Offers(int id)
    {
        var req = await _db.PurchaseRequirements.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (req == null) return NotFound();
        ViewBag.Suppliers = new SelectList(await _db.Suppliers.ToListAsync(), "Id", "Name");
        ViewBag.Requirement = req;
        return View(await _db.SupplierOffers.Include(x => x.Supplier).Where(x => x.PurchaseRequirementId == id).OrderBy(x => x.UnitPrice).ToListAsync());
    }

    public async Task<IActionResult> Compare(int id)
    {
        var requirement = await _db.PurchaseRequirements
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (requirement == null) return NotFound();

        // Compare only the latest version from each supplier; older versions remain stored.
        var allOffers = await _db.SupplierOffers
            .Include(x => x.Supplier)
            .Where(x => x.PurchaseRequirementId == id)
            .ToListAsync();
        var latestOffers = allOffers
            .GroupBy(x => x.SupplierId)
            .Select(g => g.OrderByDescending(x => x.Version).ThenByDescending(x => x.CreatedAt).First())
            .ToList();

        var model = new OfferComparisonViewModel
        {
            Requirement = requirement,
            Offers = O2Ban.EnterprisePilot.Services.OfferComparisonEngine.Compare(
                requirement, latestOffers, DateTime.UtcNow.Date)
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOffer(int requirementId, int supplierId, decimal unitPrice, int deliveryDays, string paymentTerms, DateTime validUntil, string notes)
    {
        if (unitPrice <= 0 || deliveryDays <= 0 || validUntil.Date < DateTime.UtcNow.Date) return BadRequest("قیمت، زمان تحویل و تاریخ اعتبار پیشنهاد معتبر نیستند.");
        if (!await _db.PurchaseRequirements.AnyAsync(x => x.Id == requirementId) || !await _db.Suppliers.AnyAsync(x => x.Id == supplierId)) return NotFound();
        if (unitPrice <= 0 || deliveryDays <= 0 || validUntil.Date < DateTime.UtcNow.Date) return BadRequest("قیمت، زمان تحویل و تاریخ اعتبار پیشنهاد معتبر نیستند.");
        if (!await _db.PurchaseRequirements.AnyAsync(x => x.Id == requirementId) || !await _db.Suppliers.AnyAsync(x => x.Id == supplierId)) return NotFound();
        var exists = await _db.SupplierOffers.Where(x => x.PurchaseRequirementId == requirementId && x.SupplierId == supplierId).OrderByDescending(x => x.Version).FirstOrDefaultAsync();
        _db.SupplierOffers.Add(new SupplierOffer { PurchaseRequirementId = requirementId, SupplierId = supplierId, UnitPrice = unitPrice, DeliveryDays = deliveryDays, PaymentTerms = paymentTerms ?? "", ValidUntil = validUntil, Notes = notes ?? "", Version = exists == null ? 1 : exists.Version + 1, TechnicalStatus = "نیازمند بررسی" });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Offers), new { id = requirementId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewTechnical(int offerId, string decision)
    {
        if (decision is not ("تأیید فنی" or "رد فنی" or "نیازمند بررسی")) return BadRequest();
        var offer = await _db.SupplierOffers.FindAsync(offerId);
        if (offer is null) return NotFound();
        var newer = await _db.SupplierOffers.AnyAsync(x => x.PurchaseRequirementId == offer.PurchaseRequirementId && x.SupplierId == offer.SupplierId && x.Version > offer.Version);
        if (newer) return BadRequest("فقط آخرین نسخه پیشنهاد قابل بررسی است.");
        offer.TechnicalStatus = decision; await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Offers), new { id = offer.PurchaseRequirementId });
    }
}
