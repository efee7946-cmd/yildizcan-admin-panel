# YıldızCan Yönetim Paneli

YıldızCan özel eğitim uygulamasının seans ve içerik yönetim paneli.
Ayrı bir .NET projesidir; veriye **YıldızCan Node admin API'si** (`/api/admin`) üzerinden
REST ile erişir, YıldızCan'ın veritabanına doğrudan dokunmaz.

## Yığın
- ASP.NET Core 9 + Blazor Web App (Server interactivity)
- ASP.NET Core Identity + EF Core (SQLite) — panelin **kendi** kullanıcıları
- Node admin API'ye `YildizCanApiClient` (typed `HttpClient`) ile bağlanır

## Proje yapısı
```
src/
  YildizCanAdmin.Web/        Blazor UI + Identity + API istemcisi
    Components/Pages/         ekranlar (Students.razor …)
    Services/YildizCanApiClient.cs
    Data/                     Identity DbContext + SQLite (app.db)
  YildizCanAdmin.Shared/
    AdminContracts.cs         admin API DTO record'ları (JSON sözleşmesi)
```

## Yapılandırma
`YildizCan:ApiBaseUrl` `appsettings.json`'da (sonu `/` ile biter). `ADMIN_KEY` **gizlidir**,
kaynağa girmez — user-secrets veya ortam değişkeni ile verilir:

```bash
dotnet user-secrets set "YildizCan:AdminKey" "<gercek-admin-key>" --project src/YildizCanAdmin.Web
```

> Güvenlik: `ADMIN_KEY`, YıldızCan'da tüm kullanıcıların (şifreli) öğrenci PII'ını çözen bir okuma
> yoluna açar. Yalnızca bu sunucuda tutulur, tarayıcıya asla verilmez.

## Çalıştırma
```bash
dotnet build
dotnet run --project src/YildizCanAdmin.Web
```
İlk açılışta Identity ile bir hesap oluştur (Register), giriş yap, `Öğrenciler` ekranına git.

## Durum
- [x] Solution iskeleti, Identity, API istemcisi, öğrenci listesi ekranı (derleniyor)
- [ ] Öğrenci detay + seans geçmişi ekranı
- [ ] Başarı yüzdesi grafiği
- [ ] PDF veli raporu (QuestPDF)
- [ ] Register kısıtlama (admin panelinde açık kayıt istenmez)

Plan ve endpoint sözleşmeleri: YıldızCan repo'sundaki `yonetim-paneli-taslak.md`.
