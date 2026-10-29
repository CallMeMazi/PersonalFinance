# PersonalFinance

برنامه محلی مدیریت مالی شخصی با ASP.NET Core MVC، رابط فارسی و تقویم شمسی. اطلاعات روی SQL Server خودتان ذخیره می‌شود و پس از بستن برنامه باقی می‌ماند.

## پیش‌نیازها

- Windows
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) یا جدیدتر (روی این سیستم SDKهای ۸ و ۱۰ نصب بود)
- SQL Server LocalDB (همراه Visual Studio)، یا SQL Server Express، یا SQL Server معمولی
- Visual Studio 2022 یا دستور `dotnet` در ترمینال

نسخه هدف پروژه: `net8.0`

## تنظیم Connection String

فایل `PersonalFinance/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PersonalFinance;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
}
```

نمونه برای SQL Server Express:

```text
Server=.\\SQLEXPRESS;Database=PersonalFinance;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True
```

نمونه برای SQL Server معمولی:

```text
Server=localhost;Database=PersonalFinance;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True
```

اگر از حساب SQL استفاده می‌کنید، `User ID` و `Password` را اضافه کنید و `Trusted_Connection` را بردارید.

## ساخت دیتابیس و Migration

از ریشه راه‌حل:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project PersonalFinance --startup-project PersonalFinance
dotnet ef database update --project PersonalFinance --startup-project PersonalFinance
```

اگر Migration از قبل در پوشه `PersonalFinance/Migrations` باشد، فقط `database update` کافی است.

هنگام اجرای برنامه هم `Migrate` به‌صورت خودکار اعمال می‌شود و دسته‌بندی‌های نمونه فقط در صورت خالی بودن جدول اضافه می‌گردند. این دسته‌ها قابل ویرایش و حذف هستند.

## اجرا در Visual Studio

1. فایل راه‌حل `PersonalFinance.slnx` یا پروژه `PersonalFinance/PersonalFinance.csproj` را باز کنید.
2. پروژه `PersonalFinance` را به‌عنوان Startup Project انتخاب کنید.
3. پروفایل `http` یا `https` را اجرا کنید.

## اجرا از ترمینال

```bash
cd PersonalFinance
dotnet restore
dotnet build
dotnet run --launch-profile http
```

آدرس پیش‌فرض پروفایل http: `http://localhost:5227`

## فایل‌های محلی ظاهر برنامه

برنامه برای نمایش صفحات به اینترنت نیاز ندارد.

- Bootstrap 5 RTL: `PersonalFinance/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css`
- Bootstrap JS: `PersonalFinance/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js`
- Chart.js: `PersonalFinance/wwwroot/lib/chartjs/chart.umd.min.js`
- استایل سفارشی: `PersonalFinance/wwwroot/css/site.css`
- اسکریپت سفارشی: `PersonalFinance/wwwroot/js/site.js`
- فونت ایران‌سنس: `PersonalFinance/wwwroot/fonts/iransans/`

وزن‌های موجود ایران‌سنس: Light، Regular، Medium، Bold با پسوند `woff2`. اگر خواستید فایل را عوض کنید، همین مسیر را حفظ کنید و `@font-face` در `site.css` را در صورت نیاز به‌روز کنید. در نبود فایل، مرورگر به Tahoma و Segoe UI برمی‌گردد.

## نحوه محاسبه گزارش‌ها

- **درآمد واقعی:** مجموع `ساعات × نرخ ذخیره‌شده همان ثبت کاری` در بازه انتخابی. تغییر نرخ فعلی، درآمد روزهای قبلی را عوض نمی‌کند.
- **نرخ مؤثر بازه:** میانگین موزون نرخ ثبت‌های همان بازه؛ اگر ثبتی نباشد از نرخ فعلی استفاده می‌شود.
- **درآمد مورد انتظار:** ساعات هدف بازه × نرخ مؤثر. اگر هدف تنظیم نشده باشد عدد ساختگی نشان داده نمی‌شود.
- **اختلاف ساعات:** ساعات واقعی − ساعات هدف.
- **اختلاف درآمد:** درآمد واقعی − درآمد مورد انتظار.
- **خالص درآمد:** درآمد واقعی − مجموع هزینه‌های همان بازه.
- **بودجه دسته:** درآمد تحقق‌یافته × درصد تخصیص آن دسته. تخصیص به‌معنی هزینه‌شدن نیست.
- **مانده بودجه:** بودجه دسته − هزینه واقعی همان دسته.
- **درصد پیشرفت کاری:** ساعات واقعی ÷ ساعات هدف × ۱۰۰. اگر از هدف عبور کنید مقدار متن می‌تواند بالای ۱۰۰٪ باشد؛ نوار پیشرفت حداکثر تا ۱۰۰٪ پر می‌شود.
- **عقب‌ماندگی کاری** زیان مالی واقعی نیست؛ جدا از خالص درآمد و تجاوز از بودجه نمایش داده می‌شود.
- هفته از **شنبه تا جمعه** و ماه‌ها بر اساس **تقویم شمسی** محاسبه می‌شوند، نه ماه میلادی.
- هزینه‌ها بر اساس تاریخ وقوع‌شان در همان هفته یا ماه شمسی فیلتر می‌شوند.
- مجموع درصد تخصیص دسته‌ها نمی‌تواند از ۱۰۰ بیشتر شود. کمتر از ۱۰۰ به‌عنوان «تخصیص‌نیافته» دیده می‌شود.

## تست‌های ساده

```bash
dotnet test
```

تست‌ها تبدیل تاریخ شمسی، مرز هفته و ماه، سال کبیسه و فرمول‌های درآمد/بودجه را پوشش می‌دهند.
