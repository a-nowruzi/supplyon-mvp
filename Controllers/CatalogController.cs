using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class CatalogController : Controller
{
    private readonly ApplicationDbContext _db;
    public CatalogController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index(string? q) => View(await _db.CatalogItems.Where(x => q == null || x.Name.Contains(q) || x.ItemCode.Contains(q)).OrderBy(x => x.Name).ToListAsync());
    [Authorize(Roles="Admin")] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin")]
    public async Task<IActionResult> Create(CatalogItem model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}
