# AI-Powered POS — Two-Portal Architecture

.NET 8 modular-monolith backend, with **two separate Angular web apps** — an admin/back-office portal and a public customer storefront — plus a Flutter mobile app for staff.

As before: this sandbox has no network access to NuGet/pub.dev/npm registries, so I wrote real, correct source files but couldn't `dotnet build` / `ng serve` / `flutter run` them here. Restore packages locally to run everything.

## Structure

```
pos-system/
  backend/          ASP.NET Core 8 -- modular monolith (Identity/RBAC, Catalog, Orders, Payments, Insights, Storefront)
  web-admin/        Angular 18 -- staff/back-office portal (Users, Roles & Permissions, Tenants, Products, Categories, Inventory, POS checkout)
  web-storefront/   Angular 18 -- public customer portal (browse, cart, customer accounts, self-checkout)
  mobile/           Flutter -- staff app (POS checkout + catalog CRUD), unchanged by this pass
```

**Scope note:** this round of work covers the two *web* portals in full. The Flutter app remains the staff-facing app from before (POS checkout + product/category CRUD) -- it wasn't extended with the new Users/Roles/Tenants/Inventory screens or turned into a second customer-facing mobile app. Both are natural next steps that follow the same pattern already established, just not built out here to keep this pass focused.

## Backend: what's new

### Real RBAC (not just an enum)
- **`Role`** (tenant-scoped, e.g. Owner/Manager/Cashier, plus any custom roles a tenant creates) holds a comma-separated list of permission codes (`Domain.Common.Permissions` -- a fixed catalog: `users.manage`, `roles.manage`, `products.manage`, `categories.manage`, `inventory.manage`, `orders.view`, `orders.checkout`, `tenants.manage`).
- **`UserRoleAssignment`** is the many-to-many between `User` and `Role`.
- Every tenant gets three seeded system roles on signup (Owner = everything, Manager = catalog + orders, Cashier = checkout only) -- editable, but not deletable.
- Staff JWTs carry one `permission` claim per granted permission, computed at login by flattening the user's assigned roles. A custom `[RequirePermission("products.manage")]` attribute (`POS.API/Authorization`) gates controller actions.
- **Platform admin**: a separate, simpler mechanism -- `User.IsPlatformAdmin` (bool). Not settable via any API; flip it directly in the database for your first platform operator (see "Seeding a platform admin" below). Gated by `[RequirePlatformAdmin]`, used only by `TenantsController`.

### Storefront module (new)
Customer identity is deliberately separate from staff `User`/`Role` -- a `Customer` (already existed for loyalty tracking) now optionally has a `PasswordHash` and can register/log in to a specific tenant's storefront. Customer JWTs carry no permissions (a customer can only ever act as themselves) and are scoped to one tenant.

- `POST /api/storefront/{tenantId}/auth/register` / `/login` -- customer account, public
- `GET /api/storefront/{tenantId}/products` -- public catalog browsing, `[AllowAnonymous]`
- `POST /api/storefront/checkout` -- customer self-checkout, requires a customer JWT

### Orders: in-store vs online
`Order` now has a `Channel` (`InStore` | `Online`) and a **nullable** `CashierUserId` (null for online orders -- no staff member rang it up). `OrderService.CreateOrderAsync` is shared by both `OrdersController` (staff, in-store) and `StorefrontOrdersController` (customer, online) -- same checkout logic, same payment orchestration, same stock decrement, different actor.

### Inventory as its own surface
Alongside `ProductsController`'s existing stock field, there's now a dedicated `InventoryController` (`GET/PATCH /api/inventory`) that lists stock across every location with reorder points and a `isLow` flag -- the admin portal's Inventory screen.

### Migration
Rewritten (still hand-authored -- see the in-file comment) to include the RBAC tables, `Order.Channel`, nullable `CashierUserId`, `Tenant.IsActive`, and `Customer.PasswordHash`. Same caveat as before: delete it and run `dotnet ef migrations add InitialCreate --context WriteDbContext` locally once you can restore packages, to get a tooling-verified migration + Designer/Snapshot files. Only `WriteDbContext` ever gets migrations -- see the read/write split section below.

### Read/write database split (CQRS-lite)
Two `DbContext`s, two connection strings, wired through the same DI-registered interfaces every service already depended on -- **`IWriteDbContext`** (exposes `DbSet<T>`, backed by `WriteDbContext`, always the primary database) and **`IReadDbContext`** (exposes `IQueryable<T>` only -- no `Add`/`Update`/`Remove`/`SaveChanges` reachable through the type at all -- backed by `ReadDbContext`, points at a read replica in production).

**The split isn't "all reads go to Read."** Every service was reviewed individually: a read that must be consistent with a write *in the same operation* stays on `IWriteDbContext`; an independent read (a list screen, a login lookup, an analytics query) goes to `IReadDbContext`. Concretely:

| Goes through **Write** | Goes through **Read** |
|---|---|
| `OrderService.CreateOrderAsync` -- entirely on Write, including the product/price/stock lookups. Checkout deciding "is this in stock" against a possibly-lagging replica is the textbook reason not to split this. | `ProductService.GetCatalogAsync` / `GetByIdAsync` -- the highest-traffic read in the app (POS, storefront, admin grid all hit this) |
| Every `Create`/`Update`/`Delete` method -- writes, then builds its response DTO from the entities already in the write context's tracker rather than re-querying (avoids a just-created row briefly 404ing against a lagging replica) | `UserService.GetAllAsync`, `RoleService.GetAllAsync`, `TenantService.GetAllAsync`, `InventoryService.GetAllAsync` -- independent list screens |
| `CustomerAuthService.RegisterAsync` -- check-email-then-create needs to see its own check | `AuthService.LoginAsync`, `CustomerAuthService.LoginAsync` -- pure reads; a login moments after registration hitting replication lag is an accepted, retry-succeeds trade-off |
| `RoleService.DeleteAsync`'s "is this role still assigned to anyone" guard -- must see current state before deleting | `SimpleMovingAverageForecastingService` (the AI reorder-suggestions feature) -- a heavier 30-day analytical scan with no reason to compete with checkout traffic on the primary |

**Schema is shared, migrations are not.** `PosModelConfiguration.Configure()` (`POS.Infrastructure/Persistence`) holds the entity mapping both contexts use -- same tables, same indexes, same relationships -- so there's one place to change it. Only `WriteDbContext` ever gets a migration; `ReadDbContext`'s schema is expected to arrive via database-level replication (e.g. Postgres streaming replication) in production, exactly like its data does.

**Local dev without replica infrastructure:** `appsettings.json` has separate `Write` and `Read` connection strings; point both at the same local Postgres database and everything works correctly -- you just don't get the actual scaling benefit until you add a real replica and repoint `Read` at it.

### Seeding a platform admin
There's no API for this by design (it shouldn't be self-service). After registering at least one tenant through the admin portal, run:
```sql
UPDATE "Users" SET "IsPlatformAdmin" = true WHERE "Email" = 'you@example.com';
```
That user will then see the Tenants section in the admin portal.

## web-admin (staff/back-office portal)

Sidebar-shell layout (`src/app/shell`), routes nested underneath it, each item shown/hidden based on the logged-in user's permissions (`AuthService.hasPermission(code)`) or platform-admin flag:

- **POS checkout** -- unchanged from before (staff-assisted, in-store)
- **Products / Categories** -- unchanged CRUD from before
- **Inventory** (new) -- stock levels across locations, inline adjust, low-stock highlighting
- **Users** (new) -- create/deactivate staff accounts, assign roles
- **Roles & Permissions** (new) -- create custom roles, toggle permission checkboxes, edit/delete (system roles can't be deleted)
- **Tenants** (new, platform-admin only) -- cross-tenant list, activate/deactivate a business

## web-storefront (public customer portal)

A separate, standalone Angular app -- deliberately has no knowledge of staff auth, roles, or the admin API surface. Public product grid → cart (in-memory `CartService` using signals) → checkout, which prompts for customer login/registration inline before placing the order.

**Important limitation:** this starter serves **one tenant's shop per deployment** -- `environment.ts` has a hardcoded `tenantId`/`locationId`. A real multi-tenant storefront would resolve the tenant from a subdomain or custom domain (e.g. `shop.acmecoffee.com` → look up tenant by domain) rather than a build-time constant. Set `tenantId` to the value returned when you register a business through the admin portal.

**Also not wired up yet (same as before):** real card collection. The checkout screen has a payment-method picker but doesn't mount an actual Stripe Elements card form -- `StripeService.createIntent()` exists and is ready to use once you add `@stripe/stripe-js`.

## Running it

```bash
# Backend
cd backend
dotnet restore
dotnet ef migrations add InitialCreate --context WriteDbContext --project src/POS.Infrastructure --startup-project src/POS.API
dotnet run --project src/POS.API   # Swagger at https://localhost:5001/swagger

# Admin portal
cd web-admin
npm install
ng serve   # defaults to http://localhost:4200

# Storefront (run on a different port since both are Angular apps)
cd web-storefront
npm install
ng serve --port 4201   # set environment.ts tenantId first!

# Mobile (staff app, unchanged)
cd mobile
flutter pub get
flutter run
```

## Suggested next implementation steps (in order)

1. Run `dotnet ef migrations add InitialCreate --context WriteDbContext` locally and confirm both portals end-to-end against Postgres.
2. Provision an actual Postgres read replica and point the `Read` connection string at it -- right now both connection strings point at the same database, so the split is architecturally real but not yet delivering its scaling benefit.
3. Seed a platform admin (see above) and confirm the Tenants screen in web-admin.
4. Wire up real Stripe Elements (both web apps) / `flutter_stripe` (mobile) client-side card collection.
5. Resolve storefront tenant by subdomain/custom domain instead of a hardcoded `environment.ts` value, if you want one storefront deployment to serve multiple tenants.
6. Extend the Flutter app with the same Users/Roles/Tenants/Inventory screens, or build a Flutter storefront app mirroring `web-storefront` -- same backend, no new endpoints needed.
7. Add a real tax engine, refunds/voids, regional payment rails (still open from before -- see `OrderService`/`IPaymentProcessor`).
8. Layer in the LLM-based "ask your data" reporting feature from the original plan (Insights module).
