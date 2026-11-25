using Microsoft.EntityFrameworkCore;
using O2Ban.ValidationWorkspace.Models;
namespace O2Ban.ValidationWorkspace.Data;

public static class LookupSeeder
{
    private static readonly Dictionary<string, string[]> Values = new()
    {
        ["product-category"] = new[] { "قطعات فلزی تولیدی", "قطعات پلیمری تولیدی", "قطعات الکتریکی تولیدی", "قطعات الکترونیکی تولیدی", "قطعات مکانیکی تولیدی", "ملزومات نگهداری غیرتولیدی", "ابزار و مصرفی کارگاه", "ایمنی و حفاظت فردی", "بسته‌بندی غیرتولیدی", "ابزار اندازه‌گیری", "روانکار و نگهداری", "تجهیزات تعمیرگاه" },
        ["unit"] = new[] { "عدد", "دستگاه", "بسته", "جفت", "کیلوگرم", "گرم", "تن", "متر", "مترمربع", "مترمکعب", "لیتر", "گالن", "رول", "شاخه", "ست", "حلقه", "کارتن" },
        ["priority"] = new[] { "عادی", "زیاد", "فوری" },
        ["evidence-status"] = new[] { "اعلام‌شده", "دارای مدرک", "تأییدشده توسط شرکت", "نیازمند به‌روزرسانی", "دارای تناقض", "مشکوک", "ردشده" },
        ["supplier-segment"] = new[] { "داخلی / تولیدی", "داخلی / غیرتولیدی", "خارجی / تولیدی", "خارجی / غیرتولیدی" },
        ["requirement-status"] = new[] { "جدید", "در حال بررسی", "در انتظار استعلام", "در حال مذاکره", "تکمیل‌شده", "لغوشده" }
    };

    public static async Task SeedAsync(AppDb db)
    {
        foreach (var pair in Values)
        {
            for (var i = 0; i < pair.Value.Length; i++)
            {
                var group = pair.Key;
                var value = pair.Value[i];
                if (!await db.LookupOptions.AnyAsync(x => x.Group == group && x.Value == value))
                    db.LookupOptions.Add(new LookupOption { Group = group, Value = value, SortOrder = i + 1, IsActive = true });
            }
        }
        await db.SaveChangesAsync();

        // Normalize the known legacy label without changing its meaning.
        var legacySupplierStatus = await db.Suppliers.Where(x => x.EvidenceStatus == "دارای مستندات").ToListAsync();
        foreach (var item in legacySupplierStatus) item.EvidenceStatus = "دارای مدرک";
        var legacyEvidenceStatus = await db.Evidence.Where(x => x.Status == "دارای مستندات").ToListAsync();
        foreach (var item in legacyEvidenceStatus) item.Status = "دارای مدرک";
        if (legacySupplierStatus.Count + legacyEvidenceStatus.Count > 0) await db.SaveChangesAsync();

        // Preserve other legacy free-text values already stored by v0.2/v0.3 users.
        // They become selectable reference values instead of being lost or blocking startup.
        var legacy = new Dictionary<string, List<string>>
        {
            ["product-category"] = (await db.Products.Select(x => x.Category).Distinct().ToListAsync())
                .Concat(await db.Requirements.Select(x => x.Category).Distinct().ToListAsync()).ToList(),
            ["unit"] = (await db.Products.Select(x => x.Unit).Distinct().ToListAsync())
                .Concat(await db.Requirements.Select(x => x.Unit).Distinct().ToListAsync()).ToList(),
            ["priority"] = await db.Requirements.Select(x => x.Priority).Distinct().ToListAsync(),
            ["evidence-status"] = (await db.Suppliers.Select(x => x.EvidenceStatus).Distinct().ToListAsync())
                .Concat(await db.Evidence.Select(x => x.Status).Distinct().ToListAsync()).ToList()
        };
        foreach (var pair in legacy)
        {
            foreach (var raw in pair.Value)
            {
                var value = raw?.Trim();
                if (string.IsNullOrWhiteSpace(value) || value.Length > 120) continue;
                if (!await db.LookupOptions.AnyAsync(x => x.Group == pair.Key && x.Value == value))
                    db.LookupOptions.Add(new LookupOption { Group = pair.Key, Value = value, SortOrder = 900, IsActive = true });
            }
        }
        await db.SaveChangesAsync();
    }

    public static IReadOnlyDictionary<string, string[]> All => Values;
}
