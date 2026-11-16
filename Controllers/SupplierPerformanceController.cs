using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;
using O2Ban.EnterprisePilot.Services;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class SupplierPerformanceController : Controller
{
    private readonly ApplicationDbContext _db;
    public SupplierPerformanceController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? supplierId)
    {
        var query = _db.ProcurementOrders
            .Include(x => x.Supplier)
            .Include(x => x.Requirement).ThenInclude(x => x!.Item)
            .Include(x => x.Evaluation)
            .AsQueryable();
        if (supplierId.HasValue) query = query.Where(x => x.SupplierId == supplierId.Value);
        var orders = await query.OrderByDescending(x => x.OrderDate).ToListAsync();
        return View(new SupplierPerformanceIndexViewModel
        {
            SupplierId = supplierId,
            Orders = orders,
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(x => x.DeliveryStatus == "در انتظار تحویل" || x.DeliveryStatus == "تحویل جزئی"),
            CompletedOrders = orders.Count(x => x.DeliveryStatus == "تحویل کامل"),
            EvaluatedOrders = orders.Count(x => x.Evaluation != null)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(int offerId)
    {
        var offer = await _db.SupplierOffers
            .Include(x => x.Supplier)
            .Include(x => x.Requirement)
            .FirstOrDefaultAsync(x => x.Id == offerId);
        if (offer == null) return NotFound();
        if (offer.TechnicalStatus != "تأیید فنی")
        {
            TempData["PerformanceError"] = "برای تبدیل پیشنهاد به سفارش، ابتدا باید وضعیت فنی آن توسط کارشناس تأیید شود.";
            return RedirectToAction("Offers", "Requirements", new { id = offer.PurchaseRequirementId });
        }
        if (offer.ValidUntil.Date < DateTime.UtcNow.Date)
        {
            TempData["PerformanceError"] = "اعتبار این پیشنهاد پایان یافته و امکان تبدیل آن به سفارش وجود ندارد.";
            return RedirectToAction("Offers", "Requirements", new { id = offer.PurchaseRequirementId });
        }
        var newerVersionExists = await _db.SupplierOffers.AnyAsync(x =>
            x.PurchaseRequirementId == offer.PurchaseRequirementId && x.SupplierId == offer.SupplierId && x.Version > offer.Version);
        if (newerVersionExists)
        {
            TempData["PerformanceError"] = "این نسخه پیشنهاد، نسخه جدیدتری دارد و قابل تبدیل به سفارش نیست.";
            return RedirectToAction("Offers", "Requirements", new { id = offer.PurchaseRequirementId });
        }
        if (await _db.ProcurementOrders.AnyAsync(x => x.SupplierOfferId == offerId))
        {
            TempData["PerformanceError"] = "برای این نسخه پیشنهاد قبلاً سفارش ثبت شده است.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        var order = new ProcurementOrder
        {
            OrderCode = "PO-" + now.ToString("yyMMddHHmmssfff"),
            PurchaseRequirementId = offer.PurchaseRequirementId,
            SupplierOfferId = offer.Id,
            SupplierId = offer.SupplierId,
            OrderedQuantity = offer.Requirement?.Quantity ?? 0,
            UnitPrice = offer.UnitPrice,
            OrderDate = now.Date,
            ExpectedDeliveryDate = now.Date.AddDays(Math.Max(1, offer.DeliveryDays)),
            DeliveryStatus = "در انتظار تحویل",
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            CreatedAt = now
        };
        _db.ProcurementOrders.Add(order);
        await _db.SaveChangesAsync();
        TempData["PerformanceSuccess"] = $"سفارش {order.OrderCode} ثبت شد. پس از دریافت کالا، اطلاعات تحویل را تکمیل کنید.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.ProcurementOrders
            .Include(x => x.Supplier)
            .Include(x => x.Requirement).ThenInclude(x => x!.Item)
            .Include(x => x.Offer)
            .Include(x => x.Evaluation)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDelivery(DeliveryUpdateViewModel model)
    {
        var order = await _db.ProcurementOrders.FirstOrDefaultAsync(x => x.Id == model.OrderId);
        if (order == null) return NotFound();
        var allowedStatuses = new[] { "در انتظار تحویل", "تحویل جزئی", "تحویل کامل", "لغوشده" };
        if (!allowedStatuses.Contains(model.DeliveryStatus)) ModelState.AddModelError(nameof(model.DeliveryStatus), "وضعیت تحویل معتبر نیست.");
        if (model.ReceivedQuantity > order.OrderedQuantity) ModelState.AddModelError(nameof(model.ReceivedQuantity), "مقدار دریافتی نمی‌تواند از مقدار سفارش بیشتر باشد.");
        if (model.AcceptedQuantity + model.RejectedQuantity > model.ReceivedQuantity) ModelState.AddModelError(nameof(model.AcceptedQuantity), "مجموع مقدار تأییدشده و مردود نمی‌تواند از مقدار دریافتی بیشتر باشد.");
        if (model.DeliveryStatus == "تحویل کامل" && model.ReceivedQuantity != order.OrderedQuantity) ModelState.AddModelError(nameof(model.ReceivedQuantity), "برای ثبت تحویل کامل، مقدار دریافتی باید با مقدار سفارش برابر باشد.");
        if (model.DeliveryStatus == "تحویل کامل" && model.AcceptedQuantity + model.RejectedQuantity != model.ReceivedQuantity) ModelState.AddModelError(nameof(model.AcceptedQuantity), "برای نهایی‌کردن تحویل، تمام مقدار دریافتی باید به‌عنوان تأییدشده یا مردود تعیین تکلیف شود.");
        if ((model.DeliveryStatus == "تحویل جزئی" || model.DeliveryStatus == "تحویل کامل") && !model.ActualDeliveryDate.HasValue) ModelState.AddModelError(nameof(model.ActualDeliveryDate), "برای تحویل جزئی یا کامل، تاریخ دریافت الزامی است.");
        if (order.Evaluation != null && (model.DeliveryStatus != "تحویل کامل" || model.ReceivedQuantity != order.ReceivedQuantity || model.AcceptedQuantity != order.AcceptedQuantity || model.RejectedQuantity != order.RejectedQuantity || model.ActualDeliveryDate != order.ActualDeliveryDate))
            ModelState.AddModelError("", "این سفارش ارزیابی نهایی دارد. برای حفظ اعتبار سابقه، پس از ارزیابی امکان تغییر اطلاعات تحویل وجود ندارد.");

        if (!ModelState.IsValid)
        {
            return View("Details", await _db.ProcurementOrders.Include(x => x.Supplier).Include(x => x.Requirement).ThenInclude(x => x!.Item).Include(x => x.Offer).Include(x => x.Evaluation).FirstAsync(x => x.Id == order.Id));
        }

        order.DeliveryStatus = model.DeliveryStatus;
        order.ActualDeliveryDate = model.ActualDeliveryDate?.Date;
        order.ReceivedQuantity = model.ReceivedQuantity;
        order.AcceptedQuantity = model.AcceptedQuantity;
        order.RejectedQuantity = model.RejectedQuantity;
        order.DeliveryNotes = model.DeliveryNotes ?? "";
        await _db.SaveChangesAsync();
        TempData["PerformanceSuccess"] = "اطلاعات تحویل سفارش ذخیره شد.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> Evaluate(int id)
    {
        var order = await _db.ProcurementOrders.Include(x => x.Supplier).Include(x => x.Evaluation).FirstOrDefaultAsync(x => x.Id == id);
        if (order == null) return NotFound();
        if (order.DeliveryStatus != "تحویل کامل" || !order.ActualDeliveryDate.HasValue)
        {
            TempData["PerformanceError"] = "ارزیابی نهایی پس از ثبت تحویل کامل و تاریخ دریافت امکان‌پذیر است.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (order.Evaluation != null)
        {
            TempData["PerformanceError"] = "برای این سفارش ارزیابی نهایی ثبت شده است و در این نسخه قابل بازنویسی نیست.";
            return RedirectToAction(nameof(Details), new { id });
        }
        var model = new SupplierPerformanceEvaluationViewModel
        {
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            SupplierName = order.Supplier?.Name ?? "",
            OrderedQuantity = order.OrderedQuantity,
            AcceptedQuantity = order.AcceptedQuantity,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            ActualDeliveryDate = order.ActualDeliveryDate,
            QualityRating = order.Evaluation?.QualityRating ?? 3,
            CommunicationRating = order.Evaluation?.CommunicationRating ?? 3,
            DocumentationRating = order.Evaluation?.DocumentationRating ?? 3,
            Comments = order.Evaluation?.Comments ?? ""
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Evaluate(SupplierPerformanceEvaluationViewModel model)
    {
        var order = await _db.ProcurementOrders.Include(x => x.Evaluation).FirstOrDefaultAsync(x => x.Id == model.OrderId);
        if (order == null) return NotFound();
        if (order.DeliveryStatus != "تحویل کامل" || !order.ActualDeliveryDate.HasValue)
            ModelState.AddModelError("", "ابتدا اطلاعات تحویل کامل سفارش را ثبت کنید.");
        if (order.Evaluation != null)
            ModelState.AddModelError("", "برای این سفارش ارزیابی نهایی قبلاً ثبت شده و قابل بازنویسی نیست.");
        if (!ModelState.IsValid)
        {
            model.OrderCode = order.OrderCode;
            model.SupplierName = await _db.Suppliers.Where(x => x.Id == order.SupplierId).Select(x => x.Name).FirstOrDefaultAsync() ?? "";
            model.OrderedQuantity = order.OrderedQuantity;
            model.AcceptedQuantity = order.AcceptedQuantity;
            model.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
            model.ActualDeliveryDate = order.ActualDeliveryDate;
            return View(model);
        }
        var score = SupplierPerformanceCalculator.Calculate(order, model.QualityRating, model.CommunicationRating, model.DocumentationRating);
        if (order.Evaluation == null)
        {
            order.Evaluation = new SupplierPerformanceEvaluation { ProcurementOrderId = order.Id };
            _db.SupplierPerformanceEvaluations.Add(order.Evaluation);
        }
        order.Evaluation.QualityRating = model.QualityRating;
        order.Evaluation.CommunicationRating = model.CommunicationRating;
        order.Evaluation.DocumentationRating = model.DocumentationRating;
        order.Evaluation.Comments = model.Comments ?? "";
        order.Evaluation.EvaluatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        order.Evaluation.EvaluatedAt = DateTime.UtcNow;
        order.Evaluation.OverallScore = score.OverallScore;
        order.Evaluation.DeliveryScore = score.DeliveryScore;
        order.Evaluation.FulfillmentRate = score.FulfillmentRate;
        await _db.SaveChangesAsync();
        TempData["PerformanceSuccess"] = "ارزیابی عملکرد تأمین‌کننده ثبت شد و در پروفایل عملکرد او لحاظ می‌شود.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> SupplierProfile(int supplierId)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == supplierId);
        if (supplier == null) return NotFound();
        var orders = await _db.ProcurementOrders.Include(x => x.Evaluation)
            .Where(x => x.SupplierId == supplierId).OrderByDescending(x => x.OrderDate).ToListAsync();
        var delivered = orders.Where(x => x.DeliveryStatus == "تحویل کامل" && x.ActualDeliveryDate.HasValue).ToList();
        var evaluated = delivered.Where(x => x.Evaluation != null).Select(x => x.Evaluation!).ToList();
        var viewModel = new SupplierPerformanceProfileViewModel
        {
            Supplier = supplier,
            TotalOrders = orders.Count,
            EvaluatedOrders = evaluated.Count,
            AverageScore = evaluated.Count == 0 ? null : Math.Round(evaluated.Average(x => x.OverallScore), 2),
            OnTimeRate = delivered.Count == 0 ? null : Math.Round(delivered.Count(x => x.ActualDeliveryDate!.Value.Date <= x.ExpectedDeliveryDate.Date) * 100m / delivered.Count, 2),
            AcceptanceRate = delivered.Sum(x => x.ReceivedQuantity) <= 0 ? null : Math.Round(delivered.Sum(x => x.AcceptedQuantity) * 100m / delivered.Sum(x => x.ReceivedQuantity), 2),
            AverageQualityRating = evaluated.Count == 0 ? null : Math.Round(evaluated.Average(x => (decimal)x.QualityRating), 2),
            RecentOrders = orders.Take(10).ToList(),
            RecentEvaluations = evaluated.OrderByDescending(x => x.EvaluatedAt).Take(10).ToList()
        };
        return View(viewModel);
    }

}
