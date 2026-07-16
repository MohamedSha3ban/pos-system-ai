# AI-Powered POS — Implementation Starter

This is a working vertical slice of the full plan: **register a business → log in → view catalog → check out with a chosen payment method → stock decrements → AI reorder suggestions available**. It's a real, runnable foundation, not pseudocode — but you'll need to restore packages locally since this sandbox has no network access to NuGet/pub.dev/npm registries for Angular/Flutter tooling.

## Structure

```
pos-system/
  backend/    ASP.NET Core 8 Web API, Clean Architecture (Domain/Application/Infrastructure/API)
  web/        Angular 18 (standalone components) — login + POS checkout screen
  mobile/     Flutter — matching login + POS checkout screens
```

## Backend (.NET)

```bash
cd backend
dotnet restore
# Set up PostgreSQL and update appsettings.json ConnectionStrings:Default
dotnet ef migrations add InitialCreate --project src/POS.Infrastructure --startup-project src/POS.API
dotnet run --project src/POS.API
```

Swagger UI will be at `https://localhost:5001/swagger` once running.

**What's implemented:**
- Multi-tenant domain model (`Tenant`, `User`, `Product`, `InventoryItem`, `Order`, `Payment`, ...)
- JWT auth (`/api/auth/register-tenant`, `/api/auth/login`)
- Product catalog (`/api/products`)
- Checkout with split-tender support (`/api/orders/checkout`) — decrements stock, charges via the payment orchestration layer
- **Payment orchestration layer** (`IPaymentProcessor`) — Cash, CardPresent (Stripe stub), digital wallets. Swap in real Stripe/Adyen calls in `POS.Infrastructure/Services/PaymentProcessors.cs` — the interface is already wired through `OrderService`, so nothing else changes.
- **AI baseline**: `/api/insights/reorder-suggestions` — moving-average demand forecast + low-stock flagging (`IForecastingService`). Swap the implementation for a real model later without touching callers.

**Not yet implemented (next steps):** tax engine (stubbed at 0%), refunds/voids, offline sync, loyalty/CRM, LLM-based "ask your data" reporting, real Stripe/Adyen integration, EF Core migrations (need to be generated locally with `dotnet ef`).

## Web (Angular)

```bash
cd web
npm install
ng serve
```

Uses standalone components, a JWT auth interceptor, route guards, and a POS checkout screen (`features/pos`) that mirrors the mobile app. Update `src/environments/environment.ts` if your API runs on a different port.

## Mobile (Flutter)

```bash
cd mobile
flutter pub get
flutter run
```

Login screen → POS checkout screen, calling the same backend API. Update `baseUrl` in `lib/services/api_client.dart` (use `10.0.2.2` instead of `localhost` for the Android emulator).

## Suggested next implementation steps (in order)

1. Generate and run the first EF Core migration, confirm the full checkout flow end-to-end against a local Postgres instance.
2. Wire real Stripe Terminal calls into `StripeCardPaymentProcessor`.
3. Add a real tax engine (region-configurable rates) — currently hardcoded to 0%.
4. Build out the Products CRUD UI (web `features/products` is a placeholder) and its mobile equivalent.
5. Add refunds/voids to `OrderService` and the payment processor interface.
6. Layer in the LLM-based "ask your data" reporting feature described in the original plan (Section 5.1).
