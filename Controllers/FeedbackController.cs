using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class FeedbackController : Controller
{
    private readonly ApplicationDbContext _db;
    public FeedbackController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.UserFeedbacks.OrderByDescending(x => x.CreatedAt).ToListAsync());
    public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFeedback model)
    {
        ModelState.Remove(nameof(model.UserId));
        model.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        if (!ModelState.IsValid) return View(model);
        _db.Add(model); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}
