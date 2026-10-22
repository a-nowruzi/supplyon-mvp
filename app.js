const STORAGE_KEY = "riskban_supply_chain_v2";
const LEGACY_STORAGE_KEY = "riskban_mvp";
const SCHEMA_VERSION = 3;
const QUESTIONNAIRE_VERSION = "1.0.0";
const RULES_VERSION = "2.0.0";
const RISK_THRESHOLDS = { medium: 7, high: 15 };

const QUESTIONS = [
  {
    id: "supplierPerformance", category: "عملکرد تأمین‌کننده", title: "پایش تحویل",
    prompt: "آیا تأخیر، کسری یا تغییر کیفیت محموله‌های تأمین‌کننده‌های اصلی را ثبت و پیگیری می‌کنید؟",
    risk: "اختلال در تحویل مواد اولیه", impact: "کمبود مادهٔ حیاتی می‌تواند برنامهٔ تولید و تحویل سفارش‌ها را مختل کند.",
    signals: "تأخیرهای تکراری، تحویل ناقص یا نداشتن سابقهٔ عملکرد تأمین‌کننده.",
    action: "برای اقلام حیاتی، زمان تحویل و کیفیت هر محموله را ثبت کنید و برای تأخیرهای تکراری مسیر جایگزین تعریف کنید.",
    answerLikelihood: { no: 4, partial: 3, yes: null, unknown: 4 }, severity: 4, categoryRisk: "تأمین‌کننده و تداوم",
    criticalReviewAnswers: [], sourceReference: null, reviewDate: null
  },
  {
    id: "supplierQuality", category: "کیفیت مواد ورودی", title: "کنترل کیفیت هنگام دریافت",
    prompt: "آیا برای پذیرش مواد ورودی، معیار مشخصی دارید و محمولهٔ نامنطبق را جدا می‌کنید؟",
    risk: "ورود مادهٔ اولیهٔ نامنطبق", impact: "کیفیت نامناسب یا آلودگی ماده می‌تواند محصول و سلامت مصرف‌کننده را تحت تأثیر قرار دهد.",
    signals: "نبود معیار پذیرش، سوابق مرجوعی یا بررسی ظاهری و شرایط حمل.",
    action: "برای مواد حیاتی معیار پذیرش بنویسید، دریافت را ثبت کنید و محمولهٔ مشکوک را تا تعیین تکلیف جدا نگه دارید.",
    answerLikelihood: { no: 5, partial: 4, yes: null, unknown: 5 }, severity: 5, categoryRisk: "کیفیت و ایمنی مواد",
    criticalReviewAnswers: ["no", "partial", "unknown"],
    sourceReference: { title: "General Principles of Food Hygiene (CXC 1-1969)", author: "Codex Alimentarius Commission", year: 2022, publisher: "FAO/WHO Codex Alimentarius", doi: "", url: "https://www.fao.org/fao-who-codexalimentarius/codex-texts/codes-of-practice/en/" }, reviewDate: null
  },
  {
    id: "traceability", category: "رهگیری و فراخوان", title: "ردیابی بچ مواد اولیه",
    prompt: "آیا می‌توانید هر محمولهٔ مادهٔ اولیه را به تأمین‌کننده و بچ محصولی که در آن مصرف شده وصل کنید؟",
    risk: "ناتوانی در رهگیری مواد یا محصول", impact: "در رخداد کیفیتی یا ایمنی، دامنهٔ بررسی و جمع‌آوری محصول نامشخص می‌ماند.",
    signals: "ثبت‌نشدن شمارهٔ بچ، تاریخ دریافت یا ارتباط مواد با تولید روزانه.",
    action: "شناسهٔ بچ و تاریخ دریافت را ثبت کنید و یک تمرین کوتاه رهگیری از محصول تا محمولهٔ ورودی انجام دهید.",
    answerLikelihood: { no: 4, partial: 3, yes: null, unknown: 4 }, severity: 5, categoryRisk: "رهگیری و فراخوان",
    criticalReviewAnswers: ["no", "partial", "unknown"],
    sourceReference: { title: "Principles for Traceability / Product Tracing (CXG 60-2006)", author: "Codex Alimentarius Commission", year: 2006, publisher: "FAO/WHO Codex Alimentarius", doi: "", url: "https://www.fao.org/fao-who-codexalimentarius/committees/committee/related-standards/en/?committee=CCFICS", statusNote: "نسخهٔ مصوب ثبت‌شده ۲۰۰۶ است؛ پیش‌نویس بازنگری آن در ۱۸ سپتامبر ۲۰۲۶ برای دریافت نظر منتشر شد و باید پیش از استفاده دوباره بررسی شود.", statusUrl: "https://www.fao.org/fao-who-codexalimentarius/resources/circular-letters/en/" }, reviewDate: null
  },
  {
    id: "inventory", category: "موجودی و ماندگاری", title: "پوشش موجودی و زمان تأمین",
    prompt: "آیا نقطهٔ سفارش مواد را با زمان تأمین، نوسان مصرف و ماندگاری آن‌ها هماهنگ می‌کنید؟",
    risk: "کمبود یا ضایعات ناشی از برنامه‌ریزی موجودی", impact: "موجودی ناکافی توقف تولید و موجودی بیش‌ازحد فساد یا خواب سرمایه ایجاد می‌کند.",
    signals: "خرید اضطراری، تمام‌شدن مواد یا دورریز اقلام تاریخ‌دار.",
    action: "برای هر مادهٔ حیاتی زمان تأمین و پوشش موجودی را ثبت و نقطهٔ سفارش را با ماندگاری واقعی بازبینی کنید.",
    answerLikelihood: { no: 5, partial: 4, yes: null, unknown: 5 }, severity: 4, categoryRisk: "موجودی و برنامه‌ریزی",
    criticalReviewAnswers: [], sourceReference: null, reviewDate: null
  },
  {
    id: "substitution", category: "جایگزین و وابستگی", title: "جایگزین تأییدشده",
    prompt: "اگر ماده یا تأمین‌کنندهٔ اصلی در دسترس نباشد، آیا جایگزین آن از نظر کیفیت، فرمول و ایمنی بررسی شده است؟",
    risk: "وابستگی به منبع یا فرمول تک‌گزینه‌ای", impact: "اختلال یک منبع ممکن است تولید محصول وابسته را متوقف کند یا به تغییر کنترل‌نشدهٔ فرمول منجر شود.",
    signals: "یک منبع برای مادهٔ کلیدی یا جایگزینی بدون آزمون و تأیید داخلی.",
    action: "اقلام تک‌منبعی را مشخص کنید و جایگزین را پیش از استفاده از نظر کیفیت، فرمول و آلرژن‌ها ارزیابی کنید.",
    answerLikelihood: { no: 5, partial: 3, yes: null, unknown: 5 }, severity: 4, categoryRisk: "تأمین‌کننده و تداوم",
    criticalReviewAnswers: [], sourceReference: null, reviewDate: null
  },
  {
    id: "allergens", category: "آلرژن‌ها", title: "کنترل آلرژن در تأمین و تولید",
    prompt: "آیا وضعیت آلرژن مواد ورودی، جداسازی در نگهداری و تطابق اطلاعات محصول را کنترل می‌کنید؟",
    risk: "آلودگی متقاطع یا اطلاعات ناقص آلرژن", impact: "خطا در کنترل یا اعلام آلرژن می‌تواند برای مصرف‌کننده پیامد جدی داشته باشد.",
    signals: "تغییر تأمین‌کننده بدون بررسی مشخصات، ابزار مشترک یا برچسب بازبینی‌نشده.",
    action: "اطلاعات آلرژن مواد را ثبت کنید، جداسازی لازم را تعریف کنید و پیش از تغییر تأمین‌کننده مشخصات را دوباره بررسی کنید.",
    answerLikelihood: { no: 5, partial: 4, yes: null, unknown: 5 }, severity: 5, categoryRisk: "آلرژن و ایمنی غذا",
    criticalReviewAnswers: ["no", "partial", "unknown"],
    sourceReference: { title: "Code of Practice on Food Allergen Management for Food Business Operators (CXC 80-2020)", author: "Codex Alimentarius Commission", year: 2020, publisher: "FAO/WHO Codex Alimentarius", doi: "", url: "https://www.fao.org/fao-who-codexalimentarius/sh-proxy/en/?lnk=1&url=https%253A%252F%252Fworkspace.fao.org%252Fsites%252Fcodex%252FStandards%252FCXC+80-2020%252FCXC_080e.pdf" }, reviewDate: null
  },
  {
    id: "coldChain", category: "زنجیره سرد", title: "دمای دریافت و نگهداری",
    prompt: "آیا دمای دریافت و نگهداری مواد حساس مانند لبنیات، تخم‌مرغ یا خامه ثبت و در صورت انحراف پیگیری می‌شود؟",
    risk: "آسیب به مواد حساس به دما", impact: "انحراف دما می‌تواند فساد، ضایعات یا خطر ایمنی غذایی ایجاد کند.",
    signals: "نبود ثبت دما، تأخیر در انتقال به سردخانه یا نامشخص‌بودن واکنش به انحراف.",
    action: "حدود پذیرش و نگهداری را برای مواد حساس مشخص کنید، دما را ثبت کنید و برای محمولهٔ خارج از محدوده تصمیم ازپیش‌تعیین‌شده داشته باشید.",
    answerLikelihood: { no: 5, partial: 4, yes: null, unknown: 5 }, severity: 5, categoryRisk: "زنجیره سرد و ایمنی غذا",
    criticalReviewAnswers: ["no", "partial", "unknown"],
    sourceReference: { title: "General Principles of Food Hygiene (CXC 1-1969)", author: "Codex Alimentarius Commission", year: 2022, publisher: "FAO/WHO Codex Alimentarius", doi: "", url: "https://www.fao.org/fao-who-codexalimentarius/codex-texts/codes-of-practice/en/" }, reviewDate: null
  },
  {
    id: "transport", category: "حمل‌ونقل و بسته‌بندی", title: "حفظ کیفیت در جابه‌جایی",
    prompt: "آیا روش حمل و بسته‌بندی ورودی و خروجی با حساسیت مواد و محصول نهایی متناسب است؟",
    risk: "آسیب یا افت کیفیت در حمل‌ونقل", impact: "تأخیر، آلودگی یا تغییر شکل محصول می‌تواند باعث مرجوعی و اختلال در تحویل شود.",
    signals: "تأخیر پرتکرار، بسته‌بندی آسیب‌دیده یا نبود معیار دریافت و ارسال.",
    action: "شرایط حمل، تحویل و بسته‌بندی را برای اقلام حساس تعریف و شکایت‌ها یا محموله‌های آسیب‌دیده را ثبت کنید.",
    answerLikelihood: { no: 4, partial: 2, yes: null, unknown: 4 }, severity: 4, categoryRisk: "حمل‌ونقل و تحویل",
    criticalReviewAnswers: [],
    sourceReference: { title: "General Principles of Food Hygiene (CXC 1-1969)", author: "Codex Alimentarius Commission", year: 2022, publisher: "FAO/WHO Codex Alimentarius", doi: "", url: "https://www.fao.org/fao-who-codexalimentarius/codex-texts/codes-of-practice/en/" }, reviewDate: null
  },
  {
    id: "priceAvailability", category: "قیمت و دسترسی", title: "نوسان قیمت و دسترسی مواد",
    prompt: "آیا تغییر قیمت یا دشوارشدن دسترسی به مواد اصلی را پیش از اثرگذاری بر تولید و حاشیهٔ سود پیگیری می‌کنید؟",
    risk: "نوسان قیمت یا محدودیت دسترسی مواد", impact: "افزایش ناگهانی هزینه یا کمبود مواد می‌تواند برنامهٔ خرید و حاشیهٔ سود را به‌هم بزند.",
    signals: "خرید بدون مقایسه، تغییرهای ناگهانی قیمت یا وابستگی زیاد به یک بازار.",
    action: "برای چند قلم پرهزینه، روند قیمت و زمان تأمین را مرور و گزینه‌های خرید یا فرمول جایگزین را از قبل بررسی کنید.",
    answerLikelihood: { no: 5, partial: 3, yes: null, unknown: 5 }, severity: 3, categoryRisk: "قیمت و دسترسی",
    criticalReviewAnswers: [], sourceReference: null, reviewDate: null
  },
  {
    id: "continuity", category: "تداوم تولید", title: "وابستگی به تجهیزات و زیرساخت",
    prompt: "آیا برای خرابی فر، یخچال، برق یا تجهیزات گلوگاهی راهکار تداوم تولید و نگهداری دارید؟",
    risk: "توقف تولید بر اثر خرابی تجهیزات یا زیرساخت", impact: "خرابی گلوگاه تولید می‌تواند سفارش‌ها را عقب بیندازد و مواد فسادپذیر را از بین ببرد.",
    signals: "تعمیر اضطراری تکراری، نبود سرویس پیشگیرانه یا نداشتن راهکار برای قطع برق.",
    action: "گلوگاه‌ها را فهرست کنید، سرویس پیشگیرانه و واکنش به قطع برق یا خرابی را برای اقلام حساس مشخص کنید.",
    answerLikelihood: { no: 4, partial: 3, yes: null, unknown: 4 }, severity: 4, categoryRisk: "تداوم تولید",
    criticalReviewAnswers: [], sourceReference: null, reviewDate: null
  }
];

const ANSWERS = [
  { value: "yes", label: "بله، کنترل اجرا می‌شود" },
  { value: "partial", label: "تا حدی، کامل یا منظم نیست" },
  { value: "no", label: "خیر، کنترل نداریم" },
  { value: "unknown", label: "نامشخص، نیازمند بررسی" }
];
const PAGE_META = {
  dashboard: ["نمای کلی", "ریسک مواد اولیه، تأمین‌کننده‌ها و تداوم تولید را یک‌جا ببینید."],
  business: ["کسب‌وکار", "زمینهٔ فعالیتی که ارزیابی بر اساس آن انجام می‌شود."],
  supply: ["تأمین‌کننده و مواد", "اقلام حیاتی، وابستگی‌ها و پوشش موجودی."],
  assessment: ["ارزیابی ریسک", "کنترل‌های زنجیرهٔ تأمین را بر اساس شواهد مرور کنید."],
  risks: ["فهرست ریسک‌ها", "ریسک‌های فعال، دلیل شناسایی و اقدام پیشنهادی."],
  actions: ["برنامهٔ اقدام", "مسئولیت‌ها و موعدهای پیگیری."],
  report: ["گزارش مدیریتی", "خلاصهٔ قابل چاپ برای مرور وضعیت زنجیرهٔ تأمین."],
  settings: ["تنظیمات و داده‌ها", "پشتیبان‌گیری، بازیابی و پاک‌سازی داده‌ها."]
};

function emptyState() {
  return {
    schemaVersion: SCHEMA_VERSION,
    mode: "empty",
    business: { name: "", type: "قنادی با تولید روزانه", size: "۱ تا ۵ نفر", city: "", products: "", processes: "خرید، دریافت، نگهداری، تولید، بسته‌بندی، ارسال" },
    suppliers: [], materials: [], events: [], risks: [], actions: [],
    assessment: {
      status: "draft", questionnaireVersion: QUESTIONNAIRE_VERSION, rulesVersion: RULES_VERSION,
      answers: {}, calculatedAt: null, answerCount: 0, questionCount: QUESTIONS.length
    },
    createdAt: new Date().toISOString(), updatedAt: new Date().toISOString(),
    migrationNotice: false, legacyArchive: null
  };
}

function loadState() {
  try {
    const current = JSON.parse(localStorage.getItem(STORAGE_KEY) || "null");
    if (current && current.schemaVersion === SCHEMA_VERSION) return normalizeState(current);
    if (current && current.schemaVersion === 2) {
      const migrated = normalizeState(migrateV2State(current));
      try { localStorage.setItem(STORAGE_KEY, JSON.stringify(migrated)); } catch (error) { /* Keep the migrated data in memory if storage is full. */ }
      return migrated;
    }
    const legacy = JSON.parse(localStorage.getItem(LEGACY_STORAGE_KEY) || "null");
    if (legacy) {
      const migrated = emptyState();
      migrated.mode = legacy.business?.name?.includes("نمونه") ? "sample" : "real";
      migrated.business = {
        ...migrated.business,
        name: legacy.business?.name || "",
        type: legacy.business?.type === "تولید کیک و شیرینی" ? "تولید شیرینی و کیک" : legacy.business?.type || migrated.business.type,
        size: legacy.business?.size || migrated.business.size,
        city: legacy.business?.city || "",
        processes: legacy.business?.processes || migrated.business.processes
      };
      migrated.migrationNotice = true;
      migrated.legacyArchive = legacy;
      return migrated;
    }
  } catch (error) {
    return emptyState();
  }
  return emptyState();
}

function normalizeState(saved) {
  if (saved?.schemaVersion === 2) saved = migrateV2State(saved);
  const base = emptyState();
  const { answers: legacyAnswers, ...savedWithoutLegacyAnswers } = saved;
  const savedAnswers = saved.assessment?.answers && typeof saved.assessment.answers === "object" && !Array.isArray(saved.assessment.answers)
    ? saved.assessment.answers
    : legacyAnswers && typeof legacyAnswers === "object" && !Array.isArray(legacyAnswers) ? legacyAnswers : {};
  return {
    ...base, ...savedWithoutLegacyAnswers,
    business: { ...base.business, ...(saved.business || {}) },
    suppliers: Array.isArray(saved.suppliers) ? saved.suppliers : [],
    materials: Array.isArray(saved.materials) ? saved.materials : [],
    events: Array.isArray(saved.events) ? saved.events : [],
    risks: Array.isArray(saved.risks) ? saved.risks : [],
    actions: Array.isArray(saved.actions) ? saved.actions : [],
    assessment: { ...base.assessment, ...(saved.assessment || {}), answers: savedAnswers }
  };
}

function migrateV2State(saved) {
  const previousAnswers = saved.assessment?.answers || saved.answers || {};
  const migrated = { ...saved, schemaVersion: SCHEMA_VERSION };
  delete migrated.answers;
  migrated.assessment = {
    ...(saved.assessment || {}),
    questionnaireVersion: saved.assessment?.questionnaireVersion || "legacy-unversioned",
    rulesVersion: saved.assessment?.rulesVersion || "legacy-unversioned",
    answers: previousAnswers,
    status: saved.assessment?.calculatedAt ? "needs_recalculation" : saved.assessment?.status || "draft"
  };
  return migrated;
}

let state = loadState();
let toastTimer = null;
let activePage = "dashboard";

function saveState() {
  state.schemaVersion = SCHEMA_VERSION;
  state.updatedAt = new Date().toISOString();
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    return true;
  } catch (error) {
    showToast("ذخیره‌سازی ناموفق بود. از خروجی JSON پشتیبان بگیرید.");
    return false;
  }
}

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, char => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[char]));
}

function safeExternalUrl(value) {
  try {
    const url = new URL(value, window.location.href);
    return url.protocol === "https:" ? url.href : "";
  } catch (error) {
    return "";
  }
}

function renderSourceMetadata(risk) {
  const reference = risk.sourceReference;
  if (!reference || typeof reference !== "object") return "";
  const url = safeExternalUrl(reference.url);
  const title = escapeHtml(reference.title || "منبع بدون عنوان");
  const citation = [reference.author, reference.year, reference.publisher, reference.doi ? `DOI: ${reference.doi}` : ""]
    .filter(Boolean).map(escapeHtml).join(" · ");
  const statusUrl = safeExternalUrl(reference.statusUrl);
  const status = reference.statusNote ? `${escapeHtml(reference.statusNote)}${statusUrl ? ` <a href="${escapeHtml(statusUrl)}" target="_blank" rel="noopener noreferrer">وضعیت بازنگری</a>` : ""}` : "";
  return `<div class="risk-source"><strong>منبع راهنما</strong><span>${url ? `<a href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer">${title}</a>` : title}</span>${citation ? `<small>${citation}</small>` : ""}${status ? `<small>${status}</small>` : ""}<small>تاریخ بازبینی تخصصی محتوا: ${escapeHtml(risk.reviewDate || "ثبت نشده")}. این منبع از موضوع کنترل پشتیبانی می‌کند و مقدار احتمال عددی را اعتبارسنجی نمی‌کند.</small></div>`;
}

function faNumber(value) { return Number(value || 0).toLocaleString("fa-IR"); }
function localDateISO(date = new Date()) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}
function todayISO() { return localDateISO(); }
function formatDate(value) {
  if (!value) return "ثبت نشده";
  const date = new Date(value.length === 10 ? `${value}T12:00:00` : value);
  return Number.isNaN(date.getTime()) ? "نامعتبر" : new Intl.DateTimeFormat("fa-IR", { dateStyle: "medium" }).format(date);
}
function formatDateTime(value) {
  if (!value) return "ثبت نشده";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "ثبت نشده" : new Intl.DateTimeFormat("fa-IR", { dateStyle: "medium", timeStyle: "short" }).format(date);
}
function makeId(prefix) { return `${prefix}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 7)}`; }
function showToast(message) {
  const toast = document.getElementById("appToast");
  if (!toast) return;
  toast.textContent = message;
  toast.classList.add("is-visible");
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.classList.remove("is-visible"), 3600);
}

function hasAnyData() {
  return Boolean(state.business.name || state.suppliers.length || state.materials.length || state.events.length || Object.keys(state.assessment.answers || {}).length);
}
function isOverdue(action) {
  return action.status !== "تکمیل‌شده" && action.dueDate && action.dueDate < todayISO();
}
function markAssessmentStale() {
  if (state.assessment.calculatedAt) state.assessment.status = "needs_recalculation";
}

function showPage(page) {
  if (!PAGE_META[page]) return;
  activePage = page;
  document.querySelectorAll(".page-section").forEach(section => section.classList.toggle("hidden", section.id !== `page-${page}`));
  document.querySelectorAll(".nav-link[data-page]").forEach(button => {
    const isActive = button.dataset.page === page;
    button.classList.toggle("active", isActive);
    if (isActive) button.setAttribute("aria-current", "page"); else button.removeAttribute("aria-current");
  });
  document.getElementById("pageTitle").textContent = PAGE_META[page][0];
  document.getElementById("pageSubtitle").textContent = PAGE_META[page][1];
  renderAll();
  window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth" });
}

function renderAll() {
  renderHeader();
  renderDashboard();
  renderBusiness();
  renderSupply();
  renderAssessment();
  renderRisks();
  renderActions();
  renderReport();
  renderSettings();
}

function renderHeader() {
  const pill = document.getElementById("modePill");
  const modes = { empty: ["بدون داده", "mode-empty"], real: ["دادهٔ کسب‌وکار", "mode-real"], sample: ["نمونهٔ نمایشی", "mode-sample"] };
  const [label, className] = modes[state.mode] || modes.real;
  pill.textContent = label;
  pill.className = `mode-pill ${className}`;
  const banner = document.getElementById("stateBanner");
  const stale = state.assessment.status === "needs_recalculation";
  if (state.migrationNotice) {
    banner.innerHTML = `<span class="info-icon">i</span><span>اطلاعات نسخهٔ قبلی برای پشتیبان نگه داشته شده است. این نسخه برای زنجیرهٔ تأمین، ارزیابی و داده‌های جدید می‌سازد؛ نسخهٔ قبلی را از خروجی JSON دریافت کنید.</span><button class="banner-close" type="button" aria-label="بستن">×</button>`;
    banner.classList.remove("hidden");
  } else if (state.mode === "sample") {
    const staleNote = stale ? " نتیجهٔ فعلی هم نیازمند محاسبهٔ دوباره است." : "";
    banner.innerHTML = `<span class="info-icon">✳</span><span>دادهٔ نمونه است و وضعیت واقعی کسب‌وکار شما را نشان نمی‌دهد.${staleNote}</span><button class="banner-recalc" id="switchToReal" type="button">شروع با دادهٔ واقعی</button>`;
    banner.classList.remove("hidden");
  } else if (stale) {
    banner.innerHTML = `<span class="info-icon">!</span><span>مواد، تأمین‌کننده‌ها یا پاسخ‌ها پس از آخرین محاسبه تغییر کرده‌اند؛ نتیجهٔ فعلی نیازمند محاسبهٔ دوباره است.</span><button class="banner-recalc" type="button">محاسبهٔ دوباره</button>`;
    banner.classList.remove("hidden");
  } else {
    banner.classList.add("hidden");
    banner.innerHTML = "";
  }
}

function riskLevel(score) { return score >= RISK_THRESHOLDS.high ? "high" : score >= RISK_THRESHOLDS.medium ? "medium" : "low"; }
function riskLevelLabel(level) { return ({ high: "بالا", medium: "متوسط", low: "پایین" })[level] || "بررسی"; }
function answerLabel(answer) { return (ANSWERS.find(item => item.value === answer) || {}).label || "نامشخص"; }
function riskBadge(level) { return `<span class="risk-badge risk-${level}">ریسک ${riskLevelLabel(level)}</span>`; }
function scoreRisk(likelihood, severity) {
  if (!Number.isInteger(likelihood) || likelihood < 1 || likelihood > 5 || !Number.isInteger(severity) || severity < 1 || severity > 5) {
    throw new RangeError("Risk likelihood and severity must be integers from 1 to 5.");
  }
  return likelihood * severity;
}
function makeRisk({ id, title, category, impact, signals, action, likelihood, severity, reason, answer = null, materialId = null, supplierId = null, source, riskDefinitionId = id, criticalReview = false, reviewStatus = null, sourceReference = null, reviewDate = null, triggeredBy = [] }) {
  const score = scoreRisk(likelihood, severity);
  return {
    id, riskDefinitionId, title, category, impact, signals, action, likelihood, severity, score,
    level: riskLevel(score), reason, answer, materialId, supplierId, source, active: true,
    rulesVersion: RULES_VERSION, criticalReview, reviewStatus: reviewStatus || (criticalReview ? "urgent" : "open"),
    sourceReference, reviewDate, triggeredBy
  };
}

function calculateRisks(options = {}) {
  const answers = state.assessment.answers || {};
  const previousActions = new Map(state.risks.map(risk => [risk.id, risk.action]));
  const answeredQuestions = QUESTIONS.filter(question => answers[question.id]);
  if (!answeredQuestions.length && !state.materials.length && !state.suppliers.length && !state.events.length) {
    showToast("ابتدا چند پاسخ بدهید یا مواد اولیه و تأمین‌کننده‌ها را ثبت کنید.");
    return;
  }

  const results = [];
  QUESTIONS.forEach(question => {
    const answer = answers[question.id];
    if (!answer) return;
    const likelihood = question.answerLikelihood?.[answer];
    if (likelihood == null) return;
    const answerReason = answer === "unknown"
      ? "وضعیت این کنترل مشخص نیست؛ ابتدا شواهد و روش اجرا را بررسی کنید."
      : answer === "partial"
        ? "کنترل وجود دارد، اما اجرای آن کامل یا منظم نیست."
        : "پاسخ نشان می‌دهد این کنترل در حال حاضر اجرا نمی‌شود.";
    const criticalReview = question.criticalReviewAnswers.includes(answer);
    results.push(makeRisk({
      id: `question:${question.id}`, title: question.risk, category: question.categoryRisk,
      impact: question.impact, signals: question.signals,
      action: criticalReview ? `پیش از مصرف یا آزادسازی محموله، مسئول کیفیت باید موضوع را فوراً بررسی و تصمیم را مستند کند. ${question.action}` : question.action,
      likelihood, severity: question.severity, answer, riskDefinitionId: question.id,
      criticalReview, reviewStatus: criticalReview ? "urgent" : answer === "unknown" ? "needs_review" : "open",
      sourceReference: question.sourceReference,
      reviewDate: question.reviewDate, triggeredBy: [question.id],
      reason: `${answerReason} پاسخ مرتبط: ${answerLabel(answer)}`, source: "questionnaire"
    }));
  });

  const suppliersById = new Map(state.suppliers.map(supplier => [supplier.id, supplier]));
  state.materials.forEach(material => {
    const importance = material.criticality === "high" ? 5 : material.criticality === "medium" ? 4 : 3;
    const primary = suppliersById.get(material.primarySupplierId);
    const alternate = suppliersById.get(material.alternateSupplierId);
    const itemTitle = material.name || "مادهٔ بدون نام";
    if (material.criticality !== "low" && (!alternate || !material.alternateQualified)) {
      results.push(makeRisk({
        id: `material:alternate:${material.id}`, title: `وابستگی تک‌منبعی: ${itemTitle}`,
        category: "تأمین‌کننده و تداوم", impact: `نبود جایگزین ثبت‌شده برای ${itemTitle} می‌تواند اختلال یک منبع را به توقف تولید تبدیل کند.`,
        signals: "فقط یک منبع یا نبود اطلاعات جایگزین تأییدشده.",
        action: `برای ${itemTitle} منبع جایگزین پیدا و پیش از اتکا، کیفیت و تناسب آن را ارزیابی کنید.`,
        likelihood: 4, severity: importance,
        reason: alternate ? `تأمین‌کنندهٔ جایگزین ${alternate.name} برای ${itemTitle} ثبت شده، اما هنوز از نظر کیفیت و فرمول تأیید نشده است.` : `برای ${itemTitle} تأمین‌کنندهٔ جایگزین ثبت نشده است؛ ممکن است خارج از سامانه وجود داشته باشد و باید تأیید شود.`,
        materialId: material.id, supplierId: material.primarySupplierId || null, source: "material"
      }));
    }
    if (material.criticality !== "low" && !primary) {
      results.push(makeRisk({
        id: `material:source:${material.id}`, title: `منبع اصلی ثبت نشده: ${itemTitle}`,
        category: "پوشش اطلاعات تأمین", impact: `بدون منبع مشخص برای ${itemTitle} سنجش زمان تأمین و وابستگی ممکن نیست.`,
        signals: "تأمین‌کنندهٔ اصلی در فهرست مواد انتخاب نشده است.",
        action: `تأمین‌کنندهٔ اصلی ${itemTitle} و زمان معمول تأمین را ثبت کنید.`,
        likelihood: 3, severity: importance,
        reason: `اطلاعات منبع اصلی ${itemTitle} در فهرست ثبت نشده است؛ این مورد ابتدا نیازمند تکمیل داده است.`,
        materialId: material.id, source: "material"
      }));
    }
    const leadTime = Number(material.leadTimeDays);
    const stockCover = Number(material.stockCoverDays);
    if (material.criticality !== "low" && material.leadTimeDays !== "" && material.stockCoverDays !== "" && stockCover < leadTime) {
      const likelihood = stockCover === 0 ? 5 : leadTime - stockCover >= 7 ? 4 : 3;
      results.push(makeRisk({
        id: `material:stock:${material.id}`, title: `شکاف پوشش موجودی: ${itemTitle}`,
        category: "موجودی و برنامه‌ریزی", impact: `پوشش ثبت‌شدهٔ ${faNumber(stockCover)} روز از زمان تأمین ${faNumber(leadTime)} روز کمتر است.`,
        signals: `اختلاف ${faNumber(leadTime - stockCover)} روز بین پوشش موجودی و زمان تأمین.`,
        action: `برای ${itemTitle} نقطهٔ سفارش یا موجودی اطمینان را بازبینی کنید؛ ماندگاری و خطر ضایعات را هم در نظر بگیرید.`,
        likelihood, severity: importance,
        reason: `زمان تأمین ثبت‌شده از پوشش موجودی بیشتر است (${faNumber(leadTime)} در برابر ${faNumber(stockCover)} روز).`,
        materialId: material.id, supplierId: material.primarySupplierId || null, source: "material"
      }));
    }
    if (primary?.status === "risk") {
      results.push(makeRisk({
        id: `supplier:status:${primary.id}:${material.id}`, title: `تأمین‌کنندهٔ نیازمند اقدام: ${primary.name}`,
        category: "عملکرد تأمین‌کننده", impact: `وضعیت ${primary.name} برای مادهٔ ${itemTitle} پرریسک علامت‌گذاری شده است.`,
        signals: "وضعیت تأمین‌کننده در فهرست به‌عنوان پرریسک ثبت شده است.",
        action: `علت پرریسک‌شدن ${primary.name} را مستند کنید و برای ${itemTitle} برنامهٔ جایگزین داشته باشید.`,
        likelihood: 4, severity: importance,
        reason: `تأمین‌کنندهٔ اصلی ${itemTitle} در فهرست نیازمند رسیدگی علامت‌گذاری شده است.`,
        materialId: material.id, supplierId: primary.id, source: "supplier"
      }));
    }
  });

  const cutoff = new Date(); cutoff.setDate(cutoff.getDate() - 90);
  const cutoffISO = localDateISO(cutoff);
  const recentOpenEvents = state.events.filter(item => !item.resolved && item.eventDate >= cutoffISO);
  const eventGroups = new Map();
  recentOpenEvents.forEach(item => {
    const targetId = item.materialId || item.supplierId || "general";
    const key = `${item.eventType}:${targetId}`;
    if (!eventGroups.has(key)) eventGroups.set(key, []);
    eventGroups.get(key).push(item);
  });
  const eventNames = { delay: "تأخیر در تحویل", shortage: "کسری یا کمبود", quality: "مشکل کیفیت", temperature: "انحراف دما", packaging: "آسیب بسته‌بندی", other: "رخداد تأمین" };
  eventGroups.forEach((items, key) => {
    const severe = items.some(item => item.severity === "high");
    if (!severe && items.length < 2) return;
    const [type] = key.split(":");
    const first = items[0];
    const material = state.materials.find(item => item.id === first.materialId);
    const supplier = suppliersById.get(first.supplierId);
    const target = material?.name || supplier?.name || "زنجیرهٔ تأمین";
    const severity = ["quality", "temperature"].includes(type) ? 5 : 4;
    const count = items.length;
    const criticalReview = ["quality", "temperature"].includes(type);
    results.push(makeRisk({
      id: `event:${key}`, title: `${eventNames[type] || "رخداد تأمین"} برای ${target}`,
      category: type === "temperature" || type === "quality" ? "کیفیت و ایمنی مواد" : "اختلال تأمین",
      impact: type === "temperature" || type === "quality" ? "تکرار یا شدت رخداد می‌تواند کیفیت یا ایمنی مواد ورودی را تحت تأثیر قرار دهد." : "تکرار اختلال می‌تواند زمان تأمین و تداوم تولید را تحت تأثیر قرار دهد.",
      signals: `${faNumber(count)} رخداد باز در ۹۰ روز گذشته ثبت شده است.`,
      action: `${criticalReview ? "پیش از مصرف یا آزادسازی محموله، مسئول کیفیت باید موضوع را فوراً بررسی و تصمیم را مستند کند. " : ""}علت رخدادهای ${eventNames[type] || "تأمین"} برای ${target} را بررسی و اقدام پیشگیرانه و مسیر جایگزین تعریف کنید.`,
      likelihood: severe ? 5 : Math.min(4, 2 + count), severity,
      reason: `${faNumber(count)} رخداد باز از نوع «${eventNames[type] || "سایر"}» در ۹۰ روز گذشته ثبت شده است؛ ${severe ? "حداقل یک رخداد با شدت زیاد بوده است." : "رخداد مشابه تکرار شده است."}`,
      criticalReview, triggeredBy: items.map(item => item.id),
      materialId: material?.id || null, supplierId: supplier?.id || null, source: "event"
    }));
  });

  const uniqueResults = [...new Map(results.map(risk => [risk.id, risk])).values()];
  state.risks = uniqueResults.sort((a, b) => b.score - a.score);
  const activeIds = new Set(state.risks.map(risk => risk.id));
  state.actions.forEach(action => { action.active = activeIds.has(action.riskId); });
  state.risks.forEach(risk => {
    let action = state.actions.find(item => item.riskId === risk.id);
    if (!action) {
      action = { id: makeId("action"), riskId: risk.id, title: risk.action, owner: "", dueDate: "", status: "باز", notes: "", active: true, createdAt: new Date().toISOString() };
      state.actions.push(action);
    } else {
      action.active = true;
      if (action.status !== "تکمیل‌شده" && (!action.title || action.title === previousActions.get(risk.id))) action.title = risk.action;
    }
  });
  state.assessment = {
    ...state.assessment, status: "current", questionnaireVersion: QUESTIONNAIRE_VERSION, rulesVersion: RULES_VERSION,
    calculatedAt: new Date().toISOString(), answerCount: answeredQuestions.length, questionCount: QUESTIONS.length
  };
  saveState();
  renderAll();
  if (!options.silent) {
    showToast(`${faNumber(state.risks.length)} ریسک برای پیگیری ثبت شد.`);
    showPage("risks");
  }
}

function renderDashboard() {
  const isConfigured = Boolean(state.business.name);
  document.getElementById("onboardingPanel").classList.toggle("hidden", isConfigured || state.mode === "sample");
  const activeRisks = state.risks.filter(risk => risk.active !== false);
  const overdue = state.actions.filter(action => action.active && isOverdue(action));
  document.getElementById("metricRisks").textContent = faNumber(activeRisks.length);
  document.getElementById("metricHigh").textContent = faNumber(activeRisks.filter(risk => risk.level === "high").length);
  document.getElementById("metricMaterials").textContent = faNumber(state.materials.filter(material => material.criticality === "high").length);
  document.getElementById("metricOverdue").textContent = faNumber(overdue.length);

  const priorityRoot = document.getElementById("dashboardRisks");
  const topRisks = [...activeRisks].sort((a, b) => Number(Boolean(b.criticalReview)) - Number(Boolean(a.criticalReview)) || Number(b.reviewStatus === "needs_review") - Number(a.reviewStatus === "needs_review") || b.score - a.score).slice(0, 4);
  priorityRoot.innerHTML = topRisks.length ? topRisks.map(risk => `
    <div class="priority-item"><div class="priority-marker marker-${risk.level}"></div><div class="priority-copy"><strong>${escapeHtml(risk.title)}</strong><small>${risk.criticalReview ? `<span class="critical-review-flag">بازبینی فوری انسانی</span> ` : risk.reviewStatus === "needs_review" ? `<span class="review-needed-flag">نیازمند بررسی</span> ` : ""}${escapeHtml(risk.category)} · ${escapeHtml(risk.reason)}</small></div><div class="priority-score"><b>${faNumber(risk.score)}</b><small>از ۲۵</small></div></div>`).join("")
    : `<div class="empty-state compact-empty"><span class="empty-icon">✳</span><strong>هنوز ارزیابی محاسبه نشده</strong><p>مواد و تأمین‌کننده‌ها را ثبت کنید و ارزیابی را انجام دهید.</p><button class="text-button" data-go="assessment" type="button">شروع ارزیابی ←</button></div>`;

  const coverageRoot = document.getElementById("supplyCoverage");
  if (!state.materials.length) {
    coverageRoot.innerHTML = `<div class="empty-inline">هنوز ماده‌ای ثبت نشده است.<button class="text-button" data-go="supply" type="button">ثبت مواد اولیه ←</button></div>`;
  } else {
    const important = state.materials.filter(material => material.criticality !== "low");
    const alternativeCount = important.filter(material => Boolean(material.alternateSupplierId && material.alternateQualified)).length;
    const tightCount = important.filter(material => material.leadTimeDays !== "" && material.stockCoverDays !== "" && Number(material.stockCoverDays) < Number(material.leadTimeDays)).length;
    const infoCount = important.filter(material => material.leadTimeDays === "" || material.stockCoverDays === "").length;
    coverageRoot.innerHTML = `<div class="coverage-stat"><div><span>اقلام مهم با جایگزین تأییدشده</span><strong>${faNumber(alternativeCount)} <small>از ${faNumber(important.length)}</small></strong></div><div class="coverage-track"><span style="width:${important.length ? Math.round(alternativeCount / important.length * 100) : 0}%"></span></div></div>
      <div class="coverage-detail"><span><i class="status-dot dot-warning"></i>${faNumber(tightCount)} قلم با پوشش کمتر از زمان تأمین</span><span><i class="status-dot dot-muted"></i>${faNumber(infoCount)} قلم با اطلاعات موجودی ناقص</span></div>`;
  }
  const actionRoot = document.getElementById("dashboardActions");
  const upcoming = state.actions.filter(action => action.active && action.status !== "تکمیل‌شده").sort((a, b) => (a.dueDate || "9999").localeCompare(b.dueDate || "9999")).slice(0, 3);
  actionRoot.innerHTML = upcoming.length ? upcoming.map(action => `<div class="mini-action"><span class="mini-action-dot ${isOverdue(action) ? "is-overdue" : ""}"></span><div><strong>${escapeHtml(action.title)}</strong><small>${escapeHtml(action.owner || "مسئول تعیین نشده")} · ${escapeHtml(formatDate(action.dueDate))}</small></div></div>`).join("") : `<div class="empty-inline">اقدام بازی برای نمایش وجود ندارد.</div>`;
}

function renderBusiness() {
  const form = document.getElementById("businessForm");
  if (form.dataset.loaded === "true") return;
  form.elements.name.value = state.business.name || "";
  form.elements.type.value = state.business.type || "قنادی با تولید روزانه";
  form.elements.size.value = state.business.size || "۱ تا ۵ نفر";
  form.elements.city.value = state.business.city || "";
  form.elements.products.value = state.business.products || "";
  form.elements.processes.value = state.business.processes || "";
  form.dataset.loaded = "true";
}

function coverageStatus(material) {
  if (material.leadTimeDays === "" || material.stockCoverDays === "") return ["اطلاعات ناقص", "coverage-unknown"];
  return Number(material.stockCoverDays) < Number(material.leadTimeDays) ? ["نیازمند توجه", "coverage-risk"] : ["پوشش مناسب", "coverage-good"];
}
function criticalityLabel(value) { return ({ high: "حیاتی", medium: "مهم", low: "عادی" })[value] || "مهم"; }
function supplierName(id) { return state.suppliers.find(supplier => supplier.id === id)?.name || "ثبت نشده"; }
function renderSupply() {
  const important = state.materials.filter(material => material.criticality !== "low");
  const atRisk = important.filter(material => !material.alternateSupplierId || !material.alternateQualified || (material.leadTimeDays !== "" && material.stockCoverDays !== "" && Number(material.stockCoverDays) < Number(material.leadTimeDays))).length;
  document.getElementById("supplySummary").innerHTML = `<div class="supply-summary-card"><span>همهٔ مواد</span><strong>${faNumber(state.materials.length)}</strong></div><div class="supply-summary-card"><span>مواد حیاتی و مهم</span><strong>${faNumber(important.length)}</strong></div><div class="supply-summary-card summary-attention"><span>نیازمند بررسی بیشتر</span><strong>${faNumber(atRisk)}</strong></div><div class="supply-summary-card"><span>تأمین‌کننده‌ها</span><strong>${faNumber(state.suppliers.length)}</strong></div>`;

  const materialRoot = document.getElementById("materialsTable");
  materialRoot.innerHTML = state.materials.length ? `<table><thead><tr><th>ماده</th><th>اهمیت</th><th>تأمین اصلی / جایگزین</th><th>تأمین در برابر موجودی</th><th>شرایط</th><th><span class="sr-only">عملیات</span></th></tr></thead><tbody>${state.materials.map(material => {
    const [coverageText, coverageClass] = coverageStatus(material);
    const alternateLabel = material.alternateSupplierId ? `${supplierName(material.alternateSupplierId)} · ${material.alternateQualified ? "تأییدشده" : "نیازمند ارزیابی"}` : "ثبت نشده";
    return `<tr><td><strong>${escapeHtml(material.name)}</strong><small>${escapeHtml(material.category)}</small></td><td><span class="criticality critical-${material.criticality}">${criticalityLabel(material.criticality)}</span></td><td><span>${escapeHtml(supplierName(material.primarySupplierId))}</span><small>جایگزین: ${escapeHtml(alternateLabel)}</small></td><td><span class="coverage-pill ${coverageClass}">${coverageText}</span><small>تأمین ${material.leadTimeDays === "" ? "—" : `${faNumber(material.leadTimeDays)} روز`} · موجودی ${material.stockCoverDays === "" ? "—" : `${faNumber(material.stockCoverDays)} روز`}</small></td><td>${escapeHtml(material.storage || "ثبت نشده")}<small>ماندگاری: ${material.shelfLifeDays ? `${faNumber(material.shelfLifeDays)} روز` : "ثبت نشده"}</small></td><td class="table-actions"><button class="icon-action" data-edit-material="${escapeHtml(material.id)}" type="button" aria-label="ویرایش ${escapeHtml(material.name)}">ویرایش</button><button class="icon-action icon-action-danger" data-delete-material="${escapeHtml(material.id)}" type="button" aria-label="حذف ${escapeHtml(material.name)}">حذف</button></td></tr>`;
  }).join("")}</tbody></table>` : `<div class="empty-state"><span class="empty-icon">▦</span><strong>فهرست مواد اولیه خالی است</strong><p>از آرد، لبنیات، تخم‌مرغ، شکلات یا بسته‌بندیِ اثرگذار بر تولید شروع کنید.</p><button class="button button-primary button-small" data-add-material type="button">＋ افزودن مادهٔ اولیه</button></div>`;

  const supplierRoot = document.getElementById("suppliersTable");
  supplierRoot.innerHTML = state.suppliers.length ? `<table><thead><tr><th>تأمین‌کننده</th><th>گروه / محل</th><th>وضعیت</th><th>مواد مرتبط</th><th><span class="sr-only">عملیات</span></th></tr></thead><tbody>${state.suppliers.map(supplier => {
    const linked = state.materials.filter(material => material.primarySupplierId === supplier.id || material.alternateSupplierId === supplier.id).length;
    const statusText = { normal: "عادی", watch: "نیازمند پایش", risk: "پرریسک" }[supplier.status] || "عادی";
    return `<tr><td><strong>${escapeHtml(supplier.name)}</strong></td><td>${escapeHtml(supplier.category || "سایر")}<small>${escapeHtml(supplier.city || "محل ثبت نشده")}</small></td><td><span class="supplier-status status-${supplier.status}"><i></i>${statusText}</span></td><td>${faNumber(linked)} قلم</td><td class="table-actions"><button class="icon-action" data-edit-supplier="${escapeHtml(supplier.id)}" type="button" aria-label="ویرایش ${escapeHtml(supplier.name)}">ویرایش</button><button class="icon-action icon-action-danger" data-delete-supplier="${escapeHtml(supplier.id)}" type="button" aria-label="حذف ${escapeHtml(supplier.name)}">حذف</button></td></tr>`;
  }).join("")}</tbody></table>` : `<div class="empty-state compact-empty"><strong>هنوز تأمین‌کننده‌ای ثبت نشده</strong><p>برای مواد حیاتی، منبع اصلی و جایگزین را مشخص کنید.</p><button class="button button-secondary button-small" data-add-supplier type="button">＋ افزودن تأمین‌کننده</button></div>`;

  const eventRoot = document.getElementById("eventsTable");
  const eventTypes = { delay: "تأخیر در تحویل", shortage: "کسری یا کمبود", quality: "مشکل کیفیت", temperature: "انحراف دما", packaging: "آسیب بسته‌بندی", other: "سایر" };
  const events = [...state.events].sort((a, b) => (b.eventDate || "").localeCompare(a.eventDate || ""));
  eventRoot.innerHTML = events.length ? `<table><thead><tr><th>تاریخ / نوع</th><th>ماده / تأمین‌کننده</th><th>شدت</th><th>شرح</th><th>پیگیری</th></tr></thead><tbody>${events.map(item => `<tr><td><strong>${escapeHtml(formatDate(item.eventDate))}</strong><small>${eventTypes[item.eventType] || "سایر"}</small></td><td>${escapeHtml(state.materials.find(material => material.id === item.materialId)?.name || "بدون مادهٔ مشخص")}<small>${escapeHtml(supplierName(item.supplierId))}</small></td><td><span class="event-severity severity-${item.severity}">${({ low: "کم", medium: "متوسط", high: "زیاد" })[item.severity] || "متوسط"}</span></td><td class="event-description">${escapeHtml(item.description)}</td><td><button class="icon-action ${item.resolved ? "" : "icon-action-danger"}" type="button" data-toggle-event="${escapeHtml(item.id)}">${item.resolved ? "بازگشایی" : "بستن رخداد"}</button><small>${item.resolved ? "بسته‌شده" : "باز"}</small></td></tr>`).join("")}</tbody></table>` : `<div class="empty-state compact-empty"><strong>رخدادی ثبت نشده</strong><p>ثبت اختلال‌های واقعی کمک می‌کند ریسک تکرارشونده دیده شود.</p><button class="button button-secondary button-small" data-add-event type="button">＋ ثبت رخداد</button></div>`;
}

function renderAssessment() {
  const answers = state.assessment.answers || {};
  const answered = Object.keys(answers).filter(id => answers[id]).length;
  const progress = Math.round(answered / QUESTIONS.length * 100);
  document.getElementById("assessmentProgressText").textContent = `${faNumber(answered)} از ${faNumber(QUESTIONS.length)} پاسخ`;
  document.getElementById("assessmentProgressBar").style.width = `${progress}%`;
  const container = document.getElementById("questionnaire");
  if (container.dataset.rendered === "true") {
    container.querySelectorAll("input[type=radio]").forEach(input => { input.checked = state.assessment.answers[input.name] === input.value; });
    return;
  }
  container.innerHTML = QUESTIONS.map((question, index) => `<article class="question-card">
    <div class="question-heading"><span class="question-number">${faNumber(index + 1)}</span><div><span class="question-category">${escapeHtml(question.category)}</span><h3>${escapeHtml(question.title)}</h3></div><span class="answer-state" data-answer-state="${question.id}">پاسخ نداده‌اید</span></div>
    <p class="question-prompt">${escapeHtml(question.prompt)}</p>
    <div class="answer-options" role="radiogroup" aria-label="${escapeHtml(question.title)}">${ANSWERS.map(answer => `<label class="answer-option"><input type="radio" name="${question.id}" value="${answer.value}" ${state.assessment.answers[question.id] === answer.value ? "checked" : ""}><span>${answer.label}</span></label>`).join("")}</div>
  </article>`).join("");
  container.dataset.rendered = "true";
  updateAnswerStates();
}

function updateAnswerStates() {
  QUESTIONS.forEach(question => {
    const badge = document.querySelector(`[data-answer-state="${question.id}"]`);
    if (!badge) return;
    const answer = state.assessment.answers[question.id];
    badge.textContent = answer ? answer === "yes" ? "کنترل ثبت شد" : answer === "unknown" ? "نیازمند بررسی" : "نیازمند اقدام" : "پاسخ نداده‌اید";
    badge.className = `answer-state ${answer ? answer === "yes" ? "answer-good" : answer === "unknown" ? "answer-unknown" : "answer-needs-action" : ""}`;
  });
}

function renderRisks() {
  const root = document.getElementById("riskList");
  const search = (document.getElementById("riskSearch")?.value || "").trim().toLocaleLowerCase("fa");
  const levelFilter = document.getElementById("riskLevelFilter")?.value || "all";
  const allRisks = state.risks.filter(risk => risk.active !== false);
  const filtered = allRisks.filter(risk => (levelFilter === "all" || risk.level === levelFilter) && `${risk.title} ${risk.category} ${risk.reason}`.toLocaleLowerCase("fa").includes(search)).sort((a, b) => Number(Boolean(b.criticalReview)) - Number(Boolean(a.criticalReview)) || Number(b.reviewStatus === "needs_review") - Number(a.reviewStatus === "needs_review") || b.score - a.score);
  const status = document.getElementById("riskListStatus");
  if (state.assessment.status === "needs_recalculation") status.innerHTML = `<div class="inline-alert alert-warning"><span>!</span> نتیجهٔ ذخیره‌شده به نسخهٔ قواعد ${escapeHtml(state.assessment.rulesVersion)} تعلق دارد یا داده‌ها تغییر کرده‌اند؛ محاسبهٔ دوباره، امتیاز و هشدارهای جاری را جایگزین می‌کند.</div>`;
  else if (!state.assessment.calculatedAt) status.innerHTML = `<div class="inline-alert"><span>i</span> هنوز نتیجه‌ای محاسبه نشده است. ارزیابی را انجام دهید تا ریسک‌های وابسته به اطلاعات فعلی نمایش داده شوند.</div>`;
  else status.innerHTML = `<div class="result-meta"><span>آخرین محاسبه: ${escapeHtml(formatDateTime(state.assessment.calculatedAt))}</span><span>${faNumber(state.assessment.answerCount)} از ${faNumber(QUESTIONS.length)} پرسش پاسخ داده شده</span><span>نمایش ${faNumber(filtered.length)} از ${faNumber(allRisks.length)} ریسک</span></div>`;
  root.innerHTML = filtered.length ? filtered.map(risk => `<article class="risk-card">
    <div class="risk-card-head"><div><div class="risk-meta"><span class="risk-category">${escapeHtml(risk.category)}</span>${risk.source === "material" ? `<span class="source-pill">مادهٔ اولیه</span>` : risk.source === "supplier" ? `<span class="source-pill">تأمین‌کننده</span>` : risk.source === "event" ? `<span class="source-pill">رخداد</span>` : ""}${risk.criticalReview ? `<span class="critical-review-flag">بازبینی فوری انسانی</span>` : risk.reviewStatus === "needs_review" ? `<span class="review-needed-flag">نیازمند بررسی</span>` : ""}</div><h3>${escapeHtml(risk.title)}</h3></div><div class="risk-score-box">${riskBadge(risk.level)}<strong>${faNumber(risk.score)}<small>/۲۵</small></strong></div></div>
    <div class="risk-explanation"><div><strong>چرا شناسایی شد؟</strong><p>${escapeHtml(risk.reason)}</p></div><div><strong>پیامد احتمالی</strong><p>${escapeHtml(risk.impact)}</p></div></div>
    <div class="risk-detail-row"><span><i class="detail-icon">◉</i> احتمال فعلی: ${faNumber(risk.likelihood)} از ۵</span><span><i class="detail-icon">▰</i> شدت اثر: ${faNumber(risk.severity)} از ۵</span><span>نسخهٔ قواعد: ${escapeHtml(risk.rulesVersion || state.assessment.rulesVersion)}</span></div>
    ${renderSourceMetadata(risk)}
    <div class="risk-action"><span class="action-label">اقدام پیشنهادی</span><p>${escapeHtml(risk.action)}</p><small>نشانه‌ها: ${escapeHtml(risk.signals)}</small></div>
    <div class="risk-card-footer"><span>امتیاز غربالگری است؛ آستانه‌ها و نگاشت پاسخ‌ها پیش‌فرض‌اند و باید با متخصص اعتبارسنجی شوند.</span><button class="text-button" data-go="actions" type="button">پیگیری اقدام ←</button></div>
  </article>`).join("") : `<div class="empty-state"><span class="empty-icon">⌕</span><strong>${allRisks.length ? "نتیجه‌ای با این فیلتر پیدا نشد" : "ریسکی برای نمایش ندارید"}</strong><p>${allRisks.length ? "فیلتر یا عبارت جست‌وجو را تغییر دهید." : "پرسشنامه یا فهرست مواد و تأمین‌کننده‌ها را تکمیل و ارزیابی را محاسبه کنید."}</p></div>`;
}

function renderActions() {
  const activeActions = state.actions.filter(action => action.active);
  const overdue = activeActions.filter(isOverdue).length;
  const done = activeActions.filter(action => action.status === "تکمیل‌شده").length;
  document.getElementById("actionSummary").innerHTML = `<div class="action-summary-card"><span>اقدام‌های فعال</span><strong>${faNumber(activeActions.length)}</strong></div><div class="action-summary-card"><span>تکمیل‌شده</span><strong>${faNumber(done)}</strong></div><div class="action-summary-card action-summary-overdue"><span>عقب‌افتاده</span><strong>${faNumber(overdue)}</strong></div>`;
  const root = document.getElementById("actionList");
  const rows = [...state.actions].sort((a, b) => Number(Boolean(b.active)) - Number(Boolean(a.active)) || (a.dueDate || "9999").localeCompare(b.dueDate || "9999"));
  root.innerHTML = rows.length ? `<table><thead><tr><th>ریسک / اقدام</th><th>مسئول و موعد</th><th>وضعیت</th><th>یادداشت و شواهد</th></tr></thead><tbody>${rows.map(action => {
    const risk = state.risks.find(item => item.id === action.riskId);
    const old = !action.active;
    return `<tr class="${old ? "historical-row" : ""}"><td><strong>${escapeHtml(risk?.title || "ریسک ارزیابی قبلی")}</strong><small>${escapeHtml(action.title)}</small>${old ? `<span class="historical-tag">از ارزیابی قبلی</span>` : risk?.criticalReview ? `<span class="critical-review-flag">بازبینی فوری انسانی</span>` : risk?.reviewStatus === "needs_review" ? `<span class="review-needed-flag">نیازمند بررسی</span>` : risk ? riskBadge(risk.level) : ""}</td><td><input class="table-input" data-action-field="owner" data-id="${escapeHtml(action.id)}" aria-label="مسئول اقدام" placeholder="نام مسئول" value="${escapeHtml(action.owner)}"><input class="table-input" data-action-field="dueDate" data-id="${escapeHtml(action.id)}" aria-label="موعد اقدام" type="date" value="${escapeHtml(action.dueDate)}"></td><td><select class="table-input" data-action-field="status" data-id="${escapeHtml(action.id)}" aria-label="وضعیت اقدام">${["باز", "در حال انجام", "تکمیل‌شده", "متوقف‌شده"].map(status => `<option ${action.status === status ? "selected" : ""}>${status}</option>`).join("")}</select>${isOverdue(action) ? `<span class="overdue-label">عقب‌افتاده</span>` : ""}</td><td><textarea class="table-input notes-input" data-action-field="notes" data-id="${escapeHtml(action.id)}" aria-label="یادداشت و شواهد" placeholder="توضیح، هزینه یا شاهد انجام">${escapeHtml(action.notes)}</textarea></td></tr>`;
  }).join("")}</tbody></table>` : `<div class="empty-state"><span class="empty-icon">✓</span><strong>هنوز اقدام پیشنهادی ندارید</strong><p>پس از محاسبهٔ ریسک‌ها، برای هر مورد اقدام قابل پیگیری ساخته می‌شود.</p></div>`;
}

function renderReport() {
  const root = document.getElementById("reportContent");
  const eventCutoff = new Date(); eventCutoff.setDate(eventCutoff.getDate() - 90);
  const recentOpenEvents = state.events.filter(item => !item.resolved && item.eventDate >= localDateISO(eventCutoff));
  const risks = [...state.risks].filter(risk => risk.active !== false).sort((a, b) => Number(Boolean(b.criticalReview)) - Number(Boolean(a.criticalReview)) || Number(b.reviewStatus === "needs_review") - Number(a.reviewStatus === "needs_review") || b.score - a.score);
  const activeActions = state.actions.filter(action => action.active);
  const high = risks.filter(risk => risk.level === "high").length;
  const medium = risks.filter(risk => risk.level === "medium").length;
  const low = risks.filter(risk => risk.level === "low").length;
  const resultsStale = state.assessment.status === "needs_recalculation";
  const critical = resultsStale ? null : risks.filter(risk => risk.criticalReview).length;
  root.innerHTML = `<div class="report-header"><div><span class="eyebrow">ریسک‌بان · گزارش غربالگری</span><h2>${escapeHtml(state.business.name || "کسب‌وکار ثبت‌نشده")}</h2><p>${escapeHtml(state.business.type)} · ${escapeHtml(state.business.city || "محل ثبت نشده")} · ${escapeHtml(state.business.products || "محصولات ثبت نشده")}</p></div><div class="report-date">تاریخ گزارش<strong>${escapeHtml(formatDateTime(new Date().toISOString()))}</strong><small>پرسشنامه ${escapeHtml(state.assessment.questionnaireVersion)} · قواعد ${escapeHtml(state.assessment.rulesVersion)}</small>${state.mode === "sample" ? `<span class="sample-watermark">دادهٔ نمونه</span>` : ""}</div></div>
    ${resultsStale ? `<div class="inline-alert alert-warning"><span>!</span> این گزارش شامل امتیازهای ذخیره‌شده با قواعد پیشین یا داده‌های تغییرکرده است. محاسبه را دوباره اجرا کنید تا امتیازها و هشدارهای بحرانی به‌روز شوند.</div>` : ""}
    <div class="report-stats"><div><span>ریسک فعال</span><strong>${faNumber(risks.length)}</strong></div><div><span>بالا</span><strong class="text-high">${faNumber(high)}</strong></div><div><span>متوسط</span><strong class="text-medium">${faNumber(medium)}</strong></div><div><span>پایین</span><strong class="text-low">${faNumber(low)}</strong></div><div><span>بازبینی فوری</span><strong class="text-high">${critical === null ? "—" : faNumber(critical)}</strong></div><div><span>مواد حیاتی</span><strong>${faNumber(state.materials.filter(item => item.criticality === "high").length)}</strong></div></div>
    <section class="report-section"><h3>ریسک‌های اولویت‌دار</h3>${risks.length ? `<div class="table-wrap"><table><thead><tr><th>ریسک</th><th>منبع ثبت</th><th>احتمال فعلی × شدت</th><th>امتیاز</th><th>اقدام پیشنهادی</th></tr></thead><tbody>${risks.map(risk => `<tr><td><strong>${escapeHtml(risk.title)}</strong>${risk.criticalReview ? `<span class="critical-review-flag">بازبینی فوری انسانی</span>` : risk.reviewStatus === "needs_review" ? `<span class="review-needed-flag">نیازمند بررسی</span>` : ""}<small>${escapeHtml(risk.category)}</small>${renderSourceMetadata(risk)}</td><td>${escapeHtml(risk.source === "questionnaire" ? "پرسشنامه" : risk.source === "supplier" ? "تأمین‌کننده" : risk.source === "event" ? "رخدادها" : "فهرست مواد")}</td><td>${faNumber(risk.likelihood)} × ${faNumber(risk.severity)}</td><td>${riskBadge(risk.level)} ${faNumber(risk.score)}/۲۵</td><td>${escapeHtml(risk.action)}</td></tr>`).join("")}</tbody></table></div>` : `<p class="muted">هنوز ریسکی محاسبه نشده است.</p>`}</section>
    <section class="report-section"><h3>مواد و وابستگی‌های مهم</h3>${state.materials.length ? `<div class="report-material-grid">${state.materials.map(material => `<div class="report-material"><strong>${escapeHtml(material.name)}</strong><span>${criticalityLabel(material.criticality)} · ${escapeHtml(supplierName(material.primarySupplierId))}</span><small>جایگزین: ${escapeHtml(supplierName(material.alternateSupplierId))}${material.alternateSupplierId ? material.alternateQualified ? " · تأییدشده" : " · نیازمند ارزیابی" : ""} · پوشش: ${material.stockCoverDays === "" ? "ثبت نشده" : `${faNumber(material.stockCoverDays)} روز`}</small></div>`).join("")}</div>` : `<p class="muted">فهرست مادهٔ اولیه ثبت نشده است.</p>`}</section>
    <section class="report-section"><h3>رخدادهای باز در ۹۰ روز گذشته</h3>${recentOpenEvents.length ? `<ul class="report-event-list">${recentOpenEvents.slice(0, 6).map(item => `<li><strong>${escapeHtml(formatDate(item.eventDate))} · ${escapeHtml(item.description)}</strong><small>${escapeHtml(state.materials.find(material => material.id === item.materialId)?.name || supplierName(item.supplierId))}</small></li>`).join("")}</ul>` : `<p class="muted">رخداد بازی برای نمایش وجود ندارد.</p>`}</section>
    <section class="report-section"><h3>پیگیری اقدام‌ها</h3>${activeActions.length ? `<div class="table-wrap"><table><thead><tr><th>اقدام</th><th>مسئول</th><th>موعد</th><th>وضعیت</th></tr></thead><tbody>${activeActions.map(action => `<tr><td>${escapeHtml(action.title)}</td><td>${escapeHtml(action.owner || "تعیین نشده")}</td><td>${escapeHtml(formatDate(action.dueDate))}</td><td>${escapeHtml(action.status)}</td></tr>`).join("")}</tbody></table></div>` : `<p class="muted">اقدامی برای پیگیری ثبت نشده است.</p>`}</section>
    <div class="report-method"><strong>روش و محدودیت</strong><p>${resultsStale ? "امتیازهای ذخیره‌شده پیش از آخرین تغییر و به‌روزرسانی قواعد هستند؛ فرمول فعلی تا اجرای محاسبهٔ دوباره روی این نتیجه‌ها اعمال نشده است." : `امتیاز هر ریسک برابر احتمال فعلی (۱ تا ۵) × شدت پیامد (۱ تا ۵) است؛ احتمال از نگاشت اختصاصی همان قاعده به پاسخ به دست می‌آید. «نامشخص» احتمال را نسبت به «خیر» کاهش نمی‌دهد. پایین: ۱ تا ۶، متوسط: ۷ تا ۱۴، بالا: ۱۵ تا ۲۵. ${faNumber(critical)} مورد نیازمند بازبینی فوری انسانی است؛ این هشدار مستقل از امتیاز عمل می‌کند.`} این مدل آماری نیست و نگاشت‌ها و آستانه‌ها باید با متخصص اعتبارسنجی شوند. امتیاز نمایش‌داده‌شده، ریسک فعلی/باقی‌مانده بر پایهٔ اطلاعات واردشده است؛ منبع ثبت‌شده راهنمای کنترل است و مقدار عددی احتمال را تأیید نمی‌کند.</p></div>`;
}

function renderSettings() {
  document.getElementById("schemaInfo").textContent = `ساختار دادهٔ ${SCHEMA_VERSION} · پرسشنامه ${state.assessment.questionnaireVersion} · قواعد ${state.assessment.rulesVersion} · آخرین ذخیره: ${formatDateTime(state.updatedAt)}`;
}

function fillSupplierSelects(selectedPrimary = "", selectedAlternate = "") {
  const options = state.suppliers.map(supplier => `<option value="${escapeHtml(supplier.id)}">${escapeHtml(supplier.name)}</option>`).join("");
  const primary = document.getElementById("materialPrimarySupplier");
  const alternate = document.getElementById("materialAlternateSupplier");
  primary.innerHTML = `<option value="">انتخاب نشده</option>${options}`;
  alternate.innerHTML = `<option value="">نداریم / ثبت نشده</option>${options}`;
  primary.value = selectedPrimary || "";
  alternate.value = selectedAlternate || "";
}

function openSupplierDialog(supplier = null) {
  document.getElementById("supplierForm").reset();
  document.getElementById("supplierId").value = supplier?.id || "";
  document.getElementById("supplierName").value = supplier?.name || "";
  document.getElementById("supplierCategory").value = supplier?.category || "مواد اولیه خشک";
  document.getElementById("supplierCity").value = supplier?.city || "";
  document.getElementById("supplierStatus").value = supplier?.status || "normal";
  document.getElementById("supplierDialogTitle").textContent = supplier ? "ویرایش تأمین‌کننده" : "افزودن تأمین‌کننده";
  document.getElementById("supplierDialog").showModal();
  document.getElementById("supplierName").focus();
}

function openMaterialDialog(material = null) {
  document.getElementById("materialForm").reset();
  fillSupplierSelects(material?.primarySupplierId || "", material?.alternateSupplierId || "");
  document.getElementById("materialId").value = material?.id || "";
  document.getElementById("materialName").value = material?.name || "";
  document.getElementById("materialCategory").value = material?.category || "آرد و غلات";
  document.getElementById("materialCriticality").value = material?.criticality || "high";
  document.getElementById("materialStorage").value = material?.storage || "خشک و محیطی";
  document.getElementById("materialLeadTime").value = material?.leadTimeDays ?? "";
  document.getElementById("materialStockCover").value = material?.stockCoverDays ?? "";
  document.getElementById("materialShelfLife").value = material?.shelfLifeDays ?? "";
  document.getElementById("materialAlternateQualified").value = material?.alternateQualified ? "yes" : "no";
  document.getElementById("materialDialogTitle").textContent = material ? "ویرایش مادهٔ اولیه" : "افزودن مادهٔ اولیه";
  document.getElementById("materialDialog").showModal();
  document.getElementById("materialName").focus();
}

function openEventDialog() {
  document.getElementById("eventForm").reset();
  document.getElementById("eventDate").value = todayISO();
  document.getElementById("eventMaterial").innerHTML = `<option value="">انتخاب نشده</option>${state.materials.map(material => `<option value="${escapeHtml(material.id)}">${escapeHtml(material.name)}</option>`).join("")}`;
  document.getElementById("eventSupplier").innerHTML = `<option value="">انتخاب نشده</option>${state.suppliers.map(supplier => `<option value="${escapeHtml(supplier.id)}">${escapeHtml(supplier.name)}</option>`).join("")}`;
  document.getElementById("eventDialog").showModal();
  document.getElementById("eventDescription").focus();
}

function saveEvent(event) {
  event.preventDefault();
  const eventItem = {
    id: makeId("event"), eventDate: document.getElementById("eventDate").value,
    eventType: document.getElementById("eventType").value,
    materialId: document.getElementById("eventMaterial").value,
    supplierId: document.getElementById("eventSupplier").value,
    severity: document.getElementById("eventSeverity").value,
    description: document.getElementById("eventDescription").value.trim(),
    resolved: false, createdAt: new Date().toISOString()
  };
  state.events.push(eventItem);
  if (state.mode === "empty") state.mode = "real";
  markAssessmentStale(); saveState(); document.getElementById("eventDialog").close();
  renderAll(); showToast("رخداد ثبت شد؛ در ارزیابی بعدی بررسی می‌شود.");
}

function toggleEvent(eventId) {
  const item = state.events.find(eventItem => eventItem.id === eventId);
  if (!item) return;
  item.resolved = !item.resolved;
  item.resolvedAt = item.resolved ? new Date().toISOString() : null;
  markAssessmentStale(); saveState(); renderAll();
  showToast(item.resolved ? "رخداد بسته شد؛ ارزیابی را دوباره محاسبه کنید." : "رخداد دوباره باز شد.");
}

function saveSupplier(event) {
  event.preventDefault();
  const id = document.getElementById("supplierId").value;
  const existing = state.suppliers.find(item => item.id === id);
  const supplier = {
    id: id || makeId("supplier"), name: document.getElementById("supplierName").value.trim(),
    category: document.getElementById("supplierCategory").value, city: document.getElementById("supplierCity").value.trim(),
    status: document.getElementById("supplierStatus").value, updatedAt: new Date().toISOString()
  };
  if (existing) Object.assign(existing, supplier); else state.suppliers.push(supplier);
  if (state.mode === "empty") state.mode = "real";
  markAssessmentStale(); saveState(); document.getElementById("supplierDialog").close();
  renderAll(); showToast("تأمین‌کننده ذخیره شد.");
}

function saveMaterial(event) {
  event.preventDefault();
  const id = document.getElementById("materialId").value;
  const existing = state.materials.find(item => item.id === id);
  const primarySupplierId = document.getElementById("materialPrimarySupplier").value;
  const alternateSupplierId = document.getElementById("materialAlternateSupplier").value;
  if (primarySupplierId && primarySupplierId === alternateSupplierId) {
    showToast("تأمین‌کنندهٔ اصلی و جایگزین باید متفاوت باشند."); return;
  }
  const material = {
    id: id || makeId("material"), name: document.getElementById("materialName").value.trim(),
    category: document.getElementById("materialCategory").value, criticality: document.getElementById("materialCriticality").value,
    storage: document.getElementById("materialStorage").value,
    primarySupplierId,
    alternateSupplierId,
    alternateQualified: Boolean(alternateSupplierId && document.getElementById("materialAlternateQualified").value === "yes"),
    leadTimeDays: document.getElementById("materialLeadTime").value,
    stockCoverDays: document.getElementById("materialStockCover").value,
    shelfLifeDays: document.getElementById("materialShelfLife").value,
    updatedAt: new Date().toISOString()
  };
  if (existing) Object.assign(existing, material); else state.materials.push(material);
  if (state.mode === "empty") state.mode = "real";
  markAssessmentStale(); saveState(); document.getElementById("materialDialog").close();
  renderAll(); showToast("مادهٔ اولیه ذخیره شد.");
}

function deleteMaterial(id) {
  const material = state.materials.find(item => item.id === id);
  if (!material || !window.confirm(`مادهٔ «${material.name}» از فهرست جاری حذف شود؟ تاریخچهٔ اقدام‌های قبلی حفظ می‌شود.`)) return;
  state.materials = state.materials.filter(item => item.id !== id);
  markAssessmentStale(); saveState(); renderAll(); showToast("ماده از فهرست جاری حذف شد.");
}
function deleteSupplier(id) {
  const supplier = state.suppliers.find(item => item.id === id);
  if (!supplier) return;
  if (state.materials.some(material => material.primarySupplierId === id || material.alternateSupplierId === id)) {
    showToast("این تأمین‌کننده به یک یا چند ماده وصل است؛ ابتدا ارتباط مواد را ویرایش کنید."); return;
  }
  if (!window.confirm(`تأمین‌کنندهٔ «${supplier.name}» حذف شود؟`)) return;
  state.suppliers = state.suppliers.filter(item => item.id !== id);
  markAssessmentStale(); saveState(); renderAll(); showToast("تأمین‌کننده حذف شد.");
}

function updateAction(actionId, field, value) {
  const action = state.actions.find(item => item.id === actionId);
  if (!action || !["owner", "dueDate", "status", "notes"].includes(field)) return;
  action[field] = value;
  if (field === "status" && value === "تکمیل‌شده") action.completedAt = new Date().toISOString();
  else if (field === "status") action.completedAt = null;
  saveState(); renderDashboard(); renderActions(); renderReport();
  showToast("برنامهٔ اقدام ذخیره شد.");
}

function sampleState() {
  const data = emptyState();
  data.mode = "sample";
  data.business = { name: "شیرینی‌سازی بهار", type: "تولید شیرینی و کیک", size: "۶ تا ۲۰ نفر", city: "تهران", products: "شیرینی خامه‌ای، کیک و شیرینی خشک", processes: "خرید، دریافت، نگهداری، آماده‌سازی، پخت، بسته‌بندی، ارسال" };
  data.suppliers = [
    { id: "s_flour", name: "آردینه شرق", category: "مواد اولیه خشک", city: "تهران", status: "normal" },
    { id: "s_dairy", name: "لبنیات سپید", category: "لبنیات و مواد سرد", city: "کرج", status: "watch" },
    { id: "s_egg", name: "تخم‌مرغ دانه‌طلایی", category: "تخم‌مرغ و پروتئین", city: "قزوین", status: "risk" },
    { id: "s_pack", name: "بسته‌بندی نقشینه", category: "بسته‌بندی", city: "تهران", status: "normal" },
    { id: "s_cocoa", name: "کاکائو و طعم‌آوران", category: "شکلات و مغزها", city: "تهران", status: "normal" },
    { id: "s_cocoa_alt", name: "شکلات‌سازان پارس", category: "شکلات و مغزها", city: "تهران", status: "normal" }
  ];
  data.materials = [
    { id: "m_flour", name: "آرد قنادی", category: "آرد و غلات", criticality: "high", storage: "خشک و محیطی", primarySupplierId: "s_flour", alternateSupplierId: "", leadTimeDays: "5", stockCoverDays: "8", shelfLifeDays: "180" },
    { id: "m_cream", name: "خامهٔ قنادی", category: "لبنیات و مواد سرد", criticality: "high", storage: "یخچالی", primarySupplierId: "s_dairy", alternateSupplierId: "", leadTimeDays: "2", stockCoverDays: "1", shelfLifeDays: "7" },
    { id: "m_egg", name: "تخم‌مرغ", category: "تخم‌مرغ", criticality: "high", storage: "یخچالی", primarySupplierId: "s_egg", alternateSupplierId: "", leadTimeDays: "3", stockCoverDays: "4", shelfLifeDays: "21" },
    { id: "m_cocoa", name: "شکلات و کاکائو", category: "شکلات و کاکائو", criticality: "medium", storage: "خشک و محیطی", primarySupplierId: "s_cocoa", alternateSupplierId: "s_cocoa_alt", alternateQualified: true, leadTimeDays: "7", stockCoverDays: "10", shelfLifeDays: "240" },
    { id: "m_box", name: "جعبهٔ بسته‌بندی", category: "بسته‌بندی", criticality: "medium", storage: "خشک و محیطی", primarySupplierId: "s_pack", alternateSupplierId: "", leadTimeDays: "10", stockCoverDays: "12", shelfLifeDays: "" }
  ];
  data.events = [
    { id: "e_cream_temp", eventDate: todayISO(), eventType: "temperature", materialId: "m_cream", supplierId: "s_dairy", severity: "high", description: "یک محموله با تأخیر در انتقال به یخچال دریافت شد؛ بررسی دما ثبت نشده بود.", resolved: false, createdAt: new Date().toISOString() },
    { id: "e_egg_delay", eventDate: todayISO(), eventType: "delay", materialId: "m_egg", supplierId: "s_egg", severity: "medium", description: "تحویل تخم‌مرغ یک روز دیرتر از زمان معمول رسید.", resolved: false, createdAt: new Date().toISOString() }
  ];
  data.assessment.answers = { supplierPerformance: "partial", supplierQuality: "partial", traceability: "no", inventory: "partial", substitution: "no", allergens: "partial", coldChain: "partial", transport: "yes", priceAvailability: "partial", continuity: "unknown" };
  return data;
}

function loadSample(confirmReplace = true) {
  if (confirmReplace && hasAnyData() && !window.confirm("دادهٔ فعلی با سناریوی نمونه جایگزین می‌شود. ادامه می‌دهید؟")) return;
  state = sampleState();
  document.getElementById("businessForm").dataset.loaded = "false";
  calculateRisks({ silent: true });
  state.mode = "sample";
  saveState(); showPage("dashboard"); showToast("سناریوی نمونه بارگذاری شد؛ داده‌ها نمایشی هستند.");
}

function startRealData() {
  if (hasAnyData() && !window.confirm("شروع ارزیابی واقعی، داده‌های فعلی این مرورگر را پاک می‌کند. ابتدا در صورت نیاز خروجی بگیرید.")) return;
  state = emptyState(); state.mode = "real"; saveState();
  document.getElementById("businessForm").dataset.loaded = "false";
  showPage("business"); showToast("اطلاعات کسب‌وکار خود را وارد کنید.");
}

function clearAllData() {
  if (!window.confirm("همهٔ اطلاعات ذخیره‌شده در این مرورگر پاک شود؟ این کار قابل بازگشت نیست.")) return;
  state = emptyState(); saveState();
  document.getElementById("businessForm").dataset.loaded = "false";
  document.getElementById("questionnaire").dataset.rendered = "false";
  showPage("dashboard"); showToast("داده‌ها پاک شد.");
}

function downloadFile(filename, content, type) {
  const blob = new Blob(["\ufeff", content], { type });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a"); link.href = url; link.download = filename;
  document.body.appendChild(link); link.click(); link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000);
}
function exportJSON() {
  const envelope = { application: "Riskban MVP", schema_version: SCHEMA_VERSION, exported_at: new Date().toISOString(), data: state };
  downloadFile("riskban-supply-chain-backup.json", JSON.stringify(envelope, null, 2), "application/json;charset=utf-8");
}
function csvCell(value) {
  let text = String(value ?? "");
  if (/^[\t\r ]*[=+\-@]/.test(text)) text = `'${text}`;
  return `"${text.replace(/"/g, '""')}"`;
}
function exportCSV() {
  const rows = [["ریسک", "اقدام", "مسئول", "موعد", "وضعیت", "یادداشت"]];
  state.actions.forEach(action => {
    const risk = state.risks.find(item => item.id === action.riskId);
    rows.push([risk?.title || "ریسک ارزیابی قبلی", action.title, action.owner, action.dueDate, action.status, action.notes]);
  });
  downloadFile("riskban-actions.csv", rows.map(row => row.map(csvCell).join(",")).join("\r\n"), "text/csv;charset=utf-8");
}

function isValidImportData(data) {
  const validObject = value => value == null || (typeof value === "object" && !Array.isArray(value));
  return Boolean(
    data && typeof data === "object" && !Array.isArray(data) &&
    data.business && typeof data.business === "object" && !Array.isArray(data.business) &&
    ["suppliers", "materials", "events", "risks", "actions"].every(key => Array.isArray(data[key])) &&
    validObject(data.assessment) && validObject(data.assessment?.answers) && validObject(data.answers)
  );
}

async function importJSON(file) {
  try {
    const envelope = JSON.parse(await file.text());
    const imported = envelope?.data;
    const incomingVersion = envelope?.schema_version;
    if (![2, SCHEMA_VERSION].includes(incomingVersion) || imported?.schemaVersion !== incomingVersion || !isValidImportData(imported)) {
      showToast("نسخهٔ فایل پشتیبان با این برنامه سازگار نیست."); return;
    }
    if (!window.confirm("اطلاعات فعلی با داده‌های فایل پشتیبان جایگزین شود؟")) return;
    state = normalizeState(incomingVersion === 2 ? migrateV2State(imported) : imported); saveState();
    document.getElementById("businessForm").dataset.loaded = "false";
    document.getElementById("questionnaire").dataset.rendered = "false";
    renderAll(); showPage("dashboard"); showToast("فایل پشتیبان بازیابی شد.");
  } catch (error) {
    showToast("فایل JSON خوانده نشد یا ساختار معتبری ندارد.");
  }
}

document.querySelectorAll(".nav-link[data-page]").forEach(button => button.addEventListener("click", () => showPage(button.dataset.page)));
document.addEventListener("click", event => {
  const goButton = event.target.closest("[data-go]");
  if (goButton) showPage(goButton.dataset.go);
  if (event.target.closest("#exportHeader, #exportSettings, #exportReport")) exportJSON();
  if (event.target.closest("#loadSample, #loadSampleSettings")) loadSample(true);
  if (event.target.closest("#startReal")) startRealData();
  if (event.target.closest("#switchToReal")) startRealData();
  if (event.target.closest("#addSupplier, [data-add-supplier]")) openSupplierDialog();
  if (event.target.closest("#addMaterial, [data-add-material]")) openMaterialDialog();
  if (event.target.closest("#addEvent, [data-add-event]")) openEventDialog();
  if (event.target.closest("#calculateRisks, .banner-recalc")) calculateRisks();
  if (event.target.closest("#printReport")) window.print();
  if (event.target.closest("#clearData")) clearAllData();
  if (event.target.closest("#importSettings")) document.getElementById("importFile").click();
  const close = event.target.closest("[data-close]");
  if (close) document.getElementById(close.dataset.close).close();
  const editMaterialButton = event.target.closest("[data-edit-material]");
  if (editMaterialButton) openMaterialDialog(state.materials.find(item => item.id === editMaterialButton.dataset.editMaterial));
  const editSupplierButton = event.target.closest("[data-edit-supplier]");
  if (editSupplierButton) openSupplierDialog(state.suppliers.find(item => item.id === editSupplierButton.dataset.editSupplier));
  const deleteMaterialButton = event.target.closest("[data-delete-material]");
  if (deleteMaterialButton) deleteMaterial(deleteMaterialButton.dataset.deleteMaterial);
  const deleteSupplierButton = event.target.closest("[data-delete-supplier]");
  if (deleteSupplierButton) deleteSupplier(deleteSupplierButton.dataset.deleteSupplier);
  const toggleEventButton = event.target.closest("[data-toggle-event]");
  if (toggleEventButton) toggleEvent(toggleEventButton.dataset.toggleEvent);
  const closeBanner = event.target.closest(".banner-close");
  if (closeBanner) { state.migrationNotice = false; saveState(); renderHeader(); }
});

document.getElementById("businessForm").addEventListener("submit", event => {
  event.preventDefault();
  const form = event.currentTarget;
  state.business = { name: form.elements.name.value.trim(), type: form.elements.type.value, size: form.elements.size.value, city: form.elements.city.value.trim(), products: form.elements.products.value.trim(), processes: form.elements.processes.value.trim() };
  if (state.mode === "empty") state.mode = "real";
  form.dataset.loaded = "true";
  saveState(); renderAll(); showToast("مشخصات کسب‌وکار ذخیره شد.");
});
document.getElementById("supplierForm").addEventListener("submit", saveSupplier);
document.getElementById("materialForm").addEventListener("submit", saveMaterial);
document.getElementById("eventForm").addEventListener("submit", saveEvent);
document.getElementById("questionnaire").addEventListener("change", event => {
  const input = event.target;
  if (input.type !== "radio") return;
  state.assessment.answers[input.name] = input.value;
  markAssessmentStale(); saveState();
  const answered = Object.keys(state.assessment.answers).filter(id => state.assessment.answers[id]).length;
  const progress = Math.round(answered / QUESTIONS.length * 100);
  document.getElementById("assessmentProgressText").textContent = `${faNumber(answered)} از ${faNumber(QUESTIONS.length)} پاسخ`;
  document.getElementById("assessmentProgressBar").style.width = `${progress}%`;
  updateAnswerStates(); renderHeader();
});
document.getElementById("actionList").addEventListener("change", event => {
  const field = event.target.dataset.actionField;
  if (field) updateAction(event.target.dataset.id, field, event.target.value);
});
document.getElementById("riskSearch").addEventListener("input", renderRisks);
document.getElementById("riskLevelFilter").addEventListener("change", renderRisks);
document.getElementById("importFile").addEventListener("change", event => {
  const file = event.target.files?.[0];
  if (file) importJSON(file);
  event.target.value = "";
});

document.querySelectorAll("dialog").forEach(dialog => dialog.addEventListener("click", event => {
  if (event.target === dialog) dialog.close();
}));

renderAll();
