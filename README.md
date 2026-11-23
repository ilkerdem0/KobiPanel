# KobiPanel — Küçük İşletme Yönetim Sistemi

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-19-blue)
![License](https://img.shields.io/badge/License-MIT-green)

Küçük ve orta ölçekli işletmeler için geliştirilmiş, modern ve kullanımı kolay işletme yönetim paneli. Müşteri takibi, gelir-gider yönetimi, fatura oluşturma ve raporlama özelliklerini tek bir platformda sunar.

## Hedef Kitle

Berberler, kuaförler, küçük marketler, oto yıkama/servisler, butikler, klinikler, kafeler ve küçük restoranlar.

## Özellikler

- **Çoklu İşletme** — Bir kullanıcı birden fazla işletme yönetebilir; her işletmenin verisi yalnızca sahibine açıktır
- **Müşteri Yönetimi** — Ekleme, arama, sayfalama, müşteri detayı ve işlem geçmişi
- **Gelir-Gider Takibi** — Gelir/gider kaydı, vadeli (ileri tarihli) işlemler ve tahsilat takibi, dönem özeti, günlük grafik
- **Faturalama** — Kalemli fatura oluşturma, KDV hesabı, otomatik fatura numarası (`KBP-2026-0001`), durum takibi (Taslak / Gönderildi / Ödendi / İptal)
- **Dashboard** — Günlük ve aylık gelir-gider, net kâr, bekleyen faturalar, son 30 gün grafiği
- **Kimlik Doğrulama** — JWT + otomatik yenilenen refresh token

**Planlanan:** PDF fatura çıktısı, randevu sistemi (veri modeli hazır, API ve arayüz henüz yok).

## Teknolojiler

**Backend:** ASP.NET Core 8 Web API, Entity Framework Core 8, MSSQL Server, JWT Authentication, FluentValidation, Swagger/OpenAPI

**Frontend:** React 19, TypeScript, Tailwind CSS, Recharts, Axios

**Test & CI:** xUnit entegrasyon testleri (WebApplicationFactory + EF InMemory), GitHub Actions

## API Endpoints

`Auth` dışındaki tüm endpoint'ler `Authorization: Bearer <token>` ister. `/api/businesses/{id}/...` altındaki endpoint'ler yalnızca işletmenin sahibine açıktır; başkasının işletmesi için `404` döner.

Hatalar her zaman `{ "message": "..." }` biçiminde döner. Doğrulama hatalarında ayrıca alan bazlı `errors` nesnesi bulunur.

### Auth
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/Auth/register | Kullanıcı kaydı |
| POST | /api/Auth/login | Giriş, access + refresh token |
| POST | /api/Auth/refresh | Token yenileme (eski refresh token iptal edilir) |
| POST | /api/Auth/logout | Refresh token'ı iptal et |

### Businesses
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/Businesses | Yeni işletme oluştur |
| GET | /api/Businesses | Kullanıcının işletmeleri |
| GET | /api/Businesses/{id} | İşletme detayı |
| PUT | /api/Businesses/{id} | İşletme güncelle |
| DELETE | /api/Businesses/{id} | İşletme sil |

### Customers
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/businesses/{id}/Customers | Müşteri ekle |
| GET | /api/businesses/{id}/Customers | Müşteri listesi (`search`, `page`, `pageSize`) |
| GET | /api/businesses/{id}/Customers/{cid} | Müşteri detayı ve işlem geçmişi |
| PUT | /api/businesses/{id}/Customers/{cid} | Müşteri güncelle |
| DELETE | /api/businesses/{id}/Customers/{cid} | Müşteri sil |

### Transactions
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/businesses/{id}/Transactions | Gelir/gider ekle |
| GET | /api/businesses/{id}/Transactions | İşlem listesi (`type`, `paymentStatus`, `from`, `to`, sayfalama) |
| GET | /api/businesses/{id}/Transactions/summary | Gelir-gider özeti (`from`, `to`) |
| GET | /api/businesses/{id}/Transactions/chart | Günlük gelir-gider grafiği (`days`) |
| PUT | /api/businesses/{id}/Transactions/{tid}/complete | Vadeli işlemi tahsil edildi olarak işaretle |
| DELETE | /api/businesses/{id}/Transactions/{tid} | İşlem sil |

### Invoices
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/businesses/{id}/Invoices | Fatura oluştur |
| GET | /api/businesses/{id}/Invoices | Fatura listesi (`status`, sayfalama) |
| GET | /api/businesses/{id}/Invoices/{iid} | Fatura detayı ve kalemleri |
| PUT | /api/businesses/{id}/Invoices/{iid}/status | Fatura durumunu güncelle |
| DELETE | /api/businesses/{id}/Invoices/{iid} | Fatura sil |

### Dashboard
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/businesses/{id}/Dashboard | İşletme özet verileri |

## Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server
- Node.js 18+
- `dotnet-ef` aracı (`dotnet tool install --global dotnet-ef`)

### Backend
```bash
# JWT secret repoda tutulmaz; en az 32 karakter olmalı
dotnet user-secrets set "JwtSettings:Secret" "<en-az-32-karakterlik-rastgele-deger>" --project KobiPanel.API

dotnet ef database update --project KobiPanel.Infrastructure --startup-project KobiPanel.API
dotnet run --project KobiPanel.API --launch-profile https
```
API: `https://localhost:7245/swagger`

### Frontend
```bash
cd kobipanel-web
npm install
npm start
```
Arayüz: `http://localhost:3000`

### Testler
```bash
dotnet test KobiPanel.sln
```
Testler SQL Server gerektirmez.

## Yapılandırma

| Ayar | Nerede | Açıklama |
|------|--------|----------|
| `JwtSettings:Secret` | user-secrets / `JwtSettings__Secret` env var | Zorunlu, en az 32 byte |
| `ConnectionStrings:DefaultConnection` | `appsettings.json` / env var | SQL Server bağlantısı |
| `Cors:AllowedOrigins` | `appsettings.json` / `Cors__AllowedOrigins__0` env var | Frontend adres(ler)i |
| `REACT_APP_API_URL` | `kobipanel-web/.env.local` (bkz. `.env.example`) | Frontend'in API adresi; build sırasında gömülür |

## Proje Yapısı
```
KobiPanel/
├── KobiPanel.API/              # Web API katmanı
│   ├── Controllers/            # API Controller'lar
│   ├── Filters/                # İşletme sahipliği kontrolü
│   ├── Infrastructure/         # Global hata yönetimi
│   ├── Validators/             # İstek doğrulama kuralları
│   └── Program.cs              # Uygulama yapılandırması
├── KobiPanel.Business/         # İş mantığı katmanı (servisler)
├── KobiPanel.Core/             # Domain katmanı (entity'ler, exception'lar)
├── KobiPanel.Infrastructure/   # DbContext ve EF Core migrations
├── KobiPanel.Tests/            # Entegrasyon testleri
└── kobipanel-web/              # React frontend
```

## Lisans

MIT License

## İletişim

**C. İlker Erdem** — ilkererdem33@gmail.com

[LinkedIn](https://linkedin.com/in/cilkererdem) | [GitHub](https://github.com/ilkerdem0)
