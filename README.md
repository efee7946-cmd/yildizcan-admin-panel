# YıldızCan Yönetim Paneli

**YıldızCan Therapy Management System** — [YıldızCan](https://github.com/efee7946-cmd/ozel-egitim) özel eğitim uygulamasının seans, içerik ve denetim yönetim paneli.

Özel gereksinimli çocuklar (4-8 yaş) için geliştirilen YıldızCan mobil/web uygulamasının arkasındaki yönetim aracıdır: öğrenci ilerlemesini izler, veli raporu üretir, uygulamadaki soru havuzunu yönetir ve yapay zekâ destekli içerik üretimi/denetimi sağlar. Ayrı bir .NET projesidir; veriye YıldızCan'ın Node.js API'si üzerinden REST ile erişir, uygulamanın veritabanına doğrudan bağlanmaz.

## Özellikler

### 📋 Öğrenciler
- Tüm hesaplardaki öğrenci profillerinin tek listede görünümü (ad, doğum yılı, bağlı hesap, destek notu)
- Satırdan öğrenci detayına geçiş

### 📈 Öğrenci Detayı
- Profil bilgileri + nesne tanıma özeti (oyun sayısı, doğruluk yüzdesi, hatasız oyun, son oyun) kartları
- **Doğruluk Gelişimi grafiği** — seans başına doğruluk eğrisi; saf inline SVG (JS kütüphanesi yok), hover tooltip'li (tarih + doğru/hata), erişilebilirlik için doğrulanmış renk paleti
- **Seans tabloları** — nesne tanıma ve konuşma pratiği geçmişi
- **Veli Raporu (PDF)** — QuestPDF ile tek sayfalık rapor: profil, özet kutuları, seans tabloları, sayfa numaralı altbilgi; tek tıkla indirme
- **AI Analizi** — seans verilerinin sayısal özetinden Gemini ile değerlendirme: güçlü alanlar, zorlanılan alanlar ve sonraki seans önerisi ("klinik değerlendirme yerine geçmez" notuyla)

### ✏️ İçerik (Soru Havuzu)
Uygulamanın konuşma pratiği modülündeki 10 konu (Selamlaşma, Duygular, Yemek, Hayaller...) için soru yönetimi:

- **Yerleşik sorular** — uygulamayla gelen 60 sorunun tamamı panelde listelenir; düzenlenebilir, gizlenebilir, tek tıkla özgün haline döndürülebilir (Özgün / Düzenlendi / Gizli rozetleri)
- **Özel sorular** — TR (zorunlu) + EN metin, eğitim hedefi ve görsel arama terimiyle yeni soru ekleme
- **Taslak → Yayınla akışı** — kaydedilen soru önce taslaktır; "Yayınla" denince ~5 dk içinde tüm uygulamalara iner, "Geri Çek" ile anında havuzdan düşer
- **AI ile Öner (Gemini)** — konu + seviye (kelime/cümle/anlatma) + yaş aralığı + adet seç, özel eğitime uygun soru önerileri üret; beğendiklerini tek tıkla taslağa ekle. Üretilen hiçbir içerik insan onayı olmadan yayına giremez.

### 🔍 AI Denetimi
- Uygulamada Gemini'nin çocuklara verdiği cevapların incelenmesi (son 200)
- Uygunsuz/hatalı cevabı not ekleyerek işaretleme, "sadece işaretliler" süzgeci — işaretler prompt iyileştirmesine girdi sağlar

### 🔐 Panel Girişi
- ASP.NET Core Identity ile oturum; ilk hesap açıldıktan sonra kayıt otomatik kapanır

## Mimari

```
┌─────────────┐     ┌──────────────────┐     REST      ┌────────────────────┐
│  Blazor UI  │ ──▶ │  ASP.NET Core 9  │ ────────────▶ │  YıldızCan Node API │
│  (Server)   │     │  (bu proje)      │  HttpClient   │  (Vercel serverless)│
└─────────────┘     └────────┬─────────┘               └─────────┬──────────┘
                             │ EF Core                            │
                             ▼                                    ▼
                    ┌─────────────────┐                 ┌─────────────────┐
                    │ SQLite (yerel)  │                 │   PostgreSQL    │
                    │ panel hesapları │                 │ uygulama verisi │
                    └─────────────────┘                 └─────────────────┘
```

Temel karar: panel, YıldızCan'ın veritabanına **doğrudan dokunmaz**. Tüm uygulama verisi Node API'nin yönetim uçları üzerinden okunur/yazılır; panelin kendi veritabanı yalnızca panel oturumlarını tutar. İki sistem gevşek bağlıdır — panel tarafındaki hiçbir değişiklik uygulamayı riske atmaz.

## Teknoloji

| Katman | Teknoloji |
|---|---|
| Arayüz | Blazor Web App (Server interactivity), Bootstrap 5 |
| Backend | ASP.NET Core 9 |
| Kimlik | ASP.NET Core Identity + EF Core (SQLite) |
| PDF | QuestPDF (Community) |
| Grafik | Server-rendered inline SVG (bağımlılıksız) |
| API istemcisi | Typed `HttpClient` (`YildizCanApiClient`) |
| AI | Google Gemini (Node API üzerinden) |

## Proje Yapısı

```
YildizCanAdmin.sln
src/
  YildizCanAdmin.Web/
    Components/Pages/
      Students.razor          öğrenci listesi
      StudentDetail.razor     detay + grafik + PDF + AI analizi
      Content.razor           soru havuzu (yerleşik/özel/AI öneri)
      AiReview.razor          AI cevap denetimi
    Components/Account/       Identity giriş/kayıt sayfaları
    Services/
      YildizCanApiClient.cs   Node API istemcisi (tüm uçlar)
      ParentReportPdf.cs      QuestPDF veli raporu üretici
      BuiltinCatalog.cs       yerleşik soru kataloğu (60 soru, TR+EN)
    Data/
      builtin-questions.json  yerleşik soru kataloğu verisi
      app.db                  yerel Identity veritabanı (git'e girmez)
  YildizCanAdmin.Shared/
    AdminContracts.cs         API veri sözleşmeleri (DTO record'ları)
```

## Kurulum ve Çalıştırma

Gereksinim: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
git clone https://github.com/efee7946-cmd/yildizcan-admin-panel.git
cd yildizcan-admin-panel
dotnet build
```

API erişim anahtarını user-secrets ile ver (kaynak koduna ve repo'ya girmez):

```bash
dotnet user-secrets set "YildizCan:AdminKey" "<anahtar>" --project src/YildizCanAdmin.Web
```

Çalıştır:

```bash
dotnet run --project src/YildizCanAdmin.Web
# veya gelistirme icin (kod degisikliklerini otomatik uygular, tarayiciyi acar):
dotnet watch --project src/YildizCanAdmin.Web
```

Tarayıcıda `http://localhost:5256` → ilk açılışta **Register** ile panel hesabını oluştur (ilk hesaptan sonra kayıt kapanır) → giriş yap.

API adresi `appsettings.json` → `YildizCan:ApiBaseUrl` altında yapılandırılır.

## Durum

- [x] v1 — Panel girişi, öğrenci listesi, seans detayı, doğruluk grafiği, PDF veli raporu
- [x] v2 — Soru havuzu yönetimi (özel + yerleşik), taslak→yayınla akışı, TR/EN içerik, AI cevap denetimi
- [x] v3 — Gemini ile soru önerisi üretimi, öğrenci AI analizi
- [ ] Fikirler: AI analiz paragrafının PDF rapora eklenmesi, işaretli cevaplardan otomatik prompt iyileştirme önerileri

## İlgili Depolar

- [YıldızCan](https://github.com/efee7946-cmd/ozel-egitim) — uygulamanın kendisi (vanilla JS + Node/Vercel), App Store'da ve Google Play'de (kapalı test)
