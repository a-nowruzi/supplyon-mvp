const QUESTIONS = [
  {
    id: "supplier", cat: "تأمین مواد اولیه", title: "وابستگی به تأمین‌کننده",
    question: "آیا برای مواد اولیه حیاتی، تأمین‌کننده جایگزین تأییدشده دارید؟",
    risk: "اختلال در تأمین مواد اولیه", impact: "توقف تولید، افزایش هزینه یا کاهش فروش",
    signs: "تکیه به یک فروشنده، تأخیرهای مکرر یا نوسان شدید قیمت",
    action: "شناسایی و ارزیابی تأمین‌کننده جایگزین؛ تعیین حداقل موجودی اقلام حیاتی؛ ثبت زمان تحویل و کیفیت هر محموله.",
    likelihood: 4, severity: 4
  },
  {
    id: "food", cat: "ایمنی مواد غذایی", title: "کنترل دما و نگهداری",
    question: "آیا دمای یخچال‌ها و شرایط نگهداری مواد حساس به‌صورت منظم ثبت و کنترل می‌شود؟",
    risk: "نگهداری نامناسب مواد غذایی", impact: "فساد مواد، بیماری مصرف‌کننده، ضایعات و آسیب اعتباری",
    signs: "نبود ثبت دما، خرابی تجهیزات یا نگهداری نامشخص مواد حساس",
    action: "تعیین حدود دمایی معتبر برای هر ماده؛ ثبت و بازبینی دما؛ تعیین مسئول و روش واکنش به انحراف؛ بررسی کالیبراسیون تجهیزات.",
    likelihood: 3, severity: 5
  },
  {
    id: "hygiene", cat: "ایمنی مواد غذایی", title: "بهداشت و آلودگی متقاطع",
    question: "آیا برنامه مکتوب نظافت، بهداشت کارکنان و جلوگیری از آلودگی متقاطع اجرا می‌شود؟",
    risk: "آلودگی محصول یا آلودگی متقاطع آلرژن‌ها", impact: "آسیب به مصرف‌کننده، برگشت محصول و تبعات قانونی",
    signs: "نبود سوابق نظافت، ابزار مشترک بدون کنترل یا اعلام ناقص آلرژن‌ها",
    action: "مستندسازی برنامه نظافت؛ آموزش کارکنان؛ تفکیک مواد و ابزار در صورت نیاز؛ کنترل آلرژن‌ها و بررسی برچسب محصول.",
    likelihood: 3, severity: 5
  },
  {
    id: "quality", cat: "کیفیت و تولید", title: "ثبات کیفیت محصول",
    question: "آیا دستور ساخت استاندارد، کنترل وزن و معیار پذیرش محصول دارید؟",
    risk: "ناهماهنگی کیفیت محصول", impact: "افزایش ضایعات، شکایت مشتری و دوباره‌کاری",
    signs: "تفاوت زیاد بین بچ‌ها، نبود دستور ساخت یا کنترل نهایی",
    action: "تعریف دستور ساخت نسخه‌بندی‌شده؛ معیار پذیرش؛ ثبت ضایعات و شکایت‌ها؛ بررسی علت ریشه‌ای انحراف‌ها.",
    likelihood: 3, severity: 3
  },
  {
    id: "equipment", cat: "تجهیزات و تداوم", title: "خرابی تجهیزات حیاتی",
    question: "آیا برای فرهای اصلی، یخچال‌ها و تجهیزات حیاتی برنامه نگهداری پیشگیرانه دارید؟",
    risk: "توقف تولید به دلیل خرابی تجهیزات", impact: "تأخیر سفارش، ضایعات و افزایش هزینه تعمیر",
    signs: "تعمیرات اضطراری تکراری یا نبود سابقه سرویس",
    action: "فهرست تجهیزات حیاتی؛ برنامه سرویس پیشگیرانه؛ ثبت خرابی‌ها؛ تعیین راهکار جایگزین و تأمین‌کننده تعمیرات.",
    likelihood: 3, severity: 4
  },
  {
    id: "people", cat: "نیروی انسانی", title: "وابستگی به افراد کلیدی",
    question: "آیا حداقل دو نفر برای کارهای حیاتی آموزش دیده‌اند و روش‌های اصلی مستند شده‌اند؟",
    risk: "اختلال ناشی از غیبت نیروی کلیدی", impact: "افت ظرفیت، تأخیر و خطای عملیاتی",
    signs: "دانش فقط نزد یک نفر یا نبود آموزش جانشین",
    action: "مستندسازی روش کار؛ آموزش متقابل؛ تعریف جانشین؛ ثبت مهارت‌ها و برنامه آموزش.",
    likelihood: 3, severity: 3
  },
  {
    id: "demand", cat: "مالی و تقاضا", title: "نوسان تقاضا و ضایعات",
    question: "آیا تولید بر اساس پیش‌بینی تقاضا و ثبت ضایعات تنظیم می‌شود؟",
    risk: "تولید بیش از تقاضا و ضایعات بالا", impact: "کاهش حاشیه سود و هدررفت مواد اولیه",
    signs: "دورریز زیاد، موجودی فروش‌نرفته یا کمبود مکرر",
    action: "ثبت فروش روزانه و ضایعات؛ پیش‌بینی تقاضا؛ بازبینی اندازه بچ تولید؛ تحلیل حاشیه سود محصولات.",
    likelihood: 3, severity: 3
  },
  {
    id: "delivery", cat: "حمل‌ونقل و تحویل", title: "حفظ کیفیت در ارسال",
    question: "آیا شرایط بسته‌بندی و حمل با حساسیت محصول و زمان تحویل متناسب است؟",
    risk: "آسیب یا افت کیفیت در حمل‌ونقل", impact: "مرجوعی، هزینه ارسال مجدد و نارضایتی مشتری",
    signs: "تغییر شکل محصول، تأخیر ارسال یا نبود معیار بسته‌بندی",
    action: "تعریف معیار بسته‌بندی و تحویل؛ ثبت زمان ارسال؛ انتخاب روش حمل متناسب با محصول؛ پایش شکایت‌های مربوط به ارسال.",
    likelihood: 3, severity: 3
  },
  {
    id: "compliance", cat: "قانون و اعتبار", title: "مجوزها و الزامات قانونی",
    question: "آیا مسئول مشخصی برای پایش مجوزها، الزامات بهداشتی و اطلاعات برچسب محصول دارید؟",
    risk: "عدم انطباق با الزامات قانونی", impact: "جریمه، توقف فعالیت یا آسیب به اعتبار",
    signs: "مدارک منقضی، مسئول نامشخص یا برچسب تأییدنشده",
    action: "تهیه فهرست الزامات قابل‌اعمال؛ ثبت تاریخ انقضا؛ تعیین مسئول پیگیری؛ بررسی تخصصی برچسب و مستندات.",
    likelihood: 2, severity: 5
  }
];

const DEFAULTS = {
  business: { name: "شیرینی‌سرای نمونه", type: "تولید کیک و شیرینی", size: "۶ تا ۲۰ نفر", city: "", sales: "ترکیبی", processes: "خرید، آماده‌سازی، پخت، بسته‌بندی، فروش" },
  answers: {}, risks: [], actions: [], assessed: false
};
let state = loadState();
let editingActionId = null;
let toastTimeout;

function loadState() {
  try {
    const saved = JSON.parse(localStorage.getItem("riskban_mvp"));
    return saved ? {
      ...structuredClone(DEFAULTS), ...saved,
      business: { ...DEFAULTS.business, ...(saved.business || {}) },
      answers: saved.answers || {}, risks: saved.risks || [], actions: saved.actions || []
    }
      : structuredClone(DEFAULTS);
  } catch (e) { return structuredClone(DEFAULTS) }
}
function saveState() { localStorage.setItem("riskban_mvp", JSON.stringify(state)); }
function showToast(message) {
  const toast = document.getElementById("appToast");
  toast.textContent = message;
  toast.classList.add("is-visible");
  clearTimeout(toastTimeout);
  toastTimeout = setTimeout(() => toast.classList.remove("is-visible"), 3200);
}
function esc(v) { return String(v ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])); }
function showPage(page) {
  document.querySelectorAll("main section").forEach(s => s.classList.add("hidden"));
  document.getElementById("page-" + page).classList.remove("hidden");
  document.querySelectorAll(".nav button").forEach(b => {
    const active = b.dataset.page === page;
    b.classList.toggle("active", active);
    if (active) b.setAttribute("aria-current", "page"); else b.removeAttribute("aria-current");
  });
  const titles = {
    dashboard: ["داشبورد مدیریتی", "تصویر کلی ریسک‌ها و اقدامات کسب‌وکار شما"],
    business: ["مشخصات کسب‌وکار", "اطلاعات پایه برای تنظیم ارزیابی"],
    assessment: ["ارزیابی ریسک‌ها", "پاسخ به پرسش‌های غربالگری اولیه"],
    risks: ["فهرست ریسک‌ها", "ریسک‌های شناسایی‌شده و پیشنهادهای کنترلی"],
    actions: ["برنامه اقدامات", "مسئولیت‌ها، موعدها و وضعیت اجرا"],
    report: ["گزارش نهایی", "خلاصه مدیریتی و خروجی قابل چاپ"]
  };
  document.getElementById("pageTitle").textContent = titles[page][0];
  document.getElementById("pageSubtitle").textContent = titles[page][1];
  if (page === "business") fillBusiness();
  if (page === "assessment") renderQuestions();
  if (page === "risks") renderRisks();
  if (page === "actions") renderActions();
  if (page === "report") renderReport();
  renderDashboard();
  window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth" });
}
document.querySelectorAll(".nav button").forEach(b => b.addEventListener("click", () => showPage(b.dataset.page)));

function fillBusiness() {
  const b = state.business;
  [["bName", "name"], ["bType", "type"], ["bSize", "size"], ["bCity", "city"], ["bSales", "sales"], ["bProcesses", "processes"]]
    .forEach(([id, k]) => document.getElementById(id).value = b[k] || "");
}
document.getElementById("businessForm").addEventListener("submit", e => {
  e.preventDefault();
  const b = state.business;
  [["bName", "name"], ["bType", "type"], ["bSize", "size"], ["bCity", "city"], ["bSales", "sales"], ["bProcesses", "processes"]]
    .forEach(([id, k]) => b[k] = document.getElementById(id).value.trim());
  saveState(); renderDashboard(); showToast("مشخصات کسب‌وکار ذخیره شد.");
});

function renderQuestions() {
  document.getElementById("questionCount").textContent =
    Object.keys(state.answers).filter(k => state.answers[k]).length + " از " + QUESTIONS.length;
  document.getElementById("questionnaire").innerHTML = QUESTIONS.map((q, i) => `
 <div class="risk-card card shadow-sm">
  <div class="risk-meta"><span class="badge text-bg-light text-dark border pill">${esc(q.cat)}</span><span class="badge text-bg-light text-dark border pill">سؤال ${i + 1}</span></div>
  <h3>${esc(q.title)}</h3><p>${esc(q.question)}</p>
  <div class="form-check checkline"><input class="form-check-input" type="radio" name="${q.id}" id="${q.id}-yes" value="yes" ${state.answers[q.id] === "yes" ? "checked" : ""} onchange="setAnswer('${q.id}','yes')">
    <label class="form-check-label" for="${q.id}-yes">بله، کنترل مناسب داریم</label></div>
  <div class="form-check checkline"><input class="form-check-input" type="radio" name="${q.id}" id="${q.id}-partial" value="partial" ${state.answers[q.id] === "partial" ? "checked" : ""} onchange="setAnswer('${q.id}','partial')">
    <label class="form-check-label" for="${q.id}-partial">تا حدی؛ کنترل ناقص است</label></div>
  <div class="form-check checkline"><input class="form-check-input" type="radio" name="${q.id}" id="${q.id}-no" value="no" ${state.answers[q.id] === "no" ? "checked" : ""} onchange="setAnswer('${q.id}','no')">
    <label class="form-check-label" for="${q.id}-no">خیر؛ کنترل نداریم یا نامشخص است</label></div>
 </div>`).join("");
}
function setAnswer(id, value) {
  state.answers[id] = value; saveState();
  document.getElementById("questionCount").textContent =
    Object.keys(state.answers).filter(k => state.answers[k]).length + " از " + QUESTIONS.length;
}

function calculateRisks() {
  const answered = QUESTIONS.filter(q => state.answers[q.id]);
  if (!answered.length) { showToast("برای محاسبه، ابتدا به حداقل یک سؤال پاسخ بده."); document.getElementById("supplier-yes").focus(); return; }
  state.risks = answered.filter(q => state.answers[q.id] !== "yes").map(q => {
    const factor = state.answers[q.id] === "no" ? 1 : .65;
    const score = Math.max(1, Math.round(q.likelihood * q.severity * factor));
    const level = score >= 15 ? "high" : score >= 7 ? "medium" : "low";
    return {
      id: q.id, cat: q.cat, title: q.risk, impact: q.impact, signs: q.signs,
      action: q.action, likelihood: q.likelihood, severity: q.severity, score, level,
      answer: state.answers[q.id]
    };
  });
  // Preserve existing action status, owner and due date for matching risks.
  state.risks.forEach(r => {
    const old = state.actions.find(a => a.riskId === r.id);
    if (!old) state.actions.push({
      id: "a_" + r.id, riskId: r.id, title: r.action, owner: "",
      due: "", status: "باز", notes: ""
    });
  });
  // Keep past actions for risks no longer flagged, but mark them as no longer active.
  state.actions = state.actions.filter(a => state.risks.some(r => r.id === a.riskId) || a.status === "تکمیل‌شده");
  state.assessed = true; saveState(); renderDashboard(); renderRisks();
  showPage("risks");
}
function levelText(level) { return level === "high" ? "بالا" : level === "medium" ? "متوسط" : "پایین"; }
function riskTone(level) { return ({ high: "text-bg-danger", medium: "text-bg-warning", low: "text-bg-success" })[level] || "text-bg-secondary"; }
function riskBadge(level) { return `<span class="badge rounded-pill ${riskTone(level)}">ریسک ${levelText(level)}</span>`; }
function renderRisks() {
  const filter = document.getElementById("riskFilter").value;
  const risks = [...state.risks].filter(r => filter === "all" || r.level === filter)
    .sort((a, b) => b.score - a.score);
  document.getElementById("riskList").innerHTML = risks.length ? risks.map(r => `
 <div class="risk-card card shadow-sm">
  <div class="risk-head"><div><h3>${esc(r.title)}</h3><p class="muted">${esc(r.cat)}</p></div>${riskBadge(r.level)}</div>
  <div class="risk-meta"><span class="badge text-bg-light text-dark border pill">امتیاز ${r.score} از ۲۵</span>
   <span class="badge text-bg-light text-dark border pill">احتمال: ${r.likelihood} از ۵</span><span class="badge text-bg-light text-dark border pill">شدت: ${r.severity} از ۵</span></div>
  <p><b>پیامد احتمالی:</b> ${esc(r.impact)}</p>
  <p><b>نشانه‌های هشدار:</b> ${esc(r.signs)}</p>
  <p><b>پیشنهاد کنترلی:</b> ${esc(r.action)}</p>
  <div class="actions d-flex flex-wrap gap-2 mt-3"><button class="btn btn-primary btn-sm" onclick="editAction('${r.id}')">ثبت / ویرایش اقدام</button></div>
 </div>`).join("") : `<div class="empty">ریسکی برای نمایش وجود ندارد. به پرسشنامه پاسخ بده و محاسبه را اجرا کن.</div>`;
}
function editAction(riskId) {
  const action = state.actions.find(x => x.riskId === riskId);
  if (!action) { showToast("ابتدا ارزیابی را اجرا کن."); return; }
  const risk = state.risks.find(x => x.id === riskId);
  editingActionId = action.id;
  document.getElementById("actionDialogRisk").textContent = risk ? `${risk.cat} · ${risk.title}` : action.title;
  document.getElementById("actionOwner").value = action.owner || "";
  document.getElementById("actionDue").value = action.due || "";
  document.getElementById("actionNotes").value = action.notes || "";
  document.getElementById("actionDialog").showModal();
  document.getElementById("actionOwner").focus();
}
document.getElementById("actionDialog").addEventListener("close", () => { editingActionId = null; });
document.getElementById("actionForm").addEventListener("submit", e => {
  e.preventDefault();
  const action = state.actions.find(x => x.id === editingActionId);
  if (!action) return;
  action.owner = document.getElementById("actionOwner").value.trim();
  action.due = document.getElementById("actionDue").value;
  action.notes = document.getElementById("actionNotes").value.trim();
  saveState();
  document.getElementById("actionDialog").close();
  renderActions(); renderDashboard();
  showToast("برنامه اقدام ذخیره شد.");
});
function updateAction(id, key, value) {
  const a = state.actions.find(x => x.id === id); if (!a) return;
  a[key] = value; saveState(); renderActions(); renderDashboard();
}
function renderActions() {
  const actions = state.actions.filter(a => state.risks.some(r => r.id === a.riskId) || a.status === "تکمیل‌شده");
  document.getElementById("actionList").innerHTML = actions.length ? `
 <div class="table-responsive"><table class="table table-striped table-hover align-middle"><thead><tr><th>ریسک / اقدام</th><th>مسئول و موعد</th><th>وضعیت</th><th>یادداشت</th></tr></thead><tbody>
 ${actions.map(a => {
    const r = state.risks.find(x => x.id === a.riskId); return `<tr>
 <td><b>${esc(r ? r.title : "ریسک قبلی")}</b><p class="muted">${esc(a.title)}</p>
 ${r ? riskBadge(r.level) : ""}</td>
 <td><input class="form-control form-control-sm" aria-label="مسئول" placeholder="نام مسئول" value="${esc(a.owner)}" onchange="updateAction('${a.id}','owner',this.value)">
 <input class="form-control form-control-sm" aria-label="موعد" type="date" value="${esc(a.due)}" onchange="updateAction('${a.id}','due',this.value)" style="margin-top:6px"></td>
 <td><select class="form-select form-select-sm" onchange="updateAction('${a.id}','status',this.value)">
 ${["باز", "در حال انجام", "تکمیل‌شده", "متوقف‌شده"].map(s => `<option ${a.status === s ? "selected" : ""}>${s}</option>`).join("")}
 </select></td>
 <td><textarea class="form-control form-control-sm" aria-label="یادداشت" placeholder="شواهد، هزینه یا توضیحات" onchange="updateAction('${a.id}','notes',this.value)">${esc(a.notes)}</textarea></td>
 </tr>`}).join("")}</tbody></table></div>` :
    `<div class="empty">هنوز اقدامی ثبت نشده است. ارزیابی را اجرا کن تا اقدامات پیشنهادی ساخته شوند.</div>`;
}
const RISK_LEVELS = [
  { key: "high", label: "اولویت بالا", color: "#e5484d" },
  { key: "medium", label: "اولویت متوسط", color: "#e99a20" },
  { key: "low", label: "اولویت پایین", color: "#219653" }
];
function pieSectorPath(startAngle, endAngle) {
  const cx = 100, cy = 100, r = 82;
  const point = angle => ({
    x: (cx + r * Math.cos(angle * Math.PI / 180)).toFixed(2),
    y: (cy + r * Math.sin(angle * Math.PI / 180)).toFixed(2)
  });
  const start = point(startAngle), end = point(endAngle);
  const largeArc = endAngle - startAngle > 180 ? 1 : 0;
  return `M ${cx} ${cy} L ${start.x} ${start.y} A ${r} ${r} 0 ${largeArc} 1 ${end.x} ${end.y} Z`;
}
function renderRiskDistribution(risks) {
  const distribution = document.getElementById("riskDistribution");
  const summary = document.getElementById("riskDistributionSummary");
  const levels = RISK_LEVELS.map(level => ({ ...level, count: risks.filter(r => r.level === level.key).length }));
  const total = levels.reduce((sum, level) => sum + level.count, 0);
  summary.textContent = total ? `${total.toLocaleString("fa-IR")} ریسک ثبت‌شده بر اساس اولویت` :
    "با ثبت ارزیابی، توزیع ریسک‌ها اینجا نمایش داده می‌شود.";
  if (!total) {
    distribution.innerHTML = `<div class="chart-empty">
   <svg class="risk-pie" viewBox="0 0 200 200" aria-hidden="true"><circle cx="100" cy="100" r="82" fill="#e9edf5"/><text x="100" y="96" text-anchor="middle" font-size="30" font-weight="700" fill="#697586">۰</text><text x="100" y="120" text-anchor="middle" font-size="12" fill="#697586">ریسک</text></svg>
   <p>هنوز ریسکی برای نمایش وجود ندارد.</p></div>`;
    return;
  }
  const slices = levels.filter(level => level.count > 0);
  let angle = -90;
  const paths = slices.length === 1
    ? `<circle cx="100" cy="100" r="82" fill="${slices[0].color}"><title>${slices[0].label}: ${slices[0].count}</title></circle>`
    : slices.map(level => {
      const start = angle;
      angle += level.count / total * 360;
      return `<path d="${pieSectorPath(start, angle)}" fill="${level.color}"><title>${level.label}: ${level.count}</title></path>`;
    }).join("");
  const legend = levels.map(level => {
    const percent = Math.round(level.count / total * 100);
    return `<li class="risk-legend-item">
   <span class="risk-legend-label"><i class="risk-legend-dot" style="background:${level.color}"></i>${level.label}</span>
   <span class="risk-legend-value"><strong>${level.count.toLocaleString("fa-IR")}</strong><small>${percent.toLocaleString("fa-IR")}٪</small></span>
  </li>`;
  }).join("");
  distribution.innerHTML = `<div class="risk-chart-wrap">
  <svg class="risk-pie" viewBox="0 0 200 200" role="img" aria-label="نمودار دایره‌ای توزیع سطح ریسک">
   <title>توزیع سطح ریسک</title>${paths}
  </svg>
  <ul class="risk-legend" aria-label="راهنمای سطوح ریسک">${legend}</ul>
 </div>`;
}
function renderDashboard() {
  const risks = state.risks;
  const high = risks.filter(r => r.level === "high").length;
  const done = state.actions.filter(a => a.status === "تکمیل‌شده").length;
  const open = state.actions.filter(a => a.status !== "تکمیل‌شده").length;
  document.getElementById("welcome").textContent = "سلام، مدیر " + (state.business.name || "کسب‌وکار") + " 👋";
  document.getElementById("mTotal").textContent = risks.length;
  document.getElementById("mHigh").textContent = high;
  document.getElementById("mDone").textContent = done;
  document.getElementById("mOpen").textContent = open;
  const count = Object.keys(state.answers).filter(k => state.answers[k]).length;
  const progress = Math.round(count / QUESTIONS.length * 100);
  document.getElementById("assessmentProgress").style.width = progress + "%";
  document.getElementById("assessmentProgress").parentElement.setAttribute("aria-valuenow", progress);
  document.getElementById("progressText").textContent = state.assessed ?
    `ارزیابی انجام شده · ${count} از ${QUESTIONS.length} سؤال پاسخ داده شده` :
    `پیشرفت پاسخ‌گویی: ${count} از ${QUESTIONS.length} سؤال`;
  renderRiskDistribution(risks);
  const top = [...risks].sort((a, b) => b.score - a.score).slice(0, 3);
  document.getElementById("topRisks").innerHTML = top.length ? top.map(r =>
    `<div class="risk-card card shadow-sm"><div class="risk-head"><b>${esc(r.title)}</b><b>${r.score}/25</b></div>
 <div class="risk-meta">${riskBadge(r.level)}<span class="badge text-bg-light text-dark border pill">${esc(r.cat)}</span></div></div>`).join("")
    : `<div class="empty">برای مشاهده اولویت‌ها، ارزیابی را انجام بده.</div>`;
  const recent = state.actions.slice(0, 4);
  document.getElementById("recentActions").innerHTML = recent.length ? recent.map(a =>
    `<div class="checkline"><span>${a.status === "تکمیل‌شده" ? "✓" : "◷"}</span><div style="flex:1"><b>${esc(a.title)}</b><p class="muted">مسئول: ${esc(a.owner || "تعیین نشده")} · موعد: ${esc(a.due || "تعیین نشده")}</p></div><span class="badge text-bg-light text-dark border pill">${esc(a.status)}</span></div>`
  ).join("") : `<div class="empty">پس از ارزیابی، اقدامات پیشنهادی اینجا نمایش داده می‌شوند.</div>`;
}
function renderReport() {
  const risks = [...state.risks].sort((a, b) => b.score - a.score);
  const completed = state.actions.filter(a => a.status === "تکمیل‌شده").length;
  document.getElementById("reportContent").innerHTML = `
 <h3>۱. مشخصات کسب‌وکار</h3>
 <p>نام: ${esc(state.business.name)} · نوع: ${esc(state.business.type)} · اندازه: ${esc(state.business.size)}</p>
 <p>شهر: ${esc(state.business.city || "ثبت نشده")} · فروش: ${esc(state.business.sales)}</p>
 <hr>
 <h3>۲. خلاصه ارزیابی</h3>
 <p>تعداد سؤالات پاسخ‌داده‌شده: ${Object.keys(state.answers).filter(k => state.answers[k]).length} از ${QUESTIONS.length}</p>
 <p>ریسک‌های ثبت‌شده: ${risks.length} · اولویت بالا: ${risks.filter(r => r.level === "high").length} · اقدامات تکمیل‌شده: ${completed}</p>
 <h3>۳. ریسک‌های اولویت‌دار</h3>
 ${risks.length ? `<div class="table-responsive"><table class="table table-striped table-hover align-middle"><thead><tr><th>ریسک</th><th>دسته</th><th>امتیاز</th><th>سطح</th><th>پیشنهاد</th></tr></thead><tbody>
 ${risks.map(r => `<tr><td>${esc(r.title)}</td><td>${esc(r.cat)}</td><td>${r.score}/25</td><td>${levelText(r.level)}</td><td>${esc(r.action)}</td></tr>`).join("")}</tbody></table></div>` : "هنوز ریسکی ثبت نشده است."}
 <h3 style="margin-top:22px">۴. پیگیری اقدامات</h3>
 ${state.actions.length ? `<div class="table-responsive"><table class="table table-striped table-hover align-middle"><thead><tr><th>اقدام</th><th>مسئول</th><th>موعد</th><th>وضعیت</th></tr></thead><tbody>${state.actions.map(a => `<tr><td>${esc(a.title)}</td><td>${esc(a.owner || "تعیین نشده")}</td><td>${esc(a.due || "تعیین نشده")}</td><td>${esc(a.status)}</td></tr>`).join("")}</tbody></table></div>` : "اقدامی ثبت نشده است."}
 <p class="muted" style="margin-top:18px">روش امتیازدهی این نمونه: احتمال × شدت؛ تعدیل پاسخ «تا حدی» با ضریب ۰٫۶۵. امتیازها ابزار غربالگری‌اند و برآورد آماری یا تضمین ایمنی نیستند.</p>`;
}
function download(filename, content, type) {
  const blob = new Blob(["\ufeff" + content], { type });
  const url = URL.createObjectURL(blob); const a = document.createElement("a");
  a.href = url; a.download = filename; a.click(); URL.revokeObjectURL(url);
}
function exportJSON() {
  download("riskban-report.json", JSON.stringify(state, null, 2), "application/json;charset=utf-8");
}
function exportCSV() {
  const rows = [["ریسک", "اقدام", "مسئول", "موعد", "وضعیت", "یادداشت"]];
  state.actions.forEach(a => {
    const r = state.risks.find(x => x.id === a.riskId);
    rows.push([r ? r.title : "ریسک قبلی", a.title, a.owner, a.due, a.status, a.notes]);
  });
  const csv = rows.map(row => row.map(v => '"' + String(v ?? "").replace(/"/g, '""') + '"').join(",")).join("\r\n");
  download("riskban-actions.csv", csv, "text/csv;charset=utf-8");
}

// Initial demo assessment: users can replace these answers with their real data.
if (!state.assessed && !Object.keys(state.answers).length) {
  state.answers = {
    supplier: "no", food: "partial", hygiene: "no", quality: "partial",
    equipment: "partial", people: "no", demand: "partial", delivery: "yes", compliance: "partial"
  };
  state.risks = QUESTIONS.filter(q => state.answers[q.id] !== "yes").map(q => {
    const factor = state.answers[q.id] === "no" ? 1 : .65;
    const score = Math.max(1, Math.round(q.likelihood * q.severity * factor));
    return {
      id: q.id, cat: q.cat, title: q.risk, impact: q.impact, signs: q.signs, action: q.action,
      likelihood: q.likelihood, severity: q.severity, score,
      level: score >= 15 ? "high" : score >= 7 ? "medium" : "low", answer: state.answers[q.id]
    };
  });
  state.actions = state.risks.map(r => ({
    id: "a_" + r.id, riskId: r.id, title: r.action,
    owner: "", due: "", status: "باز", notes: ""
  }));
  state.assessed = true; saveState();
}
fillBusiness();
renderDashboard();
