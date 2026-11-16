using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Items = await _db.CatalogItems.CountAsync();
        ViewBag.Suppliers = await _db.Suppliers.CountAsync();
        ViewBag.Requirements = await _db.PurchaseRequirements.CountAsync();
        ViewBag.Offers = await _db.SupplierOffers.CountAsync();
        ViewBag.Recent = await _db.PurchaseRequirements.Include(x => x.Item).OrderByDescending(x => x.CreatedAt).Take(6).ToListAsync();
        return View();
    }
    [AllowAnonymous] public IActionResult AccessDenied() => View();
    [AllowAnonymous] public IActionResult Error() => View();
}
