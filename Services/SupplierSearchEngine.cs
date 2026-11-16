using System.Text.RegularExpressions;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Services;

/// <summary>
/// Explainable local matching engine for the pilot. It does not call an LLM
/// and never treats text similarity as technical verification.
/// </summary>
public static class SupplierSearchEngine
{
    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "برای", "از", "به", "با", "در", "و", "یا", "یک", "عدد", "مورد", "نیاز", "خرید", "کالا", "قلم", "شرکت", "تامین", "تأمین"
    };

    public static List<SupplierSearchResult> Search(
        IEnumerable<Supplier> suppliers,
        string? query,
        string? itemCode,
        string? category,
        string? industrySegment)
    {
        var queryTokens = Tokenize(query);
        var itemTokens = Tokenize(itemCode);
        var categoryTokens = Tokenize(category);
        var segmentTokens = Tokenize(industrySegment);

        return suppliers.Select(supplier => Score(supplier, queryTokens, itemTokens, categoryTokens, segmentTokens))
            .Where(x => x.RelevanceScore > 0)
            .OrderByDescending(x => x.RelevanceScore)
            .ThenBy(x => x.Supplier.Name)
            .ToList();
    }

    private static SupplierSearchResult Score(Supplier supplier, HashSet<string> queryTokens,
        HashSet<string> itemTokens, HashSet<string> categoryTokens, HashSet<string> segmentTokens)
    {
        var capabilities = Normalize(supplier.Capabilities);
        var searchable = Normalize($"{supplier.Name} {supplier.Capabilities}");
        var score = 0;
        var reasons = new List<string>();
        var gaps = new List<string>();

        if (queryTokens.Count > 0)
        {
            var matched = queryTokens.Where(searchable.Contains).ToList();
            var coverage = (double)matched.Count / queryTokens.Count;
            var points = (int)Math.Round(coverage * 35);
            score += points;
            if (matched.Count > 0) reasons.Add($"{matched.Count} واژه از {queryTokens.Count} واژه جست‌وجو در نام یا توانمندی اعلام‌شده یافت شد");
        }

        if (itemTokens.Count > 0)
        {
            var matched = itemTokens.Where(capabilities.Contains).ToList();
            var coverage = (double)matched.Count / itemTokens.Count;
            var points = (int)Math.Round(coverage * 35);
            score += points;
            if (matched.Count > 0) reasons.Add("بخشی از مشخصات یا نام کالا در توانمندی اعلام‌شده تأمین‌کننده دیده شد");
            if (matched.Count == itemTokens.Count) reasons.Add("تمام واژه‌های جست‌وجوی کالا در متن توانمندی وجود دارد");
        }

        if (categoryTokens.Count > 0)
        {
            var matched = categoryTokens.Count(t => capabilities.Contains(t));
            if (matched > 0)
            {
                score += matched == categoryTokens.Count ? 20 : 10;
                reasons.Add("هم‌پوشانی متنی با دسته‌بندی کالا مشاهده شد");
            }
        }

        if (segmentTokens.Count > 0)
        {
            var matched = segmentTokens.Count(t => capabilities.Contains(t));
            if (matched > 0)
            {
                score += matched == segmentTokens.Count ? 10 : 5;
                reasons.Add("نام حوزه کاربرد در توضیحات توانمندی تأمین‌کننده دیده شد");
            }
        }

        if (string.IsNullOrWhiteSpace(supplier.Capabilities)) gaps.Add("توانمندی تأمین‌کننده ثبت نشده است");
        if (string.IsNullOrWhiteSpace(supplier.NationalId)) gaps.Add("شناسه ملی ثبت نشده است");
        gaps.Add("انطباق فنی با نقشه، استاندارد و نمونه کالا هنوز بررسی نشده است");
        gaps.Add("ظرفیت تولید، موجودی و زمان تحویل مستند در این نسخه ثبت نمی‌شود");

        var evidence = supplier.EvidenceStatus switch
        {
            "تأیید مستقل" => "وضعیت ثبت‌شده: تأیید مستقل؛ دامنه و تاریخ اعتبار مدرک باید جداگانه بررسی شود",
            "مستندات دریافت‌شده" => "مدرک دریافت شده؛ صحت و کفایت آن لزوماً تأیید نشده است",
            "اظهار تأمین‌کننده" => "اطلاعات صرفاً اظهار تأمین‌کننده است و تأیید مستقل محسوب نمی‌شود",
            _ => $"وضعیت شواهد ثبت‌شده: {supplier.EvidenceStatus}؛ نیازمند بررسی کارشناس"
        };

        var label = score >= 65 ? "ارتباط متنی بالا" : score >= 35 ? "ارتباط متنی متوسط" : score > 0 ? "ارتباط متنی محدود" : "بدون تطبیق";
        return new SupplierSearchResult
        {
            Supplier = supplier,
            RelevanceScore = Math.Min(score, 100),
            RelevanceLabel = label,
            MatchReasons = reasons.Distinct().ToList(),
            DataGaps = gaps,
            EvidenceInterpretation = evidence
        };
    }

    private static HashSet<string> Tokenize(string? value)
    {
        var normalized = Normalize(value);
        return Regex.Split(normalized, @"[^\p{L}\p{N}]+")
            .Where(x => x.Length >= 2 && !StopWords.Contains(x))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var text = value.Trim().ToLowerInvariant()
            .Replace('ي', 'ی').Replace('ك', 'ک')
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
            .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9');
        text = Regex.Replace(text, @"\s+", " ");
        return text;
    }
}
