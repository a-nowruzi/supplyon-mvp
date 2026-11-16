using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize(Roles = "Admin")]
public class ValidationReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public ValidationReportsController(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var start = (from ?? DateTime.UtcNow.Date.AddDays(-30)).Date;
        var end = (to ?? DateTime.UtcNow.Date).Date;
        if (end < start) (start, end) = (end, start);
        var endExclusive = end.AddDays(1);
        var reqs = _db.PurchaseRequirements.Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive);
        var offers = _db.SupplierOffers.Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive);
        var evidences = _db.EvidenceRecords.Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive);
        var feedback = _db.UserFeedbacks.Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive);
        var model = new ValidationReportViewModel
        {
            From = start, To = end,
            ActiveUsers = await _users.Users.CountAsync(),
            RequirementsCreated = await reqs.CountAsync(),
            OffersSubmitted = await offers.CountAsync(),
            SuppliersRegistered = await _db.Suppliers.CountAsync(),
            EvidenceTotal = await evidences.CountAsync(),
            EvidencePending = await evidences.CountAsync(x => x.Status == "در انتظار بررسی" || x.Status == "نیازمند به‌روزرسانی"),
            EvidenceVerified = await evidences.CountAsync(x => x.Status == "تأیید مستقل" || x.Status == "مبتنی بر معامله"),
            EvidenceRejected = await evidences.CountAsync(x => x.Status == "ردشده"),
            FeedbackCount = await feedback.CountAsync(),
            OrdersCreated = await _db.ProcurementOrders.CountAsync(x => x.CreatedAt >= start && x.CreatedAt < endExclusive),
            OrdersDelivered = await _db.ProcurementOrders.CountAsync(x => x.ActualDeliveryDate >= start && x.ActualDeliveryDate < endExclusive && x.DeliveryStatus == "تحویل کامل"),
            OrdersEvaluated = await _db.SupplierPerformanceEvaluations.CountAsync(x => x.EvaluatedAt >= start && x.EvaluatedAt < endExclusive),
            AverageSupplierPerformance = await _db.SupplierPerformanceEvaluations.Where(x => x.EvaluatedAt >= start && x.EvaluatedAt < endExclusive).Select(x => (decimal?)x.OverallScore).AverageAsync(),
            AverageRating = await feedback.Select(x => (double?)x.Rating).AverageAsync(),
            FeedbackByArea = await feedback.GroupBy(x => x.Area).Select(g => new FeedbackAreaSummary { Area = g.Key, Count = g.Count(), AverageRating = g.Average(x => x.Rating) }).OrderByDescending(x => x.Count).ToListAsync(),
            EvidenceByStatus = await evidences.GroupBy(x => x.Status).Select(g => new EvidenceStatusSummary { Status = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToListAsync(),
            RecentFeedback = await feedback.OrderByDescending(x => x.CreatedAt).Take(10).Select(x => new RecentFeedbackRow { Area = x.Area, Rating = x.Rating, Comment = x.Comment, CreatedAt = x.CreatedAt }).ToListAsync()
        };
        return View(model);
    }
}
