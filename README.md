# KobiPanel — Küçük İşletme Yönetim Sistemi

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![License](https://img.shields.io/badge/License-MIT-green)

Küçük ve orta ölçekli işletmeler için geliştirilmiş, modern ve kullanımı kolay işletme yönetim paneli. Müşteri takibi, gelir-gider yönetimi, fatura oluşturma, randevu sistemi ve raporlama özelliklerini tek bir platformda sunar.

## Hedef Kitle

Berberler, kuaförler, küçük marketler, oto yıkama/servisler, butikler, klinikler, kafeler ve küçük restoranlar.

## Özellikler

- **Müşteri Yönetimi** — Ekleme, arama, işlem geçmişi
- **Gelir-Gider Takibi** — Günlük/haftalık/aylık raporlar, kategori bazlı analiz
- **Fatura Oluşturma** — PDF fatura/fiş üretimi
- **Randevu Sistemi** — Takvim görünümü, hatırlatmalar
- **Dashboard** — Günlük özet, grafikler, KPI'lar
- **JWT Authentication** — Güvenli kimlik doğrulama ve yetkilendirme

## Teknolojiler

**Backend:** ASP.NET Core 8 Web API, Entity Framework Core 8, MSSQL Server, JWT Authentication, Swagger/OpenAPI

**Frontend (geliştiriliyor):** React 18, TypeScript, Tailwind CSS, Recharts

## API Endpoints

### Auth
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/Auth/register | Kullanıcı kaydı |
| POST | /api/Auth/login | Giriş ve JWT token |
| POST | /api/Auth/refresh | Token yenileme |
| POST | /api/Auth/logout | Çıkış |

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
| GET | /api/businesses/{id}/Customers | Müşteri listesi (arama + sayfalama) |
| GET | /api/businesses/{id}/Customers/{cid} | Müşteri detayı |
| PUT | /api/businesses/{id}/Customers/{cid} | Müşteri güncelle |
| DELETE | /api/businesses/{id}/Customers/{cid} | Müşteri sil |

### Transactions
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/businesses/{id}/Transactions | Gelir/gider ekle |
| GET | /api/businesses/{id}/Transactions | İşlem listesi (filtreli) |
| GET | /api/businesses/{id}/Transactions/summary | Gelir-gider özeti |
| DELETE | /api/businesses/{id}/Transactions/{tid} | İşlem sil |

### Dashboard
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/businesses/{id}/Dashboard | İşletme özet verileri |

## Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server
- Node.js 18+ (frontend için)

### Çalıştırma
```bash
cd KobiPanel.API
dotnet restore
dotnet ef database update
dotnet run
```
API: `https://localhost:7245/swagger`

## Proje Yapısı
```
KobiPanel/
├── KobiPanel.API/              # Web API katmanı
│   ├── Controllers/            # API Controller'lar
│   └── Program.cs              # Uygulama yapılandırması
├── KobiPanel.Business/         # İş mantığı katmanı
│   └── Services/               # Servisler
├── KobiPanel.Core/             # Domain katmanı
│   └── Entities/               # Veritabanı modelleri
└── KobiPanel.Infrastructure/   # Altyapı katmanı
    ├── Data/                   # DbContext
    └── Migrations/             # EF Core migrations
```

## Lisans

MIT License

## İletişim

**C. İlker Erdem** — ilkererdem33@gmail.com

[LinkedIn](https://linkedin.com/in/cilkererdem) | [GitHub](https://github.com/ilkerdem0)
