export interface Product {
  id: string;
  name: string;
  description?: string;
  sku: string;
  price: number;
  categoryName?: string;
  isActive: boolean;
  quantityOnHand: number;
}

export interface CartLine {
  product: Product;
  quantity: number;
}

export interface CustomerAuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  fullName: string;
  tenantId: string;
}

export interface Tender {
  method: string;
  amount: number;
  paymentToken?: string;
}

export interface OrderResponse {
  id: string;
  status: string;
  channel: string;
  subtotal: number;
  taxTotal: number;
  discountTotal: number;
  tipTotal: number;
  grandTotal: number;
  createdAtUtc: string;
}
