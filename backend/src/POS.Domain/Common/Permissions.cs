namespace POS.Domain.Common;

/// <summary>
/// Static permission catalog. Deliberately not a DB table -- these codes are fixed by
/// the application's capabilities, not something tenants define themselves. Roles store
/// which of these codes they grant (see Role.PermissionsCsv); the admin portal's
/// Roles & Permissions screen lets a tenant compose custom roles from this fixed list.
/// </summary>
public static class Permissions
{
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string TenantsManage = "tenants.manage"; // platform-admin only, gated separately
    public const string ProductsManage = "products.manage";
    public const string CategoriesManage = "categories.manage";
    public const string InventoryManage = "inventory.manage";
    public const string OrdersView = "orders.view";
    public const string OrdersCheckout = "orders.checkout";

    public static readonly string[] All =
    {
        UsersManage, RolesManage, TenantsManage, ProductsManage,
        CategoriesManage, InventoryManage, OrdersView, OrdersCheckout
    };

    /// <summary>Permissions assignable to *tenant* roles (excludes the platform-only TenantsManage).</summary>
    public static readonly string[] TenantAssignable =
        All.Where(p => p != TenantsManage).ToArray();
}
