#!/usr/bin/env python3
"""End-to-end smoke test for a running O2Ban Docker deployment.
Run: python3 tests/smoke_test.py http://127.0.0.1:5080
"""
import http.cookiejar
import json
import sys
import time
import urllib.error
import urllib.request

BASE = (sys.argv[1] if len(sys.argv) > 1 else "http://127.0.0.1:5080").rstrip("/")
jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
passed = []

def request(path, method="GET", payload=None, expected=(200,)):
    data = None if payload is None else json.dumps(payload, ensure_ascii=False).encode("utf-8")
    req = urllib.request.Request(BASE + path, data=data, method=method,
        headers={"Content-Type": "application/json", "Accept": "application/json"})
    try:
        with opener.open(req, timeout=20) as res:
            status, body = res.status, res.read().decode("utf-8")
    except urllib.error.HTTPError as e:
        status, body = e.code, e.read().decode("utf-8")
    if status not in expected:
        raise AssertionError(f"{method} {path}: expected {expected}, got {status}: {body[:500]}")
    try:
        parsed = json.loads(body) if body else None
    except json.JSONDecodeError:
        parsed = body
    return status, parsed

def check(name, fn):
    fn(); passed.append(name); print(f"[PASS] {name}")

# Wait briefly for app readiness when invoked immediately after compose up.
last = None
for _ in range(20):
    try:
        request("/health/ready")
        break
    except Exception as exc:
        last = exc; time.sleep(3)
else:
    raise SystemExit(f"Application is not ready: {last}")

check("Persian RTL web application loads", lambda: request("/"))
check("liveness", lambda: request("/health/live"))
check("MariaDB readiness", lambda: request("/health/ready"))
check("login with seeded expert", lambda: request("/api/auth/login", "POST", {"username":"expert1","password":"O2ban@2026"}))
check("authenticated identity", lambda: request("/api/auth/me"))

for route in ("dashboard", "companies", "suppliers", "products", "requirements", "rfqs", "offers", "evidence", "feedback", "reports"):
    check(f"GET {route}", lambda route=route: request("/api/" + route))

_, initial_suppliers = request("/api/suppliers")
_, initial_products = request("/api/products")
assert len(initial_suppliers) >= 6 and len(initial_products) >= 8
check("demo seed contains supplier and product master data", lambda: None)

# Company insert and readback.
company = {"name":"شرکت آزمون " + str(int(time.time())),"tier":"Tier 2","city":"تهران",
    "contactName":"رابط آزمون","contactRole":"کارشناس خرید","relationshipStatus":"شناسایی‌شده","notes":"رکورد تست"}
request("/api/companies", "POST", company, expected=(201,)); check("company insert", lambda: None)

# Invalid supplier must fail with a useful 400 instead of a database exception.
check("supplier validation rejects missing capabilities", lambda: request("/api/suppliers", "POST",
    {"name":"تأمین‌کننده نامعتبر تست"}, expected=(400,)))
check("supplier validation rejects malformed national ID", lambda: request("/api/suppliers", "POST",
    {"name":"تأمین‌کننده شناسه نامعتبر","nationalId":"123","capabilities":"توانمندی تست"}, expected=(400,)))

suffix = str(int(time.time()))
supplier = {"name":"تأمین‌کننده آزمون " + suffix, "nationalId":"", "city":"تهران",
    "capabilities":"تأمین قطعات و ملزومات آزمایشی", "products":"قطعه نمونه",
    "contactName":"کارشناس تست", "paymentTerms":"۳۰ روزه", "leadTimeDays":4,
    "evidenceStatus":"اعلام‌شده", "initialScore":50, "notes":"رکورد خودکار تست؛ قابل حذف پس از آزمون."}
status, created_supplier = request("/api/suppliers", "POST", supplier, expected=(201,))
assert created_supplier["id"] > 0 and created_supplier["name"] == supplier["name"]
check("supplier insert and generated identity", lambda: None)

product = {"code":"TEST-"+suffix,"name":"کالای آزمون","category":"آزمایشی","unit":"عدد","technicalDescription":"شرح تست"}
_, created_product = request("/api/products", "POST", product, expected=(201,))
check("product insert", lambda: None)
check("duplicate product code is rejected", lambda: request("/api/products", "POST", product, expected=(409,)))

requirement = {"title":"درخواست آزمون " + suffix,"category":"آزمایشی","description":"شرح نیاز تستی","quantity":3,
    "unit":"عدد","deliveryLocation":"تهران","requiredWithinDays":10,"paymentTerms":"نقدی","priority":"عادی"}
_, created_req = request("/api/requirements", "POST", requirement, expected=(201,))
check("purchase requirement insert", lambda: None)
check("supplier matching endpoint", lambda: request(f"/api/matches/{created_req['id']}"))
check("missing requirement returns 404", lambda: request("/api/matches/2147483000", expected=(404,)))

rfq = {"code":"RFQ-TEST-"+suffix,"title":"استعلام تست","requirementTitle":requirement["title"],
    "invitedSuppliers":supplier["name"],"deadline":"2030-01-01T20:30:00Z"}
_, created_rfq = request("/api/rfqs", "POST", rfq, expected=(201,))
check("RFQ insert", lambda: None)

offer = {"rfqCode":rfq["code"],"supplierName":supplier["name"],"unitPrice":12500,"quantity":3,
    "deliveryDays":4,"paymentTerms":"۳۰ روزه","technicalFit":"نیازمند تأیید","validUntil":"2030-01-02T20:30:00Z","notes":"پیشنهاد تست"}
request("/api/offers", "POST", offer, expected=(201,)); check("offer insert", lambda: None)

evidence = {"supplierName":supplier["name"],"claim":"توان تأمین کالای آزمون","source":"اظهار شرکت",
    "status":"اعلام‌شده","notes":"شاهد آزمایشی"}
request("/api/evidence", "POST", evidence, expected=(201,)); check("evidence insert", lambda: None)

feedback = {"type":"پیشنهاد","module":"تأمین‌کنندگان","rating":5,"description":"تست خودکار فرم بازخورد"}
request("/api/feedback", "POST", feedback, expected=(201,)); check("feedback insert", lambda: None)

# Verify persistence by reading the newly inserted records back.
_, suppliers = request("/api/suppliers")
assert any(x["name"] == supplier["name"] for x in suppliers)
check("supplier persistence/readback", lambda: None)
_, products = request("/api/products")
assert any(x["code"] == product["code"] for x in products)
check("product persistence/readback", lambda: None)

print(f"\n{len(passed)} smoke checks passed. Test records were intentionally created in the database.")
print("Remove records prefixed with the generated TEST/RFQ-TEST names after reviewing the test run.")
