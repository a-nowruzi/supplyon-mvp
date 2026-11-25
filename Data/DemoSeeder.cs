using Microsoft.EntityFrameworkCore;
using O2Ban.ValidationWorkspace.Models;

namespace O2Ban.ValidationWorkspace.Data;

/// <summary>
/// Idempotent automotive pilot data. All organizations, people, offers and commercial values
/// are fictional training records and must never be treated as verified market information.
/// Existing user passwords are preserved when the seed is re-run.
/// </summary>
public static class DemoSeeder
{
    private const string DemoPassword = "O2ban@2026";

    public static async Task SeedAsync(AppDb db)
    {
        await SeedUsers(db);
        await SeedCompanies(db);
        await SeedSuppliers(db);
        await SeedProducts(db);
        await SeedRequirements(db);
        await SeedRfqs(db);
        await SeedOffers(db);
        await SeedEvidence(db);
        // Normalize the status vocabulary in previously seeded demo records only.
        var legacyStatusRows = await db.Suppliers.Where(x => x.Name.Contains("(نمونه)") && x.EvidenceStatus == "دارای مستندات").ToListAsync();
        foreach (var row in legacyStatusRows) row.EvidenceStatus = "دارای مدرک";
        await db.SaveChangesAsync();
        await ValidateSeedAsync(db);
    }

    private static async Task ValidateSeedAsync(AppDb db)
    {
        var checks = new Dictionary<string, int>
        {
            ["کاربران"] = await db.Users.CountAsync(x => x.Username.StartsWith("expert")),
            ["شرکت‌های هدف"] = await db.Companies.CountAsync(),
            ["تأمین‌کنندگان"] = await db.Suppliers.CountAsync(),
            ["کالاهای کاتالوگ"] = await db.Products.CountAsync(),
            ["درخواست‌های خرید"] = await db.Requirements.CountAsync(),
            ["استعلام‌ها"] = await db.Rfqs.CountAsync(),
            ["پیشنهادها"] = await db.Offers.CountAsync(),
            ["شواهد"] = await db.Evidence.CountAsync(),
            ["گزینه‌های اطلاعات پایه"] = await db.LookupOptions.CountAsync(x => x.IsActive)
        };
        var missing = checks.Where(x => x.Value < (x.Key switch { "کاربران" => 4, "شرکت‌های هدف" => 10, "تأمین‌کنندگان" => 12, "کالاهای کاتالوگ" => 15, "درخواست‌های خرید" => 8, "استعلام‌ها" => 8, "پیشنهادها" => 8, "شواهد" => 15, "گزینه‌های اطلاعات پایه" => 30, _ => 1 })).ToList();
        var personas = new[] { "داخلی | تولیدی", "داخلی | غیرتولیدی", "خارجی | تولیدی", "خارجی | غیرتولیدی" };
        foreach (var persona in personas)
        {
            var count = await db.Requirements.CountAsync(x => x.Title.Contains(persona));
            if (count < 2) missing.Add(new KeyValuePair<string, int>($"سناریوی {persona}", count));
        }
        var expectedLookups = new Dictionary<string, int>
        {
            ["product-category"] = 12, ["unit"] = 17, ["priority"] = 3,
            ["evidence-status"] = 7, ["supplier-segment"] = 4, ["requirement-status"] = 6
        };
        foreach (var item in expectedLookups)
        {
            var actual = await db.LookupOptions.CountAsync(x => x.IsActive && x.Group == item.Key);
            if (actual < item.Value) missing.Add(new KeyValuePair<string, int>($"اطلاعات پایه {item.Key}", actual));
        }
        var evidenceStatuses = await db.LookupOptions.Where(x => x.Group == "evidence-status" && x.IsActive).Select(x => x.Value).ToListAsync();
        if (await db.Suppliers.AnyAsync(x => !evidenceStatuses.Contains(x.EvidenceStatus)))
            missing.Add(new KeyValuePair<string, int>("وضعیت شواهد تأمین‌کنندگان ناسازگار", 0));
        if (await db.Evidence.AnyAsync(x => !evidenceStatuses.Contains(x.Status)))
            missing.Add(new KeyValuePair<string, int>("وضعیت شواهد ادعاها ناسازگار", 0));
        var categories = await db.LookupOptions.Where(x => x.Group == "product-category" && x.IsActive).Select(x => x.Value).ToListAsync();
        var units = await db.LookupOptions.Where(x => x.Group == "unit" && x.IsActive).Select(x => x.Value).ToListAsync();
        var priorities = await db.LookupOptions.Where(x => x.Group == "priority" && x.IsActive).Select(x => x.Value).ToListAsync();
        if (await db.Products.AnyAsync(x => !categories.Contains(x.Category) || !units.Contains(x.Unit)))
            missing.Add(new KeyValuePair<string, int>("کالا با گروه یا واحد نامعتبر", 0));
        if (await db.Requirements.AnyAsync(x => !categories.Contains(x.Category) || !units.Contains(x.Unit) || !priorities.Contains(x.Priority)))
            missing.Add(new KeyValuePair<string, int>("درخواست خرید با گروه، واحد یا اولویت نامعتبر", 0));
        if (missing.Count > 0)
            throw new InvalidOperationException("داده‌های پایه پایلوت کامل نیستند: " + string.Join("، ", missing.Select(x => $"{x.Key}={x.Value}")));
    }

    private static async Task SeedUsers(AppDb db)
    {
        var users = new[]
        {
            new AppUser { Username="expert1", FullName="کارشناس داخلی ـ قطعات تولیدی", Role="کارشناس بازرگانی داخلی | قطعات تولیدی", OrganizationName="تیم پایلوت اتوبان", PasswordHash=PasswordHashing.Hash(DemoPassword) },
            new AppUser { Username="expert2", FullName="کارشناس داخلی ـ اقلام غیرتولیدی", Role="کارشناس بازرگانی داخلی | اقلام غیرتولیدی", OrganizationName="تیم پایلوت اتوبان", PasswordHash=PasswordHashing.Hash(DemoPassword) },
            new AppUser { Username="expert3", FullName="کارشناس خارجی ـ قطعات تولیدی", Role="کارشناس بازرگانی خارجی | قطعات تولیدی", OrganizationName="تیم پایلوت اتوبان", PasswordHash=PasswordHashing.Hash(DemoPassword) },
            new AppUser { Username="expert4", FullName="کارشناس خارجی ـ اقلام غیرتولیدی", Role="کارشناس بازرگانی خارجی | اقلام غیرتولیدی", OrganizationName="تیم پایلوت اتوبان", PasswordHash=PasswordHashing.Hash(DemoPassword) },
            new AppUser { Username="admin", FullName="مدیر محیط آزمایشی", Role="مدیر سامانه", OrganizationName="تیم پایلوت اتوبان", PasswordHash=PasswordHashing.Hash(DemoPassword) }
        };
        foreach (var seed in users)
        {
            var current = await db.Users.SingleOrDefaultAsync(x => x.Username == seed.Username);
            if (current is null) db.Users.Add(seed);
            else
            {
                // Keep a tester's changed password and active/disabled state; refresh persona metadata.
                current.FullName = seed.FullName;
                current.Role = seed.Role;
                current.OrganizationName = seed.OrganizationName;
            }
        }
    }

    private static async Task SeedCompanies(AppDb db)
    {
        var rows = new[]
        {
            C("گروه خودروسازی آذرخش (نمونه)","Tier 1","تهران","نرگس امینی","مدیر تدارکات","هدف مصاحبه؛ داده کاملاً ساختگی"),
            C("مجموعه قطعه‌سازی سپند (نمونه)","Tier 1","ساوه","پیمان راد","مدیر خرید قطعات تولیدی","سناریوی خرید داخلی قطعه تولیدی"),
            C("صنایع پویش خودرو (نمونه)","Tier 2","تبریز","سحر شریفی","کارشناس بازرگانی","تأمین‌کننده بالقوه قطعات فلزی"),
            C("تجهیزات خطوط تولید کارمانیا (نمونه)","Tier 2","اصفهان","مهدی رستگار","سرپرست نگهداری","اقلام مصرفی و نگهداری"),
            C("شرکت خدمات و پشتیبانی راهبرد (نمونه)","Tier 2","کرج","الهام مرادی","مدیر خدمات","سناریوی اقلام غیرتولیدی"),
            C("تجارت بین‌الملل سپهر (نمونه)","Tier 1","تهران","آرمان نوری","کارشناس واردات","سناریوی خرید خارجی"),
            C("تأمین قطعات دقیق آناتولی (نمونه)","Tier 1","بورسا - ترکیه","Emre Kaya","Export Sales","طرف خارجی فرضی؛ بدون ارتباط واقعی"),
            C("الکترونیک خودرو دانوب (نمونه)","Tier 1","بوداپست - مجارستان","Anna Kovacs","Sales Manager","طرف خارجی فرضی؛ بدون ارتباط واقعی"),
            C("ابزار صنعتی راین (نمونه)","Tier 2","اشتوتگارت - آلمان","Lukas Weber","Export Desk","سناریوی خرید غیرتولیدی خارجی"),
            C("تجهیزات ایمنی اوراسیا (نمونه)","Tier 2","استانبول - ترکیه","Deniz Yilmaz","Sales","اقلام ایمنی و کارگاهی"),
            C("مواد بسته‌بندی پارسین (نمونه)","Tier 2","قزوین","ریحانه توکلی","کارشناس فروش","بسته‌بندی صنعتی"),
            C("خدمات لجستیک فرضی مسیر (نمونه)","Tier 2","تهران","امیر فلاح","مدیر عملیات","صرفاً برای گفت‌وگوی سناریویی؛ ماژول لجستیک فعال نیست")
        };
        foreach (var x in rows)
            if (!await db.Companies.AnyAsync(y => y.Name == x.Name)) db.Companies.Add(x);
    }

    private static Company C(string name, string tier, string city, string contact, string role, string notes) => new()
    {
        Name=name, Tier=tier, City=city, ContactName=contact, ContactRole=role,
        RelationshipStatus="شناسایی‌شده", Notes="[داده آزمایشی ساختگی] "+notes, CreatedAt=DateTime.UtcNow.AddDays(-18)
    };

    private static async Task SeedSuppliers(AppDb db)
    {
        var rows = new[]
        {
            S("فولاد و قطعه آریا (نمونه)","تهران - ایران","ورق فولادی بدنه، براکت و قطعات پرس‌کاری؛ برش و فرم‌دهی","براکت نگهدارنده، قطعه پرس‌کاری‌شده، ورق ST12","داخلی / تولیدی","۳۰ روزه پس از تأیید اعتبار",18,"اعلام‌شده",56,"ظرفیت، گواهی مواد و نتایج آزمون ابعادی باید بررسی شود."),
            S("پارس لاستیک خودرو (نمونه)","اراک - ایران","قطعات لاستیکی خودرو، بوش و گردگیر؛ قالب‌گیری لاستیک","بوش لاستیکی، گردگیر پلوس، ضربه‌گیر","داخلی / تولیدی","۳۰٪ پیش‌پرداخت، مانده پس از تحویل",25,"دارای مدرک",68,"نمونه گواهی سیستم کیفیت درج شده؛ اصالت و دامنه اعتبارسنجی نشده است."),
            S("پویش پلیمری سپند (نمونه)","قزوین - ایران","تزریق پلاستیک و قطعات پلیمری داخلی خودرو","قاب تزئینی، بست پلاستیکی، کانال هوا","داخلی / تولیدی","نقدی یا ۱۵ روزه",14,"نیازمند به‌روزرسانی",52,"ظرفیت اعلامی قدیمی است و باید از تأمین‌کننده استعلام شود."),
            S("الکترو محرکه نوآور (نمونه)","تهران - ایران","مونتاژ دسته‌سیم و مجموعه‌های الکتریکی خودرو","دسته‌سیم، کانکتور مونتاژی، جعبه فیوز","داخلی / تولیدی","۴۵ روزه با اعتبارسنجی",30,"اعلام‌شده",55,"نمونه؛ مدارک آزمون عملکرد و ردیابی سری ساخت لازم است."),
            S("ماشین‌کاری دقیق البرز (نمونه)","کرج - ایران","تراشکاری CNC، سنگ‌زنی و ساخت قطعات فلزی دقیق","بوش فلزی، شفت، پین و قطعات ماشین‌کاری‌شده","داخلی / تولیدی","نقدی و ۳۰ روزه",12,"تأییدشده توسط شرکت",73,"تأیید صرفاً توسط خود شرکت؛ بازرسی مستقل انجام نشده است."),
            S("پخش صنعتی چرخش (نمونه)","تهران - ایران","توزیع بلبرینگ، تسمه، فیلتر و اقلام نگهداری","بلبرینگ، تسمه، فیلتر روغن و هوا","داخلی / غیرتولیدی","نقدی، ۳۰ روزه",4,"اعلام‌شده",61,"توزیع‌کننده فرضی؛ نمایندگی برندها تأیید نشده است."),
            S("بسته‌بندی کارا (نمونه)","قزوین - ایران","تأمین کارتن، فیلم استرچ و ملزومات بسته‌بندی قطعات","کارتن پنج‌لایه، فیلم استرچ، تسمه بسته‌بندی","داخلی / غیرتولیدی","نقدی یا ۱۵ روزه",6,"دارای تناقض",43,"تفاوت میان ابعاد اعلامی و کاتالوگ نمونه؛ نیازمند رفع تناقض."),
            S("تجهیزات ایمنی پایدار (نمونه)","اصفهان - ایران","توزیع تجهیزات حفاظت فردی و ایمنی کارگاه","کفش ایمنی، دستکش ضدبرش، عینک محافظ","داخلی / غیرتولیدی","۳۰ روزه",7,"اعلام‌شده",58,"استاندارد و اصالت کالا باید برای هر قلم بررسی شود."),
            S("تأمین ماشین‌افزار نوین (نمونه)","تبریز - ایران","توزیع ابزار برشی، ابزار اندازه‌گیری و ملزومات تعمیرات","مته، اینسرت تراشکاری، کولیس و آچار ترک‌متر","داخلی / غیرتولیدی","نقدی و چکی",5,"نیازمند به‌روزرسانی",49,"موجودی لحظه‌ای در این پایلوت قابل استعلام سیستمی نیست."),
            S("Anatolia Precision Components (نمونه)","بورسا - ترکیه","تولید قطعات فلزی ماشین‌کاری‌شده و فورج خودرو","فلنج، شفت، قطعات فورج و ماشین‌کاری","خارجی / تولیدی","پیش‌پرداخت ۳۰٪، مانده قبل از حمل",45,"اعلام‌شده",60,"شرکت، گواهی‌ها و ظرفیت همگی ساختگی و صرفاً برای آزمون واردات هستند."),
            S("Danube Auto Electronics (نمونه)","بوداپست - مجارستان","تولید حسگرها و ماژول‌های الکترونیکی خودرو","سنسور فشار، ماژول کنترل، سنسور دما","خارجی / تولیدی","اعتبار اسنادی یا پیش‌پرداخت",60,"نیازمند به‌روزرسانی",50,"شرایط پرداخت، کنترل صادرات و انطباق فنی باید در سناریوی واقعی بررسی شوند."),
            S("Rhein Industrial Tools (نمونه)","اشتوتگارت - آلمان","تولید و عرضه ابزار اندازه‌گیری و ابزار صنعتی","کولیس دیجیتال، میکرومتر، ابزار گشتاور","خارجی / غیرتولیدی","پیش‌پرداخت یا اعتبار اسنادی",35,"اعلام‌شده",57,"کشور مبدأ، تعرفه، حمل و اسناد واردات در این نسخه محاسبه نمی‌شوند."),
            S("Eurasia Workshop Supplies (نمونه)","استانبول - ترکیه","توزیع تجهیزات ایمنی و ملزومات تعمیرگاهی","دستکش صنعتی، کفش ایمنی، جک کارگاهی","خارجی / غیرتولیدی","پرداخت پیش از ارسال",20,"مشکوک",35,"برای آزمون وضعیت مشکوک؛ تا بررسی اسناد نباید انتخاب شود.")
        };
        foreach (var x in rows)
            if (!await db.Suppliers.AnyAsync(y => y.Name == x.Name)) db.Suppliers.Add(x);
    }

    private static Supplier S(string name,string city,string capabilities,string products,string segment,string terms,int lead,string evidence,int score,string notes) => new()
    {
        Name=name, City=city, Capabilities=capabilities, Products=products, ContactName="مخاطب نمونه",
        PaymentTerms=terms, LeadTimeDays=lead, EvidenceStatus=evidence, InitialScore=score,
        Notes=$"[داده آزمایشی ساختگی] بخش آزمون: {segment}. {notes}", CreatedAt=DateTime.UtcNow.AddDays(-12)
    };

    private static async Task SeedProducts(AppDb db)
    {
        var rows = new[]
        {
            P("AUT-P-001","براکت نگهدارنده مجموعه موتور","قطعات فلزی تولیدی","عدد","ورق فولادی؛ ضخامت، پوشش، تلرانس سوراخ‌ها و نقشه مصوب باید مشخص شود."),
            P("AUT-P-002","بوش لاستیکی طبق","قطعات پلیمری تولیدی","عدد","سختی Shore A، جنس الاستومر، ابعاد و آزمون دوام نیازمند تأیید است."),
            P("AUT-P-003","گردگیر پلوس","قطعات پلیمری تولیدی","عدد","جنس، محدوده دمایی، مقاومت روغن و ابعاد نصب باید کنترل شود."),
            P("AUT-P-004","قاب تزئینی داشبورد","قطعات پلیمری تولیدی","عدد","جنس پلیمر، رنگ، بافت سطح، ابعاد و تطابق ظاهری."),
            P("AUT-P-005","دسته‌سیم مجموعه جلو","قطعات الکتریکی تولیدی","دستگاه","نقشه سیم‌کشی، سطح مقطع، نوع کانکتور، آزمون پیوستگی و ردیابی."),
            P("AUT-P-006","سنسور فشار روغن","قطعات الکترونیکی تولیدی","عدد","محدوده فشار، دقت، نوع خروجی، کانکتور و آزمون عملکرد."),
            P("AUT-P-007","ماژول کنترل الکترونیکی","قطعات الکترونیکی تولیدی","عدد","نسخه سخت‌افزار و نرم‌افزار، پروتکل، شرایط محیطی و تأییدیه خودروساز."),
            P("AUT-P-008","بلبرینگ چرخ","قطعات مکانیکی تولیدی","عدد","ابعاد، کلاس لقی، بار دینامیکی، آب‌بندی و استاندارد فنی."),
            P("AUT-P-009","شفت ماشین‌کاری‌شده","قطعات فلزی تولیدی","عدد","جنس، عملیات حرارتی، سختی، زبری سطح و تلرانس هندسی."),
            P("AUT-N-001","فیلتر روغن تجهیزات خط","ملزومات نگهداری غیرتولیدی","عدد","ابعاد، نوع فیلتر، درجه فیلتراسیون و سازگاری تجهیز."),
            P("AUT-N-002","اینسرت تراشکاری","ابزار و مصرفی کارگاه","بسته","گرید، هندسه، پوشش و سازگاری با جنس قطعه."),
            P("AUT-N-003","دستکش ضدبرش","ایمنی و حفاظت فردی","جفت","سطح حفاظت، سایز، جنس و استاندارد معتبر."),
            P("AUT-N-004","کارتن پنج‌لایه قطعات","بسته‌بندی غیرتولیدی","عدد","ابعاد داخلی، تحمل بار، نوع فلوت و چاپ."),
            P("AUT-N-005","فیلم استرچ صنعتی","بسته‌بندی غیرتولیدی","کیلوگرم","ضخامت، عرض، وزن رول و درصد کشش."),
            P("AUT-N-006","کولیس دیجیتال ۱۵۰ میلی‌متر","ابزار اندازه‌گیری","عدد","دقت، گواهی کالیبراسیون، محدوده اندازه‌گیری و برند."),
            P("AUT-N-007","روغن هیدرولیک تجهیزات","روانکار و نگهداری","لیتر","گرید ویسکوزیته، استاندارد سازنده تجهیز و برگه فنی."),
            P("AUT-N-008","کفش ایمنی کارگاهی","ایمنی و حفاظت فردی","جفت","کلاس حفاظتی، سایزبندی، زیره و استاندارد."),
            P("AUT-N-009","جک سوسماری تعمیرگاهی","تجهیزات تعمیرگاه","دستگاه","ظرفیت مجاز، ارتفاع، استاندارد ایمنی و گارانتی.")
        };
        foreach (var x in rows)
            if (!await db.Products.AnyAsync(y => y.Code == x.Code)) db.Products.Add(x);
    }

    private static Product P(string code,string name,string category,string unit,string description) => new()
    { Code=code, Name=name, Category=category, Unit=unit, TechnicalDescription="[مشخصات سناریوی آموزشی؛ نه مشخصات خرید نهایی] "+description, Brand="نمونه / بدون برند واقعی" };

    private static async Task SeedRequirements(AppDb db)
    {
        var rows = new[]
        {
            R("داخلی | تولیدی | براکت نگهدارنده موتور","قطعات فلزی تولیدی","۱۲۰۰ عدد؛ نقشه و تلرانس پیش از سفارش باید پیوست و تأیید شود.",1200,"عدد","ساوه - ایران",21,"۳۰ روزه پس از تأیید کیفیت","زیاد","کارشناس داخلی ـ قطعات تولیدی"),
            R("داخلی | تولیدی | بوش لاستیکی طبق","قطعات پلیمری تولیدی","۵۰۰۰ عدد؛ درخواست ارائه نمونه اولیه و نتایج آزمون دوام.",5000,"عدد","تهران - ایران",30,"۳۰٪ پیش‌پرداخت، مانده پس از تأیید","فوری","کارشناس داخلی ـ قطعات تولیدی"),
            R("داخلی | غیرتولیدی | فیلتر روغن خط تولید","ملزومات نگهداری غیرتولیدی","۸۰ عدد؛ کد تجهیز و نمونه فیلتر قبل از سفارش تطبیق داده شود.",80,"عدد","تهران - ایران",7,"نقدی یا ۳۰ روزه","عادی","کارشناس داخلی ـ اقلام غیرتولیدی"),
            R("داخلی | غیرتولیدی | کارتن بسته‌بندی قطعات","بسته‌بندی غیرتولیدی","۳۰۰۰ عدد؛ ابعاد، تحمل بار و چاپ نمونه باید تأیید شود.",3000,"عدد","قزوین - ایران",10,"۱۵ روزه","زیاد","کارشناس داخلی ـ اقلام غیرتولیدی"),
            R("خارجی | تولیدی | سنسور فشار روغن","قطعات الکترونیکی تولیدی","۱۰۰۰ عدد؛ دیتاشیت، نمونه، گواهی مبدأ و انطباق با مشخصات فنی لازم است.",1000,"عدد","بندرعباس - ایران",75,"اعتبار اسنادی یا پیش‌پرداخت طبق مقررات","زیاد","کارشناس خارجی ـ قطعات تولیدی"),
            R("خارجی | تولیدی | بلبرینگ چرخ","قطعات مکانیکی تولیدی","۲۰۰۰ عدد؛ برند، کشور مبدأ، بار مجاز، گواهی و نمونه الزامی است.",2000,"عدد","تهران - ایران",60,"اعتبار اسنادی / شرایط قابل مذاکره","فوری","کارشناس خارجی ـ قطعات تولیدی"),
            R("خارجی | غیرتولیدی | کولیس دیجیتال","ابزار اندازه‌گیری","۲۵ دستگاه؛ گواهی کالیبراسیون، کشور مبدأ و ضمانت بررسی شود.",25,"دستگاه","تهران - ایران",45,"پیش‌پرداخت یا اعتبار اسنادی","عادی","کارشناس خارجی ـ اقلام غیرتولیدی"),
            R("خارجی | غیرتولیدی | ابزار گشتاور","ابزار و تجهیزات تعمیرگاهی","۱۲ دستگاه؛ محدوده گشتاور، گواهی کالیبراسیون و خدمات پس از فروش لازم است.",12,"دستگاه","اصفهان - ایران",50,"شرایط پرداخت در مذاکره مشخص شود","زیاد","کارشناس خارجی ـ اقلام غیرتولیدی")
        };
        foreach (var x in rows)
            if (!await db.Requirements.AnyAsync(y => y.Title == x.Title)) db.Requirements.Add(x);
    }

    private static PurchaseRequirement R(string title,string category,string description,decimal quantity,string unit,string location,int days,string payment,string priority,string by) => new()
    {
        Title=title, Category=category, Description="[سناریوی آموزشی ساختگی] "+description, Quantity=quantity, Unit=unit,
        DeliveryLocation=location, RequiredWithinDays=days, PaymentTerms=payment, Priority=priority, Status="جدید",
        CreatedBy=by, CreatedAt=DateTime.UtcNow.AddDays(-2)
    };

    private static async Task SeedRfqs(AppDb db)
    {
        var rows = new[]
        {
            Q("RFQ-DOM-P-001","استعلام براکت نگهدارنده موتور","داخلی | تولیدی | براکت نگهدارنده موتور","فولاد و قطعه آریا (نمونه)، ماشین‌کاری دقیق البرز (نمونه)","کارشناس داخلی ـ قطعات تولیدی",9),
            Q("RFQ-DOM-P-002","استعلام بوش لاستیکی طبق","داخلی | تولیدی | بوش لاستیکی طبق","پارس لاستیک خودرو (نمونه)، پویش پلیمری سپند (نمونه)","کارشناس داخلی ـ قطعات تولیدی",8),
            Q("RFQ-DOM-N-001","استعلام فیلتر روغن خط تولید","داخلی | غیرتولیدی | فیلتر روغن خط تولید","پخش صنعتی چرخش (نمونه)، تأمین ماشین‌افزار نوین (نمونه)","کارشناس داخلی ـ اقلام غیرتولیدی",7),
            Q("RFQ-DOM-N-002","استعلام کارتن بسته‌بندی قطعات","داخلی | غیرتولیدی | کارتن بسته‌بندی قطعات","بسته‌بندی کارا (نمونه)","کارشناس داخلی ـ اقلام غیرتولیدی",6),
            Q("RFQ-FOR-P-001","استعلام خارجی سنسور فشار روغن","خارجی | تولیدی | سنسور فشار روغن","Danube Auto Electronics (نمونه)","کارشناس خارجی ـ قطعات تولیدی",12),
            Q("RFQ-FOR-P-002","استعلام خارجی بلبرینگ چرخ","خارجی | تولیدی | بلبرینگ چرخ","Anatolia Precision Components (نمونه)","کارشناس خارجی ـ قطعات تولیدی",11),
            Q("RFQ-FOR-N-001","استعلام خارجی کولیس دیجیتال","خارجی | غیرتولیدی | کولیس دیجیتال","Rhein Industrial Tools (نمونه)","کارشناس خارجی ـ اقلام غیرتولیدی",10),
            Q("RFQ-FOR-N-002","استعلام خارجی ابزار گشتاور","خارجی | غیرتولیدی | ابزار گشتاور","Rhein Industrial Tools (نمونه)","کارشناس خارجی ـ اقلام غیرتولیدی",9)
        };
        foreach (var x in rows)
            if (!await db.Rfqs.AnyAsync(y => y.Code == x.Code)) db.Rfqs.Add(x);
    }

    private static Rfq Q(string code,string title,string requirement,string suppliers,string by,int deadlineDays) => new()
    { Code=code, Title=title, RequirementTitle=requirement, InvitedSuppliers=suppliers, Deadline=DateTime.UtcNow.AddDays(deadlineDays), Status="در انتظار پیشنهاد", CreatedBy=by, CreatedAt=DateTime.UtcNow.AddDays(-1) };

    private static async Task SeedOffers(AppDb db)
    {
        var rows = new[]
        {
            O("RFQ-DOM-P-001","فولاد و قطعه آریا (نمونه)","داخلی؛ قیمت نمونه به ریال",485000,1200,18,"۳۰ روزه", "نیازمند تأیید", "مشخصات فنی و هزینه ابزار باید روشن شود."),
            O("RFQ-DOM-P-001","ماشین‌کاری دقیق البرز (نمونه)","داخلی؛ قیمت نمونه به ریال",520000,1200,12,"نقدی و ۳۰ روزه", "نیازمند تأیید", "پیشنهاد فرضی؛ ظرفیت و نقشه باید تأیید شود."),
            O("RFQ-DOM-P-002","پارس لاستیک خودرو (نمونه)","داخلی؛ قیمت نمونه به ریال",168000,5000,25,"۳۰٪ پیش‌پرداخت", "نیازمند تأیید", "هزینه قالب و نمونه‌سازی در قیمت منظور نشده است."),
            O("RFQ-DOM-N-001","پخش صنعتی چرخش (نمونه)","داخلی؛ قیمت نمونه به ریال",890000,80,4,"نقدی", "نیازمند تأیید", "کد فنی فیلتر باید با تجهیز تطبیق داده شود."),
            O("RFQ-DOM-N-002","بسته‌بندی کارا (نمونه)","داخلی؛ قیمت نمونه به ریال",42000,3000,6,"۱۵ روزه", "نیازمند تأیید", "نمونه چاپ و تست تحمل بار لازم است."),
            O("RFQ-FOR-P-001","Danube Auto Electronics (نمونه)","خارجی؛ مبلغ فرضی معادل ریالی",2450000,1000,60,"اعتبار اسنادی", "نیازمند تأیید", "نرخ ارز، حمل، بیمه، گمرک و مالیات محاسبه نشده است."),
            O("RFQ-FOR-P-002","Anatolia Precision Components (نمونه)","خارجی؛ مبلغ فرضی معادل ریالی",1380000,2000,45,"۳۰٪ پیش‌پرداخت", "نیازمند تأیید", "هزینه حمل، بیمه، گمرک و شرایط تحویل باید جداگانه بررسی شود."),
            O("RFQ-FOR-N-001","Rhein Industrial Tools (نمونه)","خارجی؛ مبلغ فرضی معادل ریالی",7800000,25,35,"پیش‌پرداخت", "نیازمند تأیید", "گواهی کالیبراسیون و هزینه واردات در مبلغ نیست."),
            O("RFQ-FOR-N-002","Rhein Industrial Tools (نمونه)","خارجی؛ مبلغ فرضی معادل ریالی",12900000,12,40,"اعتبار اسنادی", "نیازمند تأیید", "محدوده گشتاور و گواهی کالیبراسیون باید بررسی شود.")
        };
        foreach (var x in rows)
            if (!await db.Offers.AnyAsync(y => y.RfqCode == x.RfqCode && y.SupplierName == x.SupplierName)) db.Offers.Add(x);
    }

    private static Offer O(string rfq,string supplier,string priceContext,decimal price,decimal qty,int days,string payment,string fit,string notes) => new()
    {
        RfqCode=rfq, SupplierName=supplier, UnitPrice=price, Quantity=qty, DeliveryDays=days, PaymentTerms=payment,
        TechnicalFit=fit, ValidUntil=DateTime.UtcNow.AddDays(14), Notes=$"[پیشنهاد کاملاً فرضی] {priceContext}. {notes}", CreatedAt=DateTime.UtcNow.AddHours(-10)
    };

    private static async Task SeedEvidence(AppDb db)
    {
        var rows = new[]
        {
            E("فولاد و قطعه آریا (نمونه)","توانایی تولید براکت پرس‌کاری‌شده","اظهار شرکت","اعلام‌شده","نقشه و نمونه قطعه هنوز دریافت نشده است."),
            E("فولاد و قطعه آریا (نمونه)","ظرفیت ماهانه ۸۰۰۰ عدد","اظهار شرکت","نیازمند به‌روزرسانی","عدد صرفاً ادعای آموزشی است؛ مستند ظرفیت موجود نیست."),
            E("پارس لاستیک خودرو (نمونه)","سیستم مدیریت کیفیت","سند بارگذاری‌شده","دارای مدرک","فایل نمونه در این بسته ضمیمه نشده؛ صرفاً وضعیت گردش‌کار برای تست است."),
            E("پارس لاستیک خودرو (نمونه)","توانایی تولید بوش لاستیکی","کاتالوگ","تأییدشده توسط شرکت","تأیید مستقل یا ممیزی انجام نشده است."),
            E("پویش پلیمری سپند (نمونه)","ظرفیت تزریق پلاستیک","اظهار شرکت","نیازمند به‌روزرسانی","تاریخ اعلام ظرفیت قدیمی است."),
            E("الکترو محرکه نوآور (نمونه)","آزمون پیوستگی دسته‌سیم","اظهار شرکت","اعلام‌شده","گزارش آزمون باید دریافت و بررسی شود."),
            E("ماشین‌کاری دقیق البرز (نمونه)","تجهیزات CNC","اظهار شرکت","تأییدشده توسط شرکت","تأیید صرفاً توسط شرکت؛ بازدید مستقل نشده است."),
            E("بسته‌بندی کارا (نمونه)","ابعاد کارتن پنج‌لایه","کاتالوگ","دارای تناقض","ابعاد کاتالوگ با مقدار ثبت‌شده در فرم فروش یکسان نیست."),
            E("تجهیزات ایمنی پایدار (نمونه)","استاندارد کفش ایمنی","اظهار شرکت","اعلام‌شده","شماره استاندارد و دامنه اعتبار باید بررسی شود."),
            E("تأمین ماشین‌افزار نوین (نمونه)","موجودی اینسرت تراشکاری","اظهار شرکت","نیازمند به‌روزرسانی","موجودی لحظه‌ای و تاریخ آخرین به‌روزرسانی مشخص نیست."),
            E("Anatolia Precision Components (نمونه)","کشور مبدأ قطعات فورج","اظهار شرکت","اعلام‌شده","گواهی مبدأ در این پایلوت دریافت نشده است."),
            E("Anatolia Precision Components (نمونه)","گواهی سیستم کیفیت","سند بارگذاری‌شده","نیازمند به‌روزرسانی","اعتبار و دامنه گواهی باید بررسی شود."),
            E("Danube Auto Electronics (نمونه)","دیتاشیت سنسور فشار","کاتالوگ","اعلام‌شده","نسخه دیتاشیت و انطباق قطعه نیازمند بررسی است."),
            E("Danube Auto Electronics (نمونه)","انطباق با استاندارد خودروساز","اظهار شرکت","مشکوک","ادعا بدون شماره استاندارد یا مدرک پشتیبان ثبت شده است."),
            E("Rhein Industrial Tools (نمونه)","گواهی کالیبراسیون کولیس","سند بارگذاری‌شده","اعلام‌شده","شماره سریال و مرجع صادرکننده باید بررسی شود."),
            E("Eurasia Workshop Supplies (نمونه)","نمایندگی برند تجهیزات ایمنی","اظهار شرکت","مشکوک","برای آزمون هشدار؛ هیچ تأیید مستقلی انجام نشده است."),
            E("پخش صنعتی چرخش (نمونه)","نمایندگی رسمی برند فیلتر","اظهار شرکت","دارای تناقض","نام برند در فاکتور نمونه با ادعای نمایندگی همخوانی ندارد."),
            E("ماشین‌کاری دقیق البرز (نمونه)","سابقه تحویل به خودروساز","شواهد معامله","اعلام‌شده","سند معامله واقعی در محیط آزمایشی وجود ندارد؛ ادعا ساختگی است.")
        };
        foreach (var x in rows)
            if (!await db.Evidence.AnyAsync(y => y.SupplierName == x.SupplierName && y.Claim == x.Claim)) db.Evidence.Add(x);
    }

    private static Evidence E(string supplier,string claim,string source,string status,string notes) => new()
    { SupplierName=supplier, Claim=claim, Source=source, Status=status, Notes="[داده ساختگی برای آزمون؛ اعتبار واقعی ندارد] "+notes, CreatedAt=DateTime.UtcNow.AddDays(-5) };
}

static class PasswordHashing
{
    public static string Hash(string p)
    {
        var s=System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        var h=System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(p,s,120000,System.Security.Cryptography.HashAlgorithmName.SHA256,32);
        return Convert.ToBase64String(s)+"."+Convert.ToBase64String(h);
    }
}
