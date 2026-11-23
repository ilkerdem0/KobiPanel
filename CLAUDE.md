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

The JWT secret is not in the repo; the API refuses to start without a ≥32-byte `JwtSettings:Secret`. Locally it comes from user-secrets, in production from the `JwtSettings__Secret` env var:
```bash
dotnet user-secrets set "JwtSettings:Secret" "<min 32 chars>" --project KobiPanel.API
```

Tests (xUnit integration tests in `KobiPanel.Tests`, `WebApplicationFactory<Program>` + EF InMemory, no SQL Server needed):
```bash
dotnet test KobiPanel.sln
dotnet test KobiPanel.Tests --filter "FullyQualifiedName~InvoiceNumberTests"   # single class/test
```
`KobiPanelFactory` swaps the DbContext and injects a test JWT secret; `TestUser.CreateAsync` registers a user with its own business and an authenticated client.

Frontend (from `kobipanel-web/`):
```bash
npm install
npm start                                  # http://localhost:3000
npm run build
npm test                                   # Jest via react-scripts (watch mode)
npm test -- --watchAll=false -t "<name>"   # single test, non-interactive
```
There are no CI workflows (`.github/workflows/` is empty).

## Architecture

Layered solution; project references flow API → Business → Infrastructure → Core:

- **KobiPanel.Core** — EF entities only. All inherit `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`).
- **KobiPanel.Infrastructure** — `AppDbContext` + migrations. Applies a global `!IsDeleted` query filter to every soft-deletable entity and sets `UpdatedAt` in `SaveChangesAsync`. Deletes should be soft (set `IsDeleted = true`); use `IgnoreQueryFilters()` if deleted rows are needed. Note `Include(...)` of child collections still filters explicitly in places (e.g. `Transactions.Where(t => !t.IsDeleted)`).
- **KobiPanel.Business** — concrete service classes (no interfaces) that use `AppDbContext` directly; no repository layer. Services return anonymous objects (`Task<object>`) rather than DTOs. Every new service must be registered manually with `AddScoped` in `KobiPanel.API/Program.cs`.
- **KobiPanel.API** — thin controllers with no try/catch. Request models are `record` types nested inside the controller.

Errors: services throw the typed exceptions in `KobiPanel.Core/Exceptions/AppException.cs` (`NotFoundException` 404, `BadRequestException` 400, `ConflictException` 409, `UnauthorizedException` 401) with a Turkish message. `KobiPanel.API/Infrastructure/AppExceptionHandler.cs` turns them into `{ message }` with that status; any other exception is logged and returned as a generic 500. Never throw plain `Exception` for expected errors.

Validation: every request record has a FluentValidation validator in `KobiPanel.API/Validators/RequestValidators.cs` (auto-registered, auto-run). Validation failures return 400 `{ message, errors }`, where `message` is the first error. Rules for optional fields must allow `""`, because the frontend sends empty form fields as empty strings. Allowed enum-like values (`Income`/`Expense`, payment methods, invoice statuses) live in `Rules` there.

Multi-tenancy: everything except Auth and Businesses is scoped under `api/businesses/{businessId}/[controller]`, and services filter by `BusinessId`. Invoice numbers are `KBP-{year}-{seq:D4}` per business; `InvoiceService.GenerateInvoiceNumberAsync` counts soft-deleted invoices too (`IgnoreQueryFilters`) so numbers are never reused, backed by a unique index on `(BusinessId, InvoiceNumber)` with a retry on conflict.

Ownership of `{businessId}` is enforced by the `[BusinessOwner]` filter (`KobiPanel.API/Filters/BusinessOwnerAttribute.cs`, backed by `BusinessService.IsOwnerAsync`), which returns 404 for businesses the caller doesn't own. Any new business-scoped controller must carry `[BusinessOwner]` next to `[Authorize]`. Services must also verify that foreign keys taken from the request body (`customerId`, `categoryId`, …) belong to the same `businessId`.

Auth: JWT bearer (settings in `JwtSettings` of `appsettings.json`) plus a `RefreshToken` entity; passwords are hashed with BCrypt in `AuthService`. Refresh tokens rotate: each `/Auth/refresh` revokes the old token. `/Auth/logout` revokes a refresh token and needs no access token. CORS policy `AllowFrontend` allows only `http://localhost:3000`.

Frontend:
- `src/services/api.ts` — shared axios instance with hardcoded `baseURL` `https://localhost:7245/api`, attaches the `Bearer` token from `localStorage`. On a 401 from a non-`/Auth/` request, it calls `/Auth/refresh` once (one shared request for concurrent 401s), stores the new `token`/`refreshToken` and retries; if the refresh fails, it clears storage and redirects to `/login`.
- `src/App.tsx` — defines `AuthContext`/`useAuth()` holding `token`, `userId`, `businessId`, `fullName` (all mirrored in `localStorage`) and the routes. Authenticated pages render inside `components/Layout.tsx`; pages build business-scoped URLs from `useAuth().businessId`.
- Styling is Tailwind utility classes; icons from `lucide-react`; charts via Recharts.
