export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  fullName: string;
  role: string;
  tenantId: string;
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  barcode?: string;
  price: number;
  categoryName?: string;
  quantityOnHand: number;
}

export interface CartLine {
  product: Product;
  quantity: number;
}

export interface OrderResponse {
  id: string;
  status: string;
  subtotal: number;
  taxTotal: number;
  discountTotal: number;
  tipTotal: number;
  grandTotal: number;
  createdAtUtc: string;
}

export interface Category {
  id: string;
  name: string;
  productCount: number;
}

export interface AdminProduct {
  id: string;
  name: string;
  description?: string;
  sku: string;
  barcode?: string;
  price: number;
  costPrice?: number;
  categoryId?: string;
  categoryName?: string;
  isActive: boolean;
  quantityOnHand: number;
}

export interface UpsertProductRequest {
  name: string;
  description?: string;
  sku: string;
  barcode?: string;
  price: number;
  costPrice?: number;
  categoryId?: string;
  isActive: boolean;
}
