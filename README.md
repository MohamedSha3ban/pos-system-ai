# AI-Powered POS — Mediator + Three-Gateway Architecture

.NET 8 modular monolith, fronted by **three separate API gateway projects** (one per client), a mediator layer (MediatR) decoupling every controller from concrete services, a CQRS-lite read/write database split, real RBAC, and Stripe payments.

As before: this sandbox has no network access to NuGet/pub.dev/npm registries, so I wrote real, correct source files but couldn't `dotnet build` / `ng serve` / `flutter run` them here. Restore packages locally to run everything.

## Structure

```
pos-system/
  backend/
    src/
      POS.Domain/              Entities, enums, shared Permissions catalog
      POS.Application/         Services (business logic) + Commands/Queries/Handlers (MediatR) + read/write context interfaces
      POS.Infrastructure/      WriteDbContext, ReadDbContext, payment processors, JWT, composition root
      POS.Gateway.Admin/       API #1 -- staff/back-office (port 5001)
      POS.Gateway.Ecommerce/   API #2 -- public storefront (port 5002)
      POS.Gateway.Mobile/      API #3 -- staff mobile app (port 5003)
  web-admin/          Angular -- talks ONLY to POS.Gateway.Admin
  web-storefront/     Angular -- talks ONLY to POS.Gateway.Ecommerce
  mobile/             Flutter -- talks ONLY to POS.Gateway.Mobile
```

**All three gateways share one database and one set of business logic** (Domain/Application/Infrastructure) -- they're not three different backends, they're three different *front doors* onto the same modular monolith, each exposing only the controllers relevant to its client. This is the Backend-for-Frontend (BFF) pattern.

## Mediator (MediatR)

Every controller action across all three gateways now does the same thing: build a Command or Query record, call `_mediator.Send(...)`, return the result. No controller injects a concrete `*Service` anymore.

```csharp
[HttpPost]
[RequirePermission(Permissions.ProductsManage)]
public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    => Ok(await _mediator.Send(new CreateProductCommand(TenantId, request), ct));
```

Each module has `Commands/` and `Queries/` folders (e.g. `POS.Application/Modules/Catalog/Commands/CatalogCommands.cs`) holding `IRequest<TResponse>` records plus their `IRequestHandler<TRequest, TResponse>`. **The handlers are deliberately thin** -- they call straight into the existing `*Service` classes, which already contain the real business logic and are already correctly split across `IWriteDbContext`/`IReadDbContext`. Nothing about the read/write reasoning changed; the mediator sits *above* it, not instead of it.

**Why bother, if the handlers are just pass-throughs?** One `LoggingBehavior` (`POS.Application/Common/Behaviors`) wraps all ~25 requests across every module -- that's the concrete payoff: a cross-cutting concern implemented once instead of copy-pasted into every controller or service method. Validation, caching, authorization, or transaction-wrapping could each be added as another pipeline behavior the same way, without touching a single handler.

## Three API gateways

| Gateway | Port | Exposes | Used by |
|---|---|---|---|
| **POS.Gateway.Admin** | 5001 | Auth, Users, Roles, Permissions, Tenants (platform-admin only), Products, Categories, Inventory, Orders (staff checkout), Insights, Stripe (create-intent + webhook) | `web-admin` |
| **POS.Gateway.Ecommerce** | 5002 | Storefront auth (customer register/login), public catalog browsing, customer checkout, Stripe create-intent | `web-storefront` |
| **POS.Gateway.Mobile** | 5003 | Auth, Products, Categories, Orders (staff checkout), Insights, Stripe create-intent -- trimmed to exactly what the Flutter app calls, no Users/Roles/Tenants/Inventory-list | `mobile` |

Each is a real, separately runnable ASP.NET Core project (own `Program.cs`, own `appsettings.json`, own JWT signing secret, own CORS policy locked to exactly one client origin) that calls the same `AddInfrastructure()` composition root. **Only `POS.Gateway.Admin` runs EF migrations** (`db.Database.Migrate()` in its `Program.cs`) -- the other two assume the schema is already current, since duplicate/racing migration attempts across processes is exactly the kind of thing you don't want three copies of.

**Distinct JWT secrets per gateway is intentional, not an oversight.** A token issued by one gateway is only ever sent back to that same gateway (each client only ever talks to its own gateway) -- there's no cross-gateway token validation happening, so there's no requirement for secrets to match. Keeping them separate means a leaked Mobile gateway secret doesn't compromise Admin gateway tokens.

## Backend: RBAC, Storefront module, read/write split

*(Carried over from previous work -- summarized here; see inline code comments for full detail.)*

- **RBAC**: `Role` (tenant-scoped, holds a CSV of permission codes from a fixed `Permissions` catalog) + `UserRoleAssignment`. Every tenant seeds Owner/Manager/Cashier on signup. Platform-admin access is a `User.IsPlatformAdmin` boolean, set manually in the DB (see below) -- not self-service.
- **Storefront module**: `Customer` has its own optional `PasswordHash` and JWT (no permissions, scoped to one tenant) -- entirely separate identity from staff `User`.
- **Orders**: `Order.Channel` (`InStore`/`Online`) + nullable `CashierUserId` let one `OrderService`/`CheckoutCommand` serve both staff (Admin/Mobile gateways) and customer (Ecommerce gateway) checkout.
- **Read/write split**: `IWriteDbContext` (primary, all mutations + any read needing same-transaction consistency -- checkout, registration) vs `IReadDbContext` (replica-ready, independent list/browse/report reads). Both share one `PosModelConfiguration`.

### Seeding a platform admin
```sql
UPDATE "Users" SET "IsPlatformAdmin" = true WHERE "Email" = 'you@example.com';
```
That user will then see the Tenants section in `web-admin` (talking to `POS.Gateway.Admin`).

## Running it

```bash
# Backend -- three gateways, three terminals
cd backend
dotnet restore
dotnet ef migrations add InitialCreate --context WriteDbContext --project src/POS.Infrastructure --startup-project src/POS.Gateway.Admin
dotnet run --project src/POS.Gateway.Admin       # https://localhost:5001/swagger
dotnet run --project src/POS.Gateway.Ecommerce   # https://localhost:5002/swagger
dotnet run --project src/POS.Gateway.Mobile      # https://localhost:5003/swagger

# Admin portal (talks to :5001)
cd web-admin && npm install && ng serve   # http://localhost:4200

# Storefront (talks to :5002)
cd web-storefront && npm install && ng serve --port 4201   # set environment.ts tenantId first!

# Mobile (talks to :5003, staff app)
cd mobile && flutter pub get && flutter run
```

## Suggested next implementation steps (in order)

1. Run `dotnet ef migrations add InitialCreate --context WriteDbContext` locally (via `POS.Gateway.Admin` as startup project) and confirm all three gateways end-to-end against Postgres.
2. Provision an actual Postgres read replica and point each gateway's `Read` connection string at it.
3. Seed a platform admin and confirm the Tenants screen in web-admin.
4. Wire up real Stripe Elements (both web apps) / `flutter_stripe` (mobile) client-side card collection.
5. Resolve storefront tenant by subdomain/custom domain instead of a hardcoded `environment.ts` value.
6. Extend the Flutter app with the Users/Roles/Tenants/Inventory screens (would mean pointing those specific calls at `POS.Gateway.Admin` instead of Mobile, or adding them to the Mobile gateway if you want mobile staff to manage those too), or build a Flutter storefront app talking to `POS.Gateway.Ecommerce`.
7. Add a validation pipeline behavior (e.g. FluentValidation + a `ValidationBehavior<TRequest,TResponse>`) alongside `LoggingBehavior` -- the mediator's already wired for it.
8. Real tax engine, refunds/voids, regional payment rails, and the LLM-based "ask your data" reporting feature -- all still open from before.
