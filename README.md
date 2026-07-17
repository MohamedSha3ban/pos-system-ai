# AI-Powered POS — Implementation

Backend (.NET 8, modular monolith), Angular 18 web app, and Flutter mobile app for a general-purpose POS. Register a business → log in → manage products/categories → check out with a chosen payment method (Stripe-backed) → stock decrements → AI reorder suggestions available.

As before: this sandbox has no network access to NuGet/pub.dev/npm registries, so I wrote real, correct source files but couldn't `dotnet build` / `ng serve` / `flutter run` them here. Restore packages locally to run everything.

## Structure

```
pos-system/
  backend/    ASP.NET Core 8 -- modular monolith (see "Backend architecture" below)
  web/        Angular 18 (standalone components) -- POS checkout + Products/Categories admin CRUD
  mobile/     Flutter -- matching POS checkout + Products/Categories admin CRUD
```

## Backend architecture: modular monolith

The backend is organized into **bounded-context modules** rather than one flat layer per project. Each module owns its slice of Domain/Application/Infrastructure code and exposes its own DI registration extension method:

```
POS.Domain/
  Common/BaseEntity.cs
  Modules/
    Identity/    Entities: Tenant, Location, User | Enums: UserRole
    Catalog/     Entities: Category, Product, InventoryItem
    Orders/      Entities: Customer, Order, OrderItem, Payment | Enums: OrderStatus, PaymentMethod, PaymentStatus

POS.Application/
  Common/Interfaces/IApplicationDbContext.cs        (shared persistence contract)
  Modules/
    Identity/    DTOs, ITokenService, AuthService, IdentityApplicationModule.cs
    Catalog/     DTOs, ProductService (full CRUD), CategoryService (full CRUD), CatalogApplicationModule.cs
    Orders/      DTOs, IPaymentProcessor, OrderService (checkout), OrdersApplicationModule.cs
    Insights/    IForecastingService (AI forecasting contract)

POS.Infrastructure/
  Persistence/           ApplicationDbContext, Migrations/
  Modules/
    Identity/    JwtTokenService, IdentityInfrastructureModule.cs
    Payments/    CashPaymentProcessor, StripeCardPaymentProcessor, StripeWalletPaymentProcessor,
                 StripeOptions, PaymentsInfrastructureModule.cs
    Insights/    SimpleMovingAverageForecastingService, InsightsInfrastructureModule.cs
  DependencyInjection.cs  (composition root -- wires every module together)

POS.API/
  Controllers/
    Identity/    AuthController
    Catalog/     ProductsController (full CRUD), CategoriesController (full CRUD)
    Orders/      OrdersController (checkout)
    Insights/    InsightsController
    Payments/    StripeIntentController, StripeWebhookController
```

**Why a modular monolith and not separate microservices/assemblies per module:** a single shared `ApplicationDbContext`/database keeps this simple to run and deploy while still giving you clean bounded-context boundaries in code (each module only touches its own DbSets, has its own DI registration, and could be split into a separate service+database later with minimal rewiring). Going all the way to separate `.csproj` files per module was deliberately skipped -- it would multiply build config for little real benefit at this stage, and you can extract a module into its own assembly/service later exactly when you actually need independent deployment or scaling for that module.

## Migrations

I hand-authored `POS.Infrastructure/Persistence/Migrations/20260717000000_InitialCreate.cs` with `Up()`/`Down()` matching the full entity model (all 10 tables, FKs, indexes) -- **but** the companion `.Designer.cs` and `ApplicationDbContextModelSnapshot.cs` files that EF's CLI normally generates alongside a migration are intentionally not included, since hand-rolling those accurately without running the actual tool isn't reliable (they encode a full serialized model snapshot EF uses to diff future migrations against).

**Recommended:** once you can restore packages locally, delete that file and run:
```bash
cd backend
dotnet ef migrations add InitialCreate --project src/POS.Infrastructure --startup-project src/POS.API
dotnet ef database update --project src/POS.Infrastructure --startup-project src/POS.API
```
EF will generate the correct migration (Up/Down + Designer + Snapshot) from the current model. You can diff it against my hand-written file as a sanity check -- the schema should match.

## Stripe integration

Real Stripe.net (`Stripe.net` NuGet package) calls are wired in for card and digital wallet payments:

- **`StripeCardPaymentProcessor`** / **`StripeWalletPaymentProcessor`** (Payments module) -- create a `PaymentIntent` via the Stripe API and report success/failure back through the same `IPaymentProcessor` interface `OrderService` already depends on.
- **`POST /api/payments/stripe/create-intent`** -- call this *before* checkout to get a `clientSecret`, which the web/mobile client uses with Stripe.js / Stripe Elements (web) or the `flutter_stripe` SDK (mobile) to actually collect card details and confirm the payment. The resulting PaymentIntent/PaymentMethod id is then passed as `paymentToken` on the checkout tender.
- **`POST /api/payments/stripe/webhook`** -- receives async status updates (e.g. 3DS confirmations, delayed payment methods). Register this URL in your Stripe dashboard. Signature verification uses `Stripe:WebhookSecret` from config.

Set your keys in `appsettings.json` (or better, user-secrets/environment variables locally):
```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "PublishableKey": "pk_test_...",
  "WebhookSecret": "whsec_..."
}
```

**Not yet wired up (next step):** the actual client-side card-collection UI. Stripe helper stubs exist on both web (`core/services/stripe.service.ts`) and mobile (`services/stripe_service.dart`) that call `create-intent` -- next, add `@stripe/stripe-js` + Stripe Elements on web, and the `flutter_stripe` package on mobile, to mount a real card form and confirm the PaymentIntent client-side before calling checkout.

## CRUD screens

Both web and mobile now have full Products + Categories management, in addition to the POS checkout screen:

- **Web**: `features/products` -- table view, create/edit modal form, inline category rename, delete with confirm. Linked from the POS screen via "Manage catalog →".
- **Mobile**: `screens/products_admin_screen.dart` (tabbed Products/Categories list) + `screens/product_form_screen.dart` (create/edit form). Linked from the POS screen via the catalog icon in the app bar.

Both call the same backend endpoints: `GET/POST/PUT/DELETE /api/products`, `PATCH /api/products/{id}/stock`, `GET/POST/PUT/DELETE /api/categories`.

## Running it

```bash
# Backend
cd backend
dotnet restore
# set up PostgreSQL, update appsettings.json ConnectionStrings:Default and Stripe keys
dotnet ef migrations add InitialCreate --project src/POS.Infrastructure --startup-project src/POS.API
dotnet run --project src/POS.API   # Swagger at https://localhost:5001/swagger

# Web
cd web
npm install
ng serve   # update src/environments/environment.ts if your API port differs

# Mobile
cd mobile
flutter pub get
flutter run   # update baseUrl in lib/services/api_client.dart (10.0.2.2 for Android emulator)
```

## Suggested next implementation steps (in order)

1. Run `dotnet ef migrations add InitialCreate` locally and confirm the full flow end-to-end against Postgres.
2. Wire up real Stripe Elements (web) / `flutter_stripe` (mobile) client-side card collection using the `create-intent` endpoint.
3. Add a real tax engine (region-configurable rates) -- currently hardcoded to 0% in `OrderService`.
4. Add refunds/voids (the `IPaymentProcessor.RefundAsync` methods already exist; just need controller endpoints + UI).
5. Add a QR/bank-transfer processor and a BNPL processor (Tabby/Tamara/Klarna) -- both plug in the same way as Stripe, implementing `IPaymentProcessor`.
6. Layer in the LLM-based "ask your data" reporting feature from the original plan (Insights module).
