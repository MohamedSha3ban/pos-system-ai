export const environment = {
  production: false,
  // Points at the dedicated POS.Gateway.Admin project (port 5001) -- not a shared API.
  // web-storefront and the mobile app each talk to their own gateway instead.
  apiBaseUrl: 'https://localhost:5001/api'
};
