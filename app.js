const STORAGE_KEY = "riskban_supply_chain_mvp";
const QUESTIONNAIRE_VERSION = "1.0";
const RULES_VERSION = "1.0";
const ACTION_STATUSES = ["باز", "در حال انجام", "تکمیل‌شده", "متوقف‌شده"];
const RISK_LEVELS = [
  { key: "high", label: "بالا", color: "#c45443" },
  { key: "medium", label: "متوسط", color: "#d58a32" },
  { key: "low", label: "پایین", color: "#37836f" }
];
const QUESTIONS = [
  {
    id: "supplier", cat: "تاب‌آوری تأمین", title: "منبع جایگزین برای اقلام حیاتی",
    question: "برای مواد اولیه و اقلام حیاتی، دست‌کم یک منبع جایگزین ارزیابی‌شده دارید؟",
    risk: "وابستگی به منبع یگانه", impact: "توقف یا کاهش تولید، خرید اضطراری و افزایش هزینه",
    signs: "یک تأمین‌کننده برای قلم حیاتی، نبود تأییدیه منبع دوم یا زمان طولانی جایگزینی",
    action: "اقلام تک‌منبع را فهرست کنید؛ منبع جایگزین را ارزیابی کنید و حداقل موجودی و زمان جایگزینی را ثبت کنید.",
    likelihood: 4, severity: 5
  },
  {
    id: "food", cat: "کیفیت و نگهداری", title: "شرایط حمل و نگهداری مواد حساس",
    question: "شرایط حمل و نگهداری مواد حساس، از جمله دما در صورت نیاز، هنگام دریافت و انبارش ثبت می‌شود؟",
    risk: "آسیب به مواد حساس در حمل یا انبارش", impact: "افت کیفیت، ضایعات، برگشت محصول یا آسیب به مصرف‌کننده",
    signs: "نبود ثبت شرایط دریافت، نوسان دما، بسته‌بندی آسیب‌دیده یا زمان حمل نامعلوم",
    action: "معیار دریافت و نگهداری هر قلم حساس را تعیین کنید؛ موارد خارج از معیار را ثبت، قرنطینه و پیگیری کنید.",
    likelihood: 3, severity: 5
  },
  {
    id: "hygiene", cat: "ردیابی و کیفیت", title: "ردیابی محموله و بچ تولید",
    question: "می‌توانید هر محموله ماده اولیه را به تأمین‌کننده و بچ محصول مصرف‌کننده آن پیوند دهید؟",
    risk: "ردیابی ناقص مواد و محموله‌ها", impact: "کندی رسیدگی به شکایت، محدودشدن فراخوان و دشواری یافتن علت",
    signs: "رسیدهای بدون شماره بچ، ثبت دستی پراکنده یا نامشخص‌بودن محل مصرف مواد",
    action: "شناسه محموله و تأمین‌کننده را هنگام دریافت ثبت کنید و پیوند آن با بچ تولید را نگه دارید.",
    likelihood: 3, severity: 4
  },
  {
    id: "quality", cat: "کیفیت تأمین", title: "کنترل کیفیت هنگام دریافت",
    question: "برای اقلام اصلی، معیار پذیرش، ثبت مغایرت و پیگیری اقدام اصلاحی دارید؟",
    risk: "ورود مواد نامنطبق به فرایند تولید", impact: "دوباره‌کاری، ضایعات، توقف تولید یا نارضایتی مشتری",
    signs: "پذیرش بدون بررسی، تکرار مغایرت یا نبود سابقه برگشت و اقدام اصلاحی",
    action: "معیار پذیرش اقلام کلیدی را مشخص کنید؛ نتایج دریافت و مغایرت‌ها را برای هر تأمین‌کننده ثبت کنید.",
    likelihood: 3, severity: 4
  },
  {
    id: "equipment", cat: "استمرارية تأمین", title: "آمادگی تأمین‌کننده برای اختلال",
    question: "برای تأمین‌کنندگان حیاتی، نشانه‌های اختلال و برنامه جایگزین یا بازیابی را می‌شناسید؟",
    risk: "اختلال عملیاتی تأمین‌کننده حیاتی", impact: "تأخیر در تأمین، توقف تولید و افزایش خرید اضطراری",
    signs: "تأخیرهای تکراری، ظرفیت محدود، نبود تماس جایگزین یا بی‌اطلاعی از برنامه تداوم",
    action: "تأمین‌کنندگان حیاتی را رتبه‌بندی کنید؛ زمان بازیابی، تماس جایگزین و مسیر تأمین اضطراری را ثبت کنید.",
    likelihood: 3, severity: 5
  },
  {
    id: "people", cat: "دیدپذیری زنجیره", title: "پایش عملکرد تأمین‌کنندگان",
    question: "تحویل به‌موقع، کیفیت و مغایرت تأمین‌کنندگان کلیدی با شاخص‌های ساده بازبینی می‌شود؟",
    risk: "دیر دیده‌شدن افت عملکرد تأمین‌کننده", impact: "کمبود ناگهانی، هزینه بالاتر و واکنش دیرهنگام",
    signs: "نبود سابقه تأخیر، تکرار مغایرت بدون روند یا اتکا به پیگیری شفاهی",
    action: "برای تأمین‌کنندگان کلیدی، تأخیر، کیفیت و پاسخ‌گویی را دوره‌ای ثبت و موارد نامطلوب را پیگیری کنید.",
    likelihood: 3, severity: 3
  },
  {
    id: "demand", cat: "موجودی و برنامه‌ریزی", title: "پوشش موجودی اقلام حیاتی",
    question: "حداقل موجودی و نقطه سفارش اقلام حیاتی با زمان تأمین و نوسان مصرف هماهنگ است؟",
    risk: "کمبود یا مازاد موجودی مواد حیاتی", impact: "توقف تولید، خرید اضطراری، خواب سرمایه یا فساد مواد",
    signs: "کمبودهای تکراری، سفارش دیرهنگام، موجودی بدون گردش یا نبود حداقل موجودی",
    action: "مصرف و زمان تأمین اقلام حیاتی را ثبت کنید؛ حداقل موجودی و نقطه سفارش را متناسب با نوسان بازبینی کنید.",
    likelihood: 4, severity: 4
  },
  {
    id: "delivery", cat: "حمل‌ونقل و لجستیک", title: "تداوم حمل و تحویل",
    question: "برای مسیر، حمل‌کننده یا روش تحویل جایگزین در زمان اختلال برنامه دارید؟",
    risk: "اختلال در حمل و تحویل مواد", impact: "تأخیر دریافت، افزایش هزینه حمل یا افت کیفیت محموله",
    signs: "وابستگی به یک حمل‌کننده، تأخیر پرتکرار یا نداشتن مسیر جایگزین",
    action: "مسیرها و حمل‌کنندگان مهم را مشخص کنید؛ گزینه جایگزین و روش اطلاع از تأخیر را از پیش ثبت کنید.",
    likelihood: 3, severity: 4
  },
  {
    id: "compliance", cat: "قیمت و الزامات تأمین", title: "نوسان قیمت و مدارک تأمین",
    question: "تغییرات مهم قیمت و مدارک لازم برای اقلام و تأمین‌کنندگان کلیدی به‌صورت منظم بازبینی می‌شود؟",
    risk: "غافلگیری ناشی از تغییر قیمت یا مدارک تأمین", impact: "افزایش هزینه، تأخیر خرید یا ریسک پذیرش مواد",
    signs: "جهش قیمت بدون ثبت، مدارک منقضی یا نامشخص‌بودن مسئول پیگیری",
    action: "تغییرات قیمت و مدارک تأمین را ثبت کنید؛ مسئول و موعد بازبینی هر قلم و تأمین‌کننده مهم را تعیین کنید.",
    likelihood: 3, severity: 4
  }
];

const DEFAULT_BUSINESS = {
  name: "", type: "تولید کیک و شیرینی", size: "۱ تا ۵ نفر", city: "",
  processes: "خرید، دریافت، انبار، تولید، بسته‌بندی، ارسال",
  criticalInputs: "", coverageDays: "", importShare: ""
};
let state = null;
let editingSupplierId = null;
let editingActionId = null;
let toastTimeout = null;
let storageUnavailable = false;

function uid(prefix) {
  const random = window.crypto && window.crypto.randomUUID
    ? window.crypto.randomUUID()
    : Date.now().toString(36) + Math.random().toString(36).slice(2);
  return prefix + "_" + random;
}
function freshState() {
  return {
    schema_version: "1.0", business: Object.assign({}, DEFAULT_BUSINESS),
    suppliers: [], answers: {}, risks: [], actions: [], sample: false,
    assessed: false, assessmentStatus: "draft", calculatedAt: null
  };
}
function localISODate() {
  const now = new Date();
  return new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}
function demoState() {
  const demo = {
    schema_version: "1.0",
    business: {
      name: "کارگاه نمونه سپیدار", type: "تولید کیک و شیرینی", size: "۶ تا ۲۰ نفر",
      city: "تهران", processes: "خرید، دریافت، انبار، تولید، بسته‌بندی، ارسال",
      criticalInputs: "آرد، روغن، تخم‌مرغ، خامه و بسته‌بندی", coverageDays: "۱۰", importShare: "۱۵"
    },
    suppliers: [
      { id: "demo_s1", name: "آسیاب نمونه البرز", category: "آرد", criticality: "critical", status: "stable", leadTime: "3", region: "البرز", singleSource: false, notes: "داده نمایشی؛ سابقه تحویل پایدار" },
      { id: "demo_s2", name: "روغن‌سازان نمونه", category: "روغن و چربی", criticality: "critical", status: "watch", leadTime: "8", region: "قزوین", singleSource: true, notes: "داده نمایشی؛ بررسی منبع جایگزین" },
      { id: "demo_s3", name: "تخم‌مرغ دشت نمونه", category: "تخم‌مرغ", criticality: "high", status: "stable", leadTime: "2", region: "شهریار", singleSource: false, notes: "داده نمایشی" },
      { id: "demo_s4", name: "پخش سرد نمونه", category: "خامه و لبنیات", criticality: "critical", status: "disrupted", leadTime: "5", region: "تهران", singleSource: true, notes: "داده نمایشی؛ تأخیر اخیر" },
      { id: "demo_s5", name: "بسته‌بندی نمونه آریا", category: "جعبه و لفاف", criticality: "normal", status: "watch", leadTime: "12", region: "قم", singleSource: false, notes: "داده نمایشی" },
      { id: "demo_s6", name: "باربری نمونه", category: "حمل‌ونقل", criticality: "high", status: "stable", leadTime: "1", region: "تهران", singleSource: false, notes: "داده نمایشی" }
    ],
    answers: {
      supplier: "no", food: "partial", hygiene: "partial", quality: "partial",
      equipment: "unknown", people: "partial", demand: "no", delivery: "partial", compliance: "unknown"
    },
    actions: [], risks: [], sample: true, assessed: true,
    assessmentStatus: "calculated", calculatedAt: new Date().toISOString()
  };
  demo.risks = calculateRiskResults(demo.answers);
  demo.risks.forEach(function (risk, index) {
    demo.actions.push({
      id: "demo_a" + index, riskId: risk.id, title: risk.action, owner: index === 0 ? "مسئول خرید" : "",
      due: index === 0 ? localISODate() : "", status: index === 0 ? "در حال انجام" : "باز",
      notes: "", createdAt: demo.calculatedAt, updatedAt: demo.calculatedAt
    });
  });
  return demo;
}
function migrateState(saved) {
  const base = freshState();
  const merged = Object.assign(base, saved || {});
  merged.business = Object.assign({}, DEFAULT_BUSINESS, saved && saved.business || {});
  merged.suppliers = Array.isArray(saved && saved.suppliers) ? saved.suppliers : [];
  merged.answers = saved && typeof saved.answers === "object" && saved.answers ? saved.answers : {};
  merged.risks = Array.isArray(saved && saved.risks) ? saved.risks : [];
  merged.actions = Array.isArray(saved && saved.actions) ? saved.actions : [];
  merged.schema_version = "1.0";
  if (typeof merged.sample !== "boolean") {
    merged.sample = merged.business.name === "شیرینی‌سرای نمونه" || merged.business.name === "کارگاه نمونه سپیدار";
  }
  merged.assessed = Boolean(merged.assessed || merged.calculatedAt);
  if (!merged.assessmentStatus) {
    merged.assessmentStatus = merged.assessed ? "calculated" :
      (Object.keys(merged.answers).some(function (key) { return Boolean(merged.answers[key]); }) ? "draft" : "draft");
  }
  return merged;
}
function loadState() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY) || localStorage.getItem("riskban_mvp");
    if (raw) return migrateState(JSON.parse(raw));
    const demo = demoState();
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(demo)); }
    catch (error) { storageUnavailable = true; }
    return demo;
  } catch (error) {
    storageUnavailable = true;
    return demoState();
  }
}
function saveState() {
  state.schema_version = "1.0";
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    storageUnavailable = false;
  } catch (error) {
    storageUnavailable = true;
  }
  updatePersistentNotices();
}
function showToast(message) {
  const toast = document.getElementById("appToast");
  toast.textContent = message;
  toast.classList.add("is-visible");
  clearTimeout(toastTimeout);
  toastTimeout = setTimeout(function () { toast.classList.remove("is-visible"); }, 3200);
}
function esc(value) {
  return String(value == null ? "" : value).replace(/[&<>"']/g, function (char) {
    return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[char];
  });
}
function faNum(value) { return Number(value || 0).toLocaleString("fa-IR"); }
function formatDate(date) {
  if (!date) return "تعیین نشده";
  const parsed = new Date(date.length === 10 ? date + "T00:00:00" : date);
  return Number.isNaN(parsed.getTime()) ? "تاریخ نامعتبر" : parsed.toLocaleDateString("fa-IR");
}
function updatePersistentNotices() {
  const warning = document.getElementById("storageWarning");
  if (warning) warning.classList.toggle("hidden", !storageUnavailable);
}
function markAssessmentStale() {
  if (state.assessed) state.assessmentStatus = "needs_recalculation";
  else state.assessmentStatus = "draft";
  saveState();
  renderDashboard();
  renderAssessment();
}
function showPage(page) {
  document.querySelectorAll("main section[id^='page-']").forEach(function (section) { section.classList.add("hidden"); });
  const target = document.getElementById("page-" + page);
  if (!target) return;
  target.classList.remove("hidden");
  document.querySelectorAll(".app-nav button").forEach(function (button) {
    const active = button.dataset.page === page;
    button.classList.toggle("active", active);
    if (active) button.setAttribute("aria-current", "page");
    else button.removeAttribute("aria-current");
  });
  const titles = {
    dashboard: ["نمای روز", "نمای مدیریتی", "وضعیت تأمین‌کنندگان، ریسک‌ها و اقدام‌های باز"],
    suppliers: ["پایگاه تأمین", "تأمین‌کنندگان", "وابستگی‌ها، زمان تأمین و نشانه‌های اختلال"],
    business: ["پروفایل", "مشخصات کسب‌وکار", "اطلاعات زمینه‌ای برای ارزیابی زنجیره تأمین"],
    assessment: ["پایش زنجیره", "ارزیابی زنجیره", "پرسش‌های غربالگری برای شناسایی شکاف‌های کنترلی"],
    risks: ["دفتر ریسک", "دفتر ریسک", "ریسک‌های شناسایی‌شده و دلیل اولویت آن‌ها"],
    actions: ["پیگیری اجرا", "برنامه اقدام", "مسئولیت‌ها، موعدها و وضعیت اجرا"],
    report: ["خروجی مدیریتی", "گزارش مدیریتی", "گزارش قابل چاپ از وضعیت فعلی"],
    settings: ["مدیریت داده", "داده و پشتیبان", "ذخیره‌سازی محلی، خروجی و محدودیت‌های نسخه"]
  };
  document.getElementById("pageEyebrow").textContent = titles[page][0];
  document.getElementById("pageTitle").textContent = titles[page][1];
  document.getElementById("pageSubtitle").textContent = titles[page][2];
  if (page === "business") fillBusiness();
  if (page === "suppliers") renderSuppliers();
  if (page === "assessment") renderAssessment();
  if (page === "risks") renderRisks();
  if (page === "actions") renderActions();
  if (page === "report") renderReport();
  if (page === "settings") renderSettings();
  renderDashboard();
  window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth" });
}
document.querySelectorAll(".app-nav button").forEach(function (button) {
  button.addEventListener("click", function () { showPage(button.dataset.page); });
});

function fillBusiness() {
  const business = state.business;
  [["bName", "name"], ["bType", "type"], ["bSize", "size"], ["bCity", "city"],
    ["bProcesses", "processes"], ["bCriticalInputs", "criticalInputs"],
    ["bCoverageDays", "coverageDays"], ["bImportShare", "importShare"]].forEach(function (pair) {
    document.getElementById(pair[0]).value = business[pair[1]] == null ? "" : business[pair[1]];
  });
}
document.getElementById("businessForm").addEventListener("submit", function (event) {
  event.preventDefault();
  const name = document.getElementById("bName").value.trim();
  if (!name) { showToast("نام کسب‌وکار را وارد کنید."); document.getElementById("bName").focus(); return; }
  [["bName", "name"], ["bType", "type"], ["bSize", "size"], ["bCity", "city"],
    ["bProcesses", "processes"], ["bCriticalInputs", "criticalInputs"],
    ["bCoverageDays", "coverageDays"], ["bImportShare", "importShare"]].forEach(function (pair) {
    state.business[pair[1]] = document.getElementById(pair[0]).value.trim();
  });
  saveState();
  renderDashboard();
  showToast("مشخصات کسب‌وکار ذخیره شد.");
});

function renderAssessment() {
  const answered = QUESTIONS.filter(function (question) { return Boolean(state.answers[question.id]); }).length;
  const progress = Math.round(answered / QUESTIONS.length * 100);
  document.getElementById("questionCount").textContent = faNum(answered) + " از " + faNum(QUESTIONS.length);
  document.getElementById("assessmentProgress").style.width = progress + "%";
  document.getElementById("assessmentPercent").textContent = faNum(progress) + "٪";
  document.getElementById("assessmentStatusText").textContent =
    state.assessmentStatus === "needs_recalculation" ? "پاسخ‌ها تغییر کرده‌اند؛ نتیجه قبلی نیازمند محاسبه مجدد است." :
      (state.assessed ? "نتیجه آخرین محاسبه: " + formatDate(state.calculatedAt) : "پاسخ‌ها به‌صورت خودکار ذخیره می‌شوند.");
  document.getElementById("questionnaire").innerHTML = QUESTIONS.map(function (question, index) {
    const options = [
      ["yes", "بله", "کنترل برقرار و قابل تأیید است"],
      ["partial", "تا حدی", "کنترل ناقص یا ناپایدار است"],
      ["no", "خیر", "کنترل وجود ندارد یا اجرا نمی‌شود"],
      ["unknown", "نامشخص", "اطلاعات کافی برای قضاوت نداریم"]
    ];
    return '<article class="question-card"><div class="question-head"><span class="question-index">' + faNum(index + 1) +
      '</span><span class="question-category">' + esc(question.cat) + '</span></div><h3>' + esc(question.title) +
      '</h3><p class="question-copy">' + esc(question.question) + '</p><div class="answer-options">' +
      options.map(function (option) {
        const inputId = question.id + "-" + option[0];
        return '<label class="answer-option' + (state.answers[question.id] === option[0] ? " selected" : "") +
          '" for="' + inputId + '"><input id="' + inputId + '" type="radio" name="' + question.id + '" value="' + option[0] +
          '"' + (state.answers[question.id] === option[0] ? " checked" : "") + ' onchange="setAnswer(\\'' + question.id +
          '\\',\\'' + option[0] + '\\')"><span class="answer-radio"></span><span class="answer-label"><b>' + option[1] +
          '</b><small>' + option[2] + '</small></span></label>';
      }).join("") + '</div></article>';
  }).join("");
}
function setAnswer(id, value) {
  state.answers[id] = value;
  markAssessmentStale();
}
function answerFactor(answer) {
  return answer === "partial" ? 0.65 : answer === "unknown" ? 0.85 : 1;
}
function answerText(answer) {
  return ({ yes: "بله", partial: "تا حدی", no: "خیر", unknown: "نامشخص" })[answer] || "ثبت نشده";
}
function riskLevel(score) { return score >= 15 ? "high" : score >= 7 ? "medium" : "low"; }
function calculateRiskResults(answers) {
  return QUESTIONS.filter(function (question) {
    return answers[question.id] && answers[question.id] !== "yes";
  }).map(function (question) {
    const answer = answers[question.id];
    const score = Math.max(1, Math.round(question.likelihood * question.severity * answerFactor(answer)));
    return {
      id: question.id, cat: question.cat, title: question.risk, question: question.question,
      impact: question.impact, signs: question.signs, action: question.action,
      likelihood: question.likelihood, severity: question.severity, score: score,
      level: riskLevel(score), answer: answer, answerLabel: answerText(answer),
      reviewStatus: answer === "unknown" ? "نیازمند بررسی" : "غربالگری اولیه"
    };
  }).sort(function (a, b) { return b.score - a.score; });
}
function calculateRisks() {
  const answered = QUESTIONS.filter(function (question) { return Boolean(state.answers[question.id]); });
  if (!answered.length) {
    showToast("برای محاسبه، ابتدا دست‌کم به یک سؤال پاسخ دهید.");
    document.getElementById("supplier-yes").focus();
    return;
  }
  const priorIds = state.risks.map(function (risk) { return risk.id; }).sort().join(",");
  const nextRisks = calculateRiskResults(state.answers);
  const nextIds = nextRisks.map(function (risk) { return risk.id; }).sort().join(",");
  state.risks = nextRisks;
  nextRisks.forEach(function (risk) {
    const existing = state.actions.find(function (action) { return action.riskId === risk.id && action.status !== "تکمیل‌شده"; });
    if (!existing) {
      state.actions.push({
        id: uid("action"), riskId: risk.id, title: risk.action, owner: "", due: "",
        status: "باز", notes: "", createdAt: new Date().toISOString(), updatedAt: new Date().toISOString()
      });
    }
  });
  state.assessed = true;
  state.assessmentStatus = "calculated";
  state.calculatedAt = new Date().toISOString();
  saveState();
  renderAll();
  showPage("risks");
  const historyNote = priorIds !== nextIds && state.actions.some(function (action) {
    return !nextRisks.some(function (risk) { return risk.id === action.riskId; });
  }) ? " سوابق اقدام‌های قبلی حفظ شدند." : "";
  showToast(faNum(answered.length) + " پاسخ محاسبه شد." + historyNote);
}
function riskTone(level) { return ({ high: "tone-high", medium: "tone-medium", low: "tone-low" })[level] || ""; }
function riskBadge(level) {
  const label = ({ high: "بالا", medium: "متوسط", low: "پایین" })[level] || "نامشخص";
  return '<span class="risk-badge ' + riskTone(level) + '"><i></i> ریسک ' + label + '</span>';
}
function findQuestion(id) {
  return QUESTIONS.find(function (question) { return question.id === id; });
}
function activeRiskIds() {
  return new Set(state.risks.map(function (risk) { return risk.id; }));
}
function getActionForRisk(riskId) {
  return state.actions.find(function (action) { return action.riskId === riskId && action.status !== "تکمیل‌شده"; }) ||
    state.actions.find(function (action) { return action.riskId === riskId; });
}
function renderRiskFilters() {
  const select = document.getElementById("riskCategoryFilter");
  const selected = select.value || "all";
  const categories = Array.from(new Set(QUESTIONS.map(function (question) { return question.cat; })));
  select.innerHTML = '<option value="all">همه دسته‌ها</option>' + categories.map(function (category) {
    return '<option value="' + esc(category) + '">' + esc(category) + '</option>';
  }).join("");
  if (categories.indexOf(selected) >= 0) select.value = selected;
}
function renderRisks() {
  renderRiskFilters();
  const filter = document.getElementById("riskFilter").value;
  const category = document.getElementById("riskCategoryFilter").value;
  const query = document.getElementById("riskSearch").value.trim().toLocaleLowerCase("fa");
  const risks = state.risks.filter(function (risk) {
    return (filter === "all" || risk.level === filter) &&
      (category === "all" || risk.cat === category) &&
      (!query || (risk.title + " " + risk.cat + " " + risk.signs).toLocaleLowerCase("fa").indexOf(query) >= 0);
  }).sort(function (a, b) { return b.score - a.score; });
  document.getElementById("riskList").innerHTML = risks.length ? risks.map(function (risk) {
    const action = getActionForRisk(risk.id);
    return '<article class="risk-row"><div class="risk-row-main"><div class="risk-row-title"><div><span class="risk-category">' +
      esc(risk.cat) + '</span><h3>' + esc(risk.title) + '</h3></div>' + riskBadge(risk.level) + '</div>' +
      '<div class="risk-score-line"><span><b>' + faNum(risk.score) + '</b> از ۲۵</span><span>احتمال ' + faNum(risk.likelihood) +
      ' × شدت ' + faNum(risk.severity) + '</span><span>پاسخ: ' + esc(risk.answerLabel || answerText(risk.answer)) + '</span></div>' +
      '<div class="risk-details"><p><b>پیامد:</b> ' + esc(risk.impact) + '</p><p><b>نشانه‌ها:</b> ' + esc(risk.signs) +
      '</p><p><b>پیشنهاد کنترلی:</b> ' + esc(risk.action) + '</p></div></div>' +
      '<div class="risk-row-side"><span class="score-number">' + faNum(risk.score) + '<small>/۲۵</small></span>' +
      '<span class="muted">' + (risk.answer === "unknown" ? "نیازمند بررسی" : "امتیاز اولیه") + '</span>' +
      (action ? '<button class="btn btn-sm btn-primary" onclick="editAction(\\'' + esc(action.id) + '\\')">پیگیری اقدام</button>' :
        '<button class="btn btn-sm btn-outline-secondary" onclick="createAction(\\'' + esc(risk.id) + '\\')">ثبت اقدام</button>') +
      '</div></article>';
  }).join("") : '<div class="empty-state"><span>⚑</span><b>' +
    (state.risks.length ? "نتیجه‌ای با این فیلتر پیدا نشد." : "هنوز ریسکی محاسبه نشده است.") +
    '</b><p>ارزیابی زنجیره را تکمیل کنید تا دفتر ریسک ساخته شود.</p><button class="btn btn-primary btn-sm" onclick="showPage(\\'assessment\\')">رفتن به ارزیابی</button></div>';
}

function statusText(status) {
  return ({ stable: "باثبات", watch: "نیازمند پایش", disrupted: "مختل" })[status] || "نامشخص";
}
function criticalityText(value) {
  return ({ critical: "حیاتی", high: "بالا", normal: "معمولی" })[value] || "معمولی";
}
function supplierStatusClass(status) {
  return ({ stable: "supplier-stable", watch: "supplier-watch", disrupted: "supplier-disrupted" })[status] || "";
}
function renderSuppliers() {
  const query = document.getElementById("supplierSearch").value.trim().toLocaleLowerCase("fa");
  const filter = document.getElementById("supplierFilter").value;
  const shown = state.suppliers.filter(function (supplier) {
    return (filter === "all" || supplier.status === filter) &&
      (!query || (supplier.name + " " + supplier.category + " " + supplier.region).toLocaleLowerCase("fa").indexOf(query) >= 0);
  }).sort(function (a, b) {
    const order = { disrupted: 0, watch: 1, stable: 2 };
    return (order[a.status] || 0) - (order[b.status] || 0) ||
      (a.criticality === "critical" ? -1 : 1);
  });
  const critical = state.suppliers.filter(function (supplier) { return supplier.criticality === "critical"; }).length;
  const soleSource = state.suppliers.filter(function (supplier) { return supplier.singleSource; }).length;
  const disrupted = state.suppliers.filter(function (supplier) { return supplier.status === "disrupted"; }).length;
  document.getElementById("supplierSummary").innerHTML =
    '<div><span>کل تأمین‌کنندگان</span><b>' + faNum(state.suppliers.length) + '</b></div>' +
    '<div><span>تأمین‌کننده حیاتی</span><b>' + faNum(critical) + '</b></div>' +
    '<div><span>تک‌منبع</span><b>' + faNum(soleSource) + '</b></div>' +
    '<div><span>مختل</span><b class="' + (disrupted ? "text-danger" : "") + '">' + faNum(disrupted) + '</b></div>';
  document.getElementById("supplierList").innerHTML = shown.length ?
    '<div class="table-responsive"><table class="table supplier-table"><thead><tr><th>تأمین‌کننده / قلم</th><th>اهمیت</th><th>وضعیت</th><th>زمان تأمین</th><th>مبدأ</th><th>وابستگی</th><th>یادداشت</th><th></th></tr></thead><tbody>' +
    shown.map(function (supplier) {
      return '<tr><td><b>' + esc(supplier.name) + '</b><small class="table-sub">' + esc(supplier.category) + '</small></td>' +
        '<td><span class="criticality criticality-' + esc(supplier.criticality) + '">' + criticalityText(supplier.criticality) + '</span></td>' +
        '<td><span class="supplier-status ' + supplierStatusClass(supplier.status) + '"><i></i>' + statusText(supplier.status) + '</span></td>' +
        '<td>' + (supplier.leadTime ? faNum(supplier.leadTime) + " روز" : "—") + '</td><td>' + esc(supplier.region || "ثبت نشده") + '</td>' +
        '<td>' + (supplier.singleSource ? '<span class="dependency-pill">تک‌منبع</span>' : '<span class="muted">چندمنبع</span>') + '</td>' +
        '<td class="notes-cell">' + esc(supplier.notes || "—") + '</td>' +
        '<td><div class="row-actions"><button class="icon-button" aria-label="ویرایش ' + esc(supplier.name) +
        '" title="ویرایش" onclick="openSupplier(\\'' + esc(supplier.id) + '\\')">✎</button><button class="icon-button danger-icon" aria-label="حذف ' +
        esc(supplier.name) + '" title="حذف" onclick="deleteSupplier(\\'' + esc(supplier.id) + '\\')">×</button></div></td></tr>';
    }).join("") + '</tbody></table></div>' :
    '<div class="empty-state"><span>◈</span><b>' + (state.suppliers.length ? "تأمین‌کننده‌ای با این جست‌وجو پیدا نشد." : "هنوز تأمین‌کننده‌ای ثبت نشده است.") +
    '</b><p>تأمین‌کنندگان حیاتی، تک‌منبع و دارای اختلال را ابتدا وارد کنید.</p>' +
    (state.suppliers.length ? "" : '<button class="btn btn-primary btn-sm" onclick="openSupplier()">ثبت تأمین‌کننده</button>') + '</div>';
}
function openSupplier(id) {
  const supplier = id ? state.suppliers.find(function (item) { return item.id === id; }) : null;
  editingSupplierId = supplier ? supplier.id : null;
  document.getElementById("supplierDialogTitle").textContent = supplier ? "ویرایش تأمین‌کننده" : "ثبت تأمین‌کننده";
  document.getElementById("sName").value = supplier ? supplier.name : "";
  document.getElementById("sCategory").value = supplier ? supplier.category : "";
  document.getElementById("sCriticality").value = supplier ? supplier.criticality : "critical";
  document.getElementById("sStatus").value = supplier ? supplier.status : "stable";
  document.getElementById("sLeadTime").value = supplier ? supplier.leadTime : "";
  document.getElementById("sRegion").value = supplier ? supplier.region : "";
  document.getElementById("sSingleSource").checked = supplier ? Boolean(supplier.singleSource) : false;
  document.getElementById("sNotes").value = supplier ? supplier.notes : "";
  document.getElementById("supplierDialog").showModal();
  document.getElementById("sName").focus();
}
document.getElementById("supplierForm").addEventListener("submit", function (event) {
  event.preventDefault();
  const data = {
    name: document.getElementById("sName").value.trim(),
    category: document.getElementById("sCategory").value.trim(),
    criticality: document.getElementById("sCriticality").value,
    status: document.getElementById("sStatus").value,
    leadTime: document.getElementById("sLeadTime").value,
    region: document.getElementById("sRegion").value.trim(),
    singleSource: document.getElementById("sSingleSource").checked,
    notes: document.getElementById("sNotes").value.trim()
  };
  if (!data.name || !data.category) { showToast("نام و قلم تأمین‌شده را وارد کنید."); return; }
  if (editingSupplierId) {
    const old = state.suppliers.find(function (item) { return item.id === editingSupplierId; });
    if (old) Object.assign(old, data, { updatedAt: new Date().toISOString() });
  } else {
    state.suppliers.push(Object.assign({ id: uid("supplier"), createdAt: new Date().toISOString() }, data));
  }
  state.sample = false;
  saveState();
  document.getElementById("supplierDialog").close();
  renderAll();
  showToast("اطلاعات تأمین‌کننده ذخیره شد.");
});
function deleteSupplier(id) {
  const supplier = state.suppliers.find(function (item) { return item.id === id; });
  if (!supplier || !window.confirm("تأمین‌کننده «" + supplier.name + "» حذف شود؟")) return;
  state.suppliers = state.suppliers.filter(function (item) { return item.id !== id; });
  state.sample = false;
  saveState();
  renderAll();
  showToast("تأمین‌کننده حذف شد.");
}

function renderSupplierWatch() {
  const flagged = state.suppliers.filter(function (supplier) { return supplier.status !== "stable"; })
    .sort(function (a, b) {
      const order = { disrupted: 0, watch: 1 };
      return order[a.status] - order[b.status];
    }).slice(0, 4);
  document.getElementById("supplierWatch").innerHTML = flagged.length ? flagged.map(function (supplier) {
    return '<div class="watch-row"><span class="watch-marker ' + supplierStatusClass(supplier.status) + '"></span>' +
      '<div class="watch-copy"><b>' + esc(supplier.name) + '</b><small>' + esc(supplier.category) +
      (supplier.singleSource ? " · تک‌منبع" : "") + '</small></div><span class="supplier-status ' +
      supplierStatusClass(supplier.status) + '"><i></i>' + statusText(supplier.status) + '</span></div>';
  }).join("") : '<div class="compact-empty"><span>✓</span><div><b>مورد نیازمند پایش ثبت نشده</b><small>وضعیت‌ها را از فهرست تأمین‌کنندگان به‌روز کنید.</small></div></div>';
}
function pieSectorPath(startAngle, endAngle) {
  const center = 100, radius = 78;
  const point = function (angle) {
    return { x: (center + radius * Math.cos(angle * Math.PI / 180)).toFixed(2),
      y: (center + radius * Math.sin(angle * Math.PI / 180)).toFixed(2) };
  };
  const start = point(startAngle), end = point(endAngle);
  const large = endAngle - startAngle > 180 ? 1 : 0;
  return "M " + center + " " + center + " L " + start.x + " " + start.y + " A " + radius + " " + radius +
    " 0 " + large + " 1 " + end.x + " " + end.y + " Z";
}
function renderRiskDistribution() {
  const host = document.getElementById("riskDistribution");
  const levels = RISK_LEVELS.map(function (level) {
    return Object.assign({}, level, { count: state.risks.filter(function (risk) { return risk.level === level.key; }).length });
  });
  const total = levels.reduce(function (sum, level) { return sum + level.count; }, 0);
  if (!total) {
    host.innerHTML = '<div class="chart-empty"><div class="empty-chart-mark">—</div><b>نتیجه‌ای برای نمایش نداریم</b><p>با محاسبه ارزیابی، ترکیب ریسک‌ها اینجا دیده می‌شود.</p></div>';
    return;
  }
  let angle = -90;
  const slices = levels.filter(function (level) { return level.count > 0; });
  const paths = slices.length === 1 ?
    '<circle cx="100" cy="100" r="78" fill="' + slices[0].color + '"><title>' + slices[0].label + ": " + faNum(slices[0].count) + '</title></circle>' :
    slices.map(function (level) {
      const start = angle;
      angle += level.count / total * 360;
      return '<path d="' + pieSectorPath(start, angle) + '" fill="' + level.color + '"><title>' + level.label +
        ": " + faNum(level.count) + '</title></path>';
    }).join("");
  host.innerHTML = '<div class="risk-chart-wrap"><div class="pie-wrap"><svg class="risk-pie" viewBox="0 0 200 200" role="img" aria-label="توزیع سطح ریسک">' +
    paths + '<circle cx="100" cy="100" r="48" fill="#fff"/><text x="100" y="96" text-anchor="middle" class="pie-total">' +
    faNum(total) + '</text><text x="100" y="118" text-anchor="middle" class="pie-caption">ریسک فعال</text></svg></div><ul class="risk-legend">' +
    levels.map(function (level) {
      const percent = Math.round(level.count / total * 100);
      return '<li><span><i style="background:' + level.color + '"></i>' + level.label + '</span><b>' + faNum(level.count) +
        '<small>' + faNum(percent) + '٪</small></b></li>';
    }).join("") + '</ul></div>';
}
function isOverdue(action) {
  return Boolean(action.due && action.due < localISODate() && action.status !== "تکمیل‌شده" && action.status !== "متوقف‌شده");
}
function renderTopRisks() {
  const top = state.risks.slice().sort(function (a, b) { return b.score - a.score; }).slice(0, 4);
  document.getElementById("topRisks").innerHTML = top.length ? top.map(function (risk, index) {
    return '<button class="priority-row" onclick="showPage(\\'risks\\')"><span class="priority-rank">' + faNum(index + 1) +
      '</span><span class="priority-copy"><b>' + esc(risk.title) + '</b><small>' + esc(risk.cat) + '</small></span>' +
      '<span class="priority-score">' + faNum(risk.score) + '<small>/۲۵</small></span>' + riskBadge(risk.level) + '</button>';
  }).join("") : '<div class="compact-empty"><span>⚑</span><div><b>ریسکی محاسبه نشده است</b><small>ارزیابی را تکمیل کنید تا اولویت‌ها مشخص شوند.</small></div></div>';
}
function renderUpcomingActions() {
  const active = activeRiskIds();
  const items = state.actions.filter(function (action) { return active.has(action.riskId) && action.status !== "تکمیل‌شده"; })
    .sort(function (a, b) {
      if (!a.due && !b.due) return 0;
      if (!a.due) return 1;
      if (!b.due) return -1;
      return a.due.localeCompare(b.due);
    }).slice(0, 4);
  document.getElementById("upcomingActions").innerHTML = items.length ? items.map(function (action) {
    const question = findQuestion(action.riskId);
    return '<div class="upcoming-row"><span class="upcoming-dot ' + (isOverdue(action) ? "overdue-dot" : "") + '"></span>' +
      '<div class="upcoming-copy"><b>' + esc(question ? question.risk : action.title) + '</b><small>' + esc(action.owner || "مسئول تعیین نشده") +
      ' · ' + formatDate(action.due) + '</small></div><span class="action-status status-' + (action.status === "در حال انجام" ? "doing" : "open") +
      '">' + esc(action.status) + '</span></div>';
  }).join("") : '<div class="compact-empty"><span>✓</span><div><b>اقدام بازی ثبت نشده</b><small>پس از ارزیابی، اقدام‌های کنترلی اینجا نمایش داده می‌شوند.</small></div></div>';
}
function renderDashboard() {
  const answered = QUESTIONS.filter(function (question) { return Boolean(state.answers[question.id]); }).length;
  const progress = Math.round(answered / QUESTIONS.length * 100);
  const high = state.risks.filter(function (risk) { return risk.level === "high"; }).length;
  const active = activeRiskIds();
  const overdue = state.actions.filter(function (action) { return active.has(action.riskId) && isOverdue(action); }).length;
  document.getElementById("welcome").textContent = state.business.name ? "زنجیره تأمین " + state.business.name : "تصویر امروز زنجیره تأمین";
  document.getElementById("heroBusiness").textContent = state.business.type || "کسب‌وکار";
  document.getElementById("dashboardSummary").textContent = state.assessmentStatus === "needs_recalculation" ?
    "پاسخ‌ها تغییر کرده‌اند؛ برای به‌روزرسانی اولویت‌ها ارزیابی را دوباره محاسبه کنید." :
    (state.assessed ? "آخرین ارزیابی در " + formatDate(state.calculatedAt) + " انجام شده است." : "برای دیدن اولویت‌ها، ارزیابی زنجیره تأمین را شروع کنید.");
  document.getElementById("assessmentCta").textContent = state.assessed ? "بازبینی ارزیابی" : "شروع ارزیابی";
  document.getElementById("progressPercent").textContent = faNum(progress) + "٪";
  document.getElementById("progressRing").style.setProperty("--progress", progress + "%");
  document.getElementById("progressText").textContent = faNum(answered) + " از " + faNum(QUESTIONS.length) + " پاسخ ثبت شده";
  document.getElementById("mRisks").textContent = faNum(state.risks.length);
  document.getElementById("mHigh").textContent = faNum(high);
  document.getElementById("mSuppliers").textContent = faNum(state.suppliers.length);
  document.getElementById("mOverdue").textContent = faNum(overdue);
  document.getElementById("mRiskFoot").textContent = state.assessmentStatus === "needs_recalculation" ? "نتیجه قبلی؛ نیازمند محاسبه مجدد" : "در دفتر ریسک";
  document.getElementById("mSupplierFoot").textContent = faNum(state.suppliers.filter(function (supplier) { return supplier.singleSource; }).length) + " تک‌منبع";
  document.getElementById("recalcBanner").classList.toggle("hidden", state.assessmentStatus !== "needs_recalculation");
  document.getElementById("sampleBanner").classList.toggle("hidden", !state.sample);
  const badge = document.getElementById("dataModeBadge");
  badge.textContent = state.sample ? "داده نمونه" : "داده واقعی";
  badge.classList.toggle("mode-sample", state.sample);
  badge.classList.toggle("mode-real", !state.sample);
  renderRiskDistribution();
  renderSupplierWatch();
  renderTopRisks();
  renderUpcomingActions();
  updatePersistentNotices();
}

function renderActions() {
  const statusFilter = document.getElementById("actionFilter").value;
  const viewFilter = document.getElementById("actionViewFilter").value;
  const active = activeRiskIds();
  const actions = state.actions.filter(function (action) {
    const current = active.has(action.riskId);
    return (statusFilter === "all" || action.status === statusFilter) &&
      (viewFilter === "all" || (viewFilter === "current" ? current : !current));
  }).sort(function (a, b) {
    const activeDifference = Number(active.has(b.riskId)) - Number(active.has(a.riskId));
    if (activeDifference) return activeDifference;
    if (!a.due && !b.due) return 0;
    if (!a.due) return 1;
    if (!b.due) return -1;
    return a.due.localeCompare(b.due);
  });
  document.getElementById("actionList").innerHTML = actions.length ?
    '<div class="table-responsive"><table class="table action-table"><thead><tr><th>ریسک / اقدام</th><th>مسئول و موعد</th><th>وضعیت</th><th>یادداشت</th><th></th></tr></thead><tbody>' +
    actions.map(function (action) {
      const question = findQuestion(action.riskId);
      const risk = state.risks.find(function (item) { return item.id === action.riskId; });
      const current = active.has(action.riskId);
      return '<tr class="' + (!current ? "historical-row" : "") + '"><td><b>' + esc(question ? question.risk : "ریسک قبلی") +
        '</b><small class="table-sub">' + esc(action.title) + '</small><span class="action-link-state">' +
        (current && risk ? riskBadge(risk.level) : '<span class="history-badge">ریسک بازنگری‌شده</span>') + '</span></td>' +
        '<td><span class="owner-line">' + esc(action.owner || "مسئول تعیین نشده") + '</span><small class="table-sub ' +
        (isOverdue(action) ? "text-danger" : "") + '">' + (isOverdue(action) ? "عقب‌افتاده · " : "موعد · ") + formatDate(action.due) + '</small></td>' +
        '<td><span class="action-status ' + (action.status === "تکمیل‌شده" ? "status-done" : action.status === "در حال انجام" ? "status-doing" : "status-open") +
        '">' + esc(action.status) + '</span></td><td class="notes-cell">' + esc(action.notes || "—") + '</td>' +
        '<td><button class="btn btn-sm btn-outline-secondary" onclick="editAction(\\'' + esc(action.id) + '\\')">ویرایش</button></td></tr>';
    }).join("") + '</tbody></table></div>' :
    '<div class="empty-state"><span>✓</span><b>اقدامی در این نما پیدا نشد</b><p>پس از محاسبه ارزیابی، اقدام پیشنهادی برای هر ریسک ساخته می‌شود.</p></div>';
}
function createAction(riskId) {
  const risk = state.risks.find(function (item) { return item.id === riskId; });
  if (!risk) return;
  const action = { id: uid("action"), riskId: riskId, title: risk.action, owner: "", due: "", status: "باز", notes: "" };
  state.actions.push(action);
  saveState();
  editAction(action.id);
}
function editAction(actionId) {
  const action = state.actions.find(function (item) { return item.id === actionId; });
  if (!action) return;
  editingActionId = action.id;
  const question = findQuestion(action.riskId);
  document.getElementById("actionDialogRisk").textContent = question ? question.cat + " · " + question.risk : "ریسک ثبت‌شده";
  document.getElementById("actionTitle").value = action.title || "";
  document.getElementById("actionOwner").value = action.owner || "";
  document.getElementById("actionDue").value = action.due || "";
  document.getElementById("actionStatus").value = action.status || "باز";
  document.getElementById("actionNotes").value = action.notes || "";
  document.getElementById("actionDialog").showModal();
  document.getElementById("actionTitle").focus();
}
document.getElementById("actionDialog").addEventListener("close", function () { editingActionId = null; });
document.getElementById("actionForm").addEventListener("submit", function (event) {
  event.preventDefault();
  const action = state.actions.find(function (item) { return item.id === editingActionId; });
  if (!action) return;
  action.title = document.getElementById("actionTitle").value.trim();
  action.owner = document.getElementById("actionOwner").value.trim();
  action.due = document.getElementById("actionDue").value;
  action.status = document.getElementById("actionStatus").value;
  action.notes = document.getElementById("actionNotes").value.trim();
  action.updatedAt = new Date().toISOString();
  state.sample = false;
  saveState();
  document.getElementById("actionDialog").close();
  renderAll();
  showToast("برنامه اقدام ذخیره شد.");
});

function renderReport() {
  const risks = state.risks.slice().sort(function (a, b) { return b.score - a.score; });
  const high = risks.filter(function (risk) { return risk.level === "high"; }).length;
  const completed = state.actions.filter(function (action) { return action.status === "تکمیل‌شده"; }).length;
  const answered = QUESTIONS.filter(function (question) { return Boolean(state.answers[question.id]); }).length;
  const reportDate = new Date().toLocaleDateString("fa-IR", { year: "numeric", month: "long", day: "numeric" });
  document.getElementById("reportMeta").textContent = "گزارش در " + reportDate + " · " +
    (state.calculatedAt ? "محاسبه " + formatDate(state.calculatedAt) : "ارزیابی هنوز محاسبه نشده");
  document.getElementById("reportMode").textContent = state.sample ? "داده نمونه" : "داده واقعی";
  document.getElementById("reportMode").className = "mode-badge " + (state.sample ? "mode-sample" : "mode-real");
  document.getElementById("reportContent").innerHTML =
    '<div class="report-kpis"><div><span>پاسخ ارزیابی</span><b>' + faNum(answered) + ' از ' + faNum(QUESTIONS.length) +
    '</b></div><div><span>ریسک فعال</span><b>' + faNum(risks.length) + '</b></div><div><span>اولویت بالا</span><b>' + faNum(high) +
    '</b></div><div><span>اقدام تکمیل‌شده</span><b>' + faNum(completed) + '</b></div></div>' +
    (state.assessmentStatus === "needs_recalculation" ? '<div class="notice alert alert-warning">پاسخ‌ها پس از محاسبه تغییر کرده‌اند؛ نتایج این گزارش قدیمی‌اند.</div>' : "") +
    '<h3>مشخصات کسب‌وکار و زنجیره</h3><div class="report-facts"><p><span>نام</span><b>' + esc(state.business.name || "ثبت نشده") +
    '</b></p><p><span>حوزه</span><b>' + esc(state.business.type) + '</b></p><p><span>اندازه</span><b>' + esc(state.business.size) +
    '</b></p><p><span>شهر</span><b>' + esc(state.business.city || "ثبت نشده") + '</b></p><p><span>تأمین‌کنندگان ثبت‌شده</span><b>' +
    faNum(state.suppliers.length) + '</b></p><p><span>اقلام تک‌منبع</span><b>' +
    faNum(state.suppliers.filter(function (supplier) { return supplier.singleSource; }).length) + '</b></p><p><span>پوشش موجودی</span><b>' +
    (state.business.coverageDays ? esc(state.business.coverageDays) + " روز" : "ثبت نشده") + '</b></p><p><span>وابستگی وارداتی</span><b>' +
    (state.business.importShare ? esc(state.business.importShare) + "٪" : "ثبت نشده") + '</b></p></div>' +
    '<h3>ریسک‌های اولویت‌دار</h3>' + (risks.length ?
      '<div class="table-responsive"><table class="table report-table"><thead><tr><th>ریسک</th><th>دسته</th><th>پاسخ</th><th>امتیاز</th><th>سطح</th><th>پیشنهاد کنترل</th></tr></thead><tbody>' +
      risks.map(function (risk) {
        return '<tr><td><b>' + esc(risk.title) + '</b></td><td>' + esc(risk.cat) + '</td><td>' +
          esc(risk.answerLabel || answerText(risk.answer)) + '</td><td>' + faNum(risk.score) + ' / ۲۵</td><td>' +
          riskBadge(risk.level) + '</td><td>' + esc(risk.action) + '</td></tr>';
      }).join("") + '</tbody></table></div>' : '<p class="muted">ریسکی محاسبه نشده است.</p>') +
    '<h3>پیگیری اقدام‌ها</h3>' + (state.actions.length ?
      '<div class="table-responsive"><table class="table report-table"><thead><tr><th>ریسک</th><th>اقدام</th><th>مسئول</th><th>موعد</th><th>وضعیت</th><th>ارتباط</th></tr></thead><tbody>' +
      state.actions.map(function (action) {
        const question = findQuestion(action.riskId);
        return '<tr><td>' + esc(question ? question.risk : "ریسک قبلی") + '</td><td>' + esc(action.title) + '</td><td>' +
          esc(action.owner || "تعیین نشده") + '</td><td>' + formatDate(action.due) + '</td><td>' + esc(action.status) +
          '</td><td>' + (activeRiskIds().has(action.riskId) ? "فعال" : "سابقه") + '</td></tr>';
      }).join("") + '</tbody></table></div>' : '<p class="muted">اقدامی ثبت نشده است.</p>') +
    '<div class="report-method"><b>روش و محدودیت</b><p>امتیاز غربالگری = احتمال پایه × شدت × ضریب شکاف کنترل؛ پاسخ «تا حدی» با ضریب ۰٫۶۵ و «نامشخص» با ضریب ۰٫۸۵ محاسبه می‌شود. «بله» در این نسخه ریسک مرتبط را از فهرست فعال خارج می‌کند. آستانه‌ها: پایین ۱ تا ۶، متوسط ۷ تا ۱۴ و بالا ۱۵ تا ۲۵. نبود ریسک در این گزارش به معنی نبود ریسک واقعی نیست و تکمیل اقدام، اثر آن را اثبات نمی‌کند.</p></div>';
}

function renderSettings() {
  document.getElementById("settingsMode").textContent = state.sample ? "داده نمونه" : "داده واقعی";
  document.getElementById("settingsCalculatedAt").textContent = state.calculatedAt ? formatDate(state.calculatedAt) : "هنوز انجام نشده";
}
function buildExportEnvelope() {
  return {
    schema_version: "1.0", application: "Riskban Supply Chain MVP",
    exported_at: new Date().toISOString(), business: state.business,
    suppliers: state.suppliers,
    assessment: {
      id: "assessment_local", business_id: "business_local",
      questionnaire_version: QUESTIONNAIRE_VERSION, rules_version: RULES_VERSION,
      status: state.assessmentStatus, answers: state.answers, calculated_at: state.calculatedAt
    },
    risk_results: state.risks, actions: state.actions, sample: state.sample
  };
}
function download(filename, content, mime) {
  const blob = new Blob(["\ufeff" + content], { type: mime });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
}
function exportJSON() {
  download("riskban-supply-chain-backup.json", JSON.stringify(buildExportEnvelope(), null, 2), "application/json;charset=utf-8");
  showToast("فایل پشتیبان JSON آماده شد.");
}
function exportCSV() {
  const rows = [["ریسک", "دسته", "امتیاز", "سطح", "اقدام", "مسئول", "موعد", "وضعیت", "یادداشت", "ارتباط ریسک"]];
  state.actions.forEach(function (action) {
    const risk = state.risks.find(function (item) { return item.id === action.riskId; });
    const question = findQuestion(action.riskId);
    rows.push([
      risk ? risk.title : question ? question.risk : "ریسک قبلی",
      risk ? risk.cat : question ? question.cat : "",
      risk ? risk.score : "", risk ? answerText(risk.level) : "",
      action.title, action.owner, action.due, action.status, action.notes,
      activeRiskIds().has(action.riskId) ? "فعال" : "سابقه"
    ]);
  });
  const csv = rows.map(function (row) {
    return row.map(function (value) { return '"' + String(value == null ? "" : value).replace(/"/g, '""') + '"'; }).join(",");
  }).join("\r\n");
  download("riskban-actions.csv", csv, "text/csv;charset=utf-8");
  showToast("فایل CSV اقدام‌ها آماده شد.");
}
document.getElementById("importFile").addEventListener("change", async function (event) {
  const file = event.target.files && event.target.files[0];
  if (!file) return;
  try {
    const parsed = JSON.parse(await file.text());
    const payload = parsed.data || parsed;
    if (!payload.business || typeof payload.business !== "object") throw new Error("missing business");
    const assessment = payload.assessment || {};
    const imported = migrateState({
      business: payload.business, suppliers: payload.suppliers,
      answers: assessment.answers || payload.answers,
      risks: payload.risk_results || payload.risks,
      actions: payload.actions, sample: Boolean(payload.sample),
      assessed: Boolean(assessment.calculated_at || payload.assessed),
      assessmentStatus: assessment.status || payload.assessmentStatus,
      calculatedAt: assessment.calculated_at || payload.calculatedAt
    });
    if (imported.sample) imported.sample = true;
    state = imported;
    saveState();
    renderAll();
    showToast("پشتیبان بازیابی شد.");
  } catch (error) {
    showToast("فایل انتخاب‌شده پشتیبان معتبر ریسک‌بان نیست.");
  } finally {
    event.target.value = "";
  }
});
function startRealData() {
  if (!window.confirm("داده نمونه و اطلاعات فعلی جایگزین می‌شوند. در صورت نیاز ابتدا پشتیبان بگیرید. ادامه می‌دهید؟")) return;
  state = freshState();
  saveState();
  renderAll();
  showPage("business");
  showToast("حالت داده واقعی آماده است؛ مشخصات کسب‌وکار را ثبت کنید.");
}
function clearAllData() {
  if (!window.confirm("همه مشخصات، تأمین‌کنندگان، پاسخ‌ها، ریسک‌ها و اقدام‌ها از این مرورگر پاک شوند؟")) return;
  state = freshState();
  saveState();
  renderAll();
  showPage("dashboard");
  showToast("اطلاعات این مرورگر پاک شد.");
}
function restoreDemoData() {
  if (!window.confirm("داده‌های فعلی جایگزین و داده نمونه نمایشی بارگذاری شوند؟")) return;
  state = demoState();
  saveState();
  renderAll();
  showPage("dashboard");
  showToast("داده نمونه بارگذاری شد.");
}

function renderAll() {
  renderDashboard();
  renderSuppliers();
  renderAssessment();
  renderRisks();
  renderActions();
  renderReport();
  renderSettings();
}
state = loadState();
document.getElementById("restoreDemoButton").addEventListener("click", restoreDemoData);
renderAll();
