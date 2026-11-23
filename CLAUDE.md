# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

KobiPanel is a small-business management panel (customers, income/expense tracking, invoices, appointments, dashboard). Backend is ASP.NET Core 8 Web API + EF Core 8 on SQL Server; frontend is a Create React App (React 19, TypeScript, Tailwind 3, Recharts, axios) in `kobipanel-web/`. UI text, comments, and exception messages are in Turkish — keep new user-facing strings in Turkish.

## Commands

Backend (from repo root):
```bash
dotnet build KobiPanel.sln
dotnet run --project KobiPanel.API --launch-profile https   # https://localhost:7245/swagger
```

EF Core migrations — DbContext lives in Infrastructure, startup/config in API:
```bash
dotnet ef migrations add <Name> --project KobiPanel.Infrastructure --startup-project KobiPanel.API
dotnet ef database update --project KobiPanel.Infrastructure --startup-project KobiPanel.API
```
Connection string (`appsettings.json`) targets a local SQL Server `KobiPanelDb` with Trusted_Connection.

Frontend (from `kobipanel-web/`):
```bash
npm install
npm start                                  # http://localhost:3000
npm run build
npm test                                   # Jest via react-scripts (watch mode)
npm test -- --watchAll=false -t "<name>"   # single test, non-interactive
```
There are no backend test projects and no CI workflows (`.github/workflows/` is empty).

## Architecture

Layered solution; project references flow API → Business → Infrastructure → Core:

- **KobiPanel.Core** — EF entities only. All inherit `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`).
- **KobiPanel.Infrastructure** — `AppDbContext` + migrations. Applies a global `!IsDeleted` query filter to every soft-deletable entity and sets `UpdatedAt` in `SaveChangesAsync`. Deletes should be soft (set `IsDeleted = true`); use `IgnoreQueryFilters()` if deleted rows are needed. Note `Include(...)` of child collections still filters explicitly in places (e.g. `Transactions.Where(t => !t.IsDeleted)`).
- **KobiPanel.Business** — concrete service classes (no interfaces) that use `AppDbContext` directly; no repository layer. Services return anonymous objects (`Task<object>`) rather than DTOs, and signal errors by `throw new Exception("<Turkish message>")`. Every new service must be registered manually with `AddScoped` in `KobiPanel.API/Program.cs`.
- **KobiPanel.API** — thin controllers. Request models are `record` types nested inside the controller. Controllers wrap service calls in `try/catch` and map exceptions to `BadRequest`/`NotFound` with `{ message }`.

Multi-tenancy: everything except Auth and Businesses is scoped under `api/businesses/{businessId}/[controller]`, and services filter by `BusinessId`. Ownership of `{businessId}` is enforced by the `[BusinessOwner]` filter (`KobiPanel.API/Filters/BusinessOwnerAttribute.cs`, backed by `BusinessService.IsOwnerAsync`), which returns 404 for businesses the caller doesn't own. Any new business-scoped controller must carry `[BusinessOwner]` next to `[Authorize]`. Services must also verify that foreign keys taken from the request body (`customerId`, `categoryId`, …) belong to the same `businessId`.

Auth: JWT bearer (settings in `JwtSettings` of `appsettings.json`) plus `RefreshToken` entity; passwords hashed with BCrypt in `AuthService`. CORS policy `AllowFrontend` allows only `http://localhost:3000`.

Frontend:
- `src/services/api.ts` — shared axios instance with hardcoded `baseURL` `https://localhost:7245/api`, attaches `Bearer` token from `localStorage`, and on any 401 clears storage and redirects to `/login`.
- `src/App.tsx` — defines `AuthContext`/`useAuth()` holding `token`, `userId`, `businessId`, `fullName` (all mirrored in `localStorage`) and the routes. Authenticated pages render inside `components/Layout.tsx`; pages build business-scoped URLs from `useAuth().businessId`.
- Styling is Tailwind utility classes; icons from `lucide-react`; charts via Recharts.
