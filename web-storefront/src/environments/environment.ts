export const environment = {
  production: false,
  // Points at the dedicated POS.Gateway.Ecommerce project (port 5002) -- this app never
  // talks to POS.Gateway.Admin at all, even though both ultimately hit the same database.
  apiBaseUrl: 'https://localhost:5002/api',
  // This storefront serves ONE tenant's shop -- a real deployment would resolve this
  // from a subdomain or custom domain per business (e.g. shop.acmecoffee.com). For this
  // starter it's a hardcoded id: set it to the TenantId returned when you register a
  // business through the admin portal.
  tenantId: '00000000-0000-0000-0000-000000000000',
  locationId: '00000000-0000-0000-0000-000000000000'
};
