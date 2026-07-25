export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  fullName: string;
  tenantId: string;
  isPlatformAdmin: boolean;
  roleNames: string[];
  permissions: string[];
}

export interface SessionInfo {
  id: string;
  createdAtUtc: string;
  expiresAtUtc: string;
  createdByIp?: string;
  userAgent?: string;
  isCurrent: boolean;
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
  channel: string;
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

// --- RBAC: Users & Roles ---

export interface RoleSummary {
  id: string;
  name: string;
}

export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  isActive: boolean;
  roles: RoleSummary[];
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  roleIds: string[];
}

export interface UpdateUserRequest {
  fullName: string;
  isActive: boolean;
  roleIds: string[];
}

export interface Role {
  id: string;
  name: string;
  isSystemRole: boolean;
  permissions: string[];
}

export interface UpsertRoleRequest {
  name: string;
  permissions: string[];
}

// --- Platform admin: Tenants ---

export interface TenantSummary {
  id: string;
  businessName: string;
  businessType: string;
  isActive: boolean;
  createdAtUtc: string;
  userCount: number;
  productCount: number;
}

// --- Inventory ---

export interface InventoryItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  locationId: string;
  locationName: string;
  quantityOnHand: number;
  reorderPoint: number;
  reorderQuantity: number;
  isLow: boolean;
}

export interface AdjustInventoryRequest {
  quantityOnHand: number;
  reorderPoint?: number;
  reorderQuantity?: number;
}
