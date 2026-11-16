# استقرار O2Ban Pilot در Coolify

## ۱. اتصال مخزن

در Coolify یک Application از مخزن Git بسازید و روش ساخت را روی Dockerfile قرار دهید:

- **Base Directory:** ریشه مخزن
- **Dockerfile Location:** `./Dockerfile`
- **Ports Exposes:** `8080`

Dockerfile برنامه را روی `0.0.0.0:8080` اجرا می‌کند. در Coolify یک دامنه بسازید و پورت داخلی `8080` را برای مقصد Proxy انتخاب کنید. نیازی به انتشار پورت روی میزبان نیست.

## ۲. پیکربندی و دیتابیس

این استقرار `appsettings.json` موجود در مخزن را بدون تغییر می‌خواند. متغیر محیطی `ConnectionStrings__DefaultConnection` تعریف نکنید تا مقدار فایل جایگزین نشود. اطمینان حاصل کنید که کانتینر Coolify می‌تواند به میزبان و پورت دیتابیس تنظیم‌شده در آن فایل وصل شود.

در بخش Environment Variables این مقادیر را وارد کنید:

| نام | مقدار |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `O2BAN_BOOTSTRAP_ADMIN_EMAIL` | ایمیل مدیر اولیه |
| `O2BAN_BOOTSTRAP_ADMIN_PASSWORD` | گذرواژه قوی مدیر اولیه |
| `O2BAN_SEED_DEMO_DATA` | `false` |
| `AllowedHosts` | دامنه واقعی برنامه |

`O2BAN_BOOTSTRAP_ADMIN_EMAIL` و `O2BAN_BOOTSTRAP_ADMIN_PASSWORD` فقط برای ساخت مدیر اولیه لازم‌اند. پس از نخستین ورود، گذرواژه را تغییر دهید؛ سپس این دو متغیر را می‌توان از تنظیمات Coolify حذف کرد.

## ۳. ذخیره‌سازی شواهد و سلامت سرویس

در Persistent Storage یک Volume قابل نوشتن برای کاربر برنامه به مسیر زیر متصل کنید تا فایل‌ها پس از استقرار مجدد کانتینر باقی بمانند:

```text
/app/App_Data/Evidence
```

Dockerfile یک Health Check داخلی روی مسیر `/health` دارد. اگر در داشبورد Coolify یک Health Check جداگانه تعریف می‌کنید، نوع HTTP، پورت `8080` و مسیر `/health` را بگذارید.

## ۴. انتشار

Deploy را اجرا کنید. در Deployment Logs بررسی کنید که Dockerfile ساخته شده باشد و برنامه بدون خطای اتصال دیتابیس شروع به کار کند. سپس صفحه ورود را از دامنه HTTPS باز کنید و مسیر `/health` را بررسی کنید.

اگر برنامه هنگام شروع به دیتابیس وصل نمی‌شود، میزبان/پورت فعلی داخل `appsettings.json` را از داخل شبکه سرور Coolify بررسی کنید. مسیر `/health` دسترسی به دیتابیس را هم بررسی می‌کند و در صورت قطع اتصال پاسخ ناموفق می‌دهد.

برای چک‌لیست تحویل، [چک‌لیست استقرار](DEPLOYMENT_CHECKLIST_FA.md) را ببینید.
