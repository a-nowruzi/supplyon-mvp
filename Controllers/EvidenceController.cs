using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Data;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Controllers;

[Authorize]
public class EvidenceController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".xlsx" };
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly string[] Statuses = { "در انتظار بررسی", "اظهار تأمین‌کننده", "استخراج‌شده از سند", "تأیید مستقل", "مبتنی بر معامله", "نیازمند به‌روزرسانی", "متناقض", "مشکوک", "ردشده" };

    public EvidenceController(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public async Task<IActionResult> Index(string? status, int? supplierId)
    {
        var query = _db.EvidenceRecords.Include(x => x.Supplier).Include(x => x.PurchaseRequirement).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        if (supplierId.HasValue) query = query.Where(x => x.SupplierId == supplierId);
        ViewBag.Status = status;
        ViewBag.SupplierId = supplierId;
        ViewBag.Suppliers = new SelectList(await _db.Suppliers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", supplierId);
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new EvidenceRecord());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EvidenceRecord model, IFormFile? file)
    {
        ModelState.Remove(nameof(model.UploadedByUserId)); ModelState.Remove(nameof(model.StoredFileName));
        ModelState.Remove(nameof(model.OriginalFileName)); ModelState.Remove(nameof(model.ContentType));
        ModelState.Remove(nameof(model.Supplier)); ModelState.Remove(nameof(model.PurchaseRequirement));
        if (!model.SupplierId.HasValue && !model.PurchaseRequirementId.HasValue)
            ModelState.AddModelError(nameof(model.SupplierId), "مدرک باید به یک تأمین‌کننده یا درخواست خرید مرتبط باشد.");
        if (file is not null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext)) ModelState.AddModelError("file", "فرمت فایل مجاز نیست. فرمت‌های مجاز: PDF، PNG، JPG، DOCX و XLSX.");
            if (file.Length > MaxFileSize) ModelState.AddModelError("file", "حداکثر حجم فایل ۱۰ مگابایت است.");
        }
        if (!ModelState.IsValid) { await LoadLists(model.SupplierId, model.PurchaseRequirementId); return View(model); }

        model.Status = "در انتظار بررسی";
        model.UploadedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        model.CreatedAt = DateTime.UtcNow;
        if (file is not null && file.Length > 0)
        {
            var folder = Path.Combine(_env.ContentRootPath, "App_Data", "Evidence");
            Directory.CreateDirectory(folder);
            var stored = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var fullPath = Path.Combine(folder, stored);
            await using (var stream = System.IO.File.Create(fullPath)) await file.CopyToAsync(stream);
            model.OriginalFileName = Path.GetFileName(file.FileName);
            model.StoredFileName = stored;
            model.ContentType = file.ContentType;
            model.FileSizeBytes = file.Length;
        }
        _db.EvidenceRecords.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.EvidenceRecords.Include(x => x.Supplier).Include(x => x.PurchaseRequirement).FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, string status, string reviewComment, DateTime? expiresAt)
    {
        if (!Statuses.Contains(status)) return BadRequest();
        var item = await _db.EvidenceRecords.FindAsync(id);
        if (item is null) return NotFound();
        item.Status = status;
        item.ReviewComment = reviewComment?.Trim() ?? "";
        item.ReviewedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        item.ReviewedAt = DateTime.UtcNow;
        item.ExpiresAt = expiresAt;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Download(int id)
    {
        var item = await _db.EvidenceRecords.FindAsync(id);
        if (item is null || string.IsNullOrWhiteSpace(item.StoredFileName)) return NotFound();
        var path = Path.Combine(_env.ContentRootPath, "App_Data", "Evidence", item.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, string.IsNullOrWhiteSpace(item.ContentType) ? "application/octet-stream" : item.ContentType, item.OriginalFileName);
    }

    private async Task LoadLists(int? supplierId = null, int? requirementId = null)
    {
        ViewBag.Suppliers = new SelectList(await _db.Suppliers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", supplierId);
        ViewBag.Requirements = new SelectList(await _db.PurchaseRequirements.Include(x => x.Item).OrderByDescending(x => x.CreatedAt).Select(x => new { x.Id, Label = x.RequestCode + " - " + x.Item!.Name }).ToListAsync(), "Id", "Label", requirementId);
    }
}
