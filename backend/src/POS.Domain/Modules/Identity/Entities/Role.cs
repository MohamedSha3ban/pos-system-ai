using POS.Domain.Common;

namespace POS.Domain.Modules.Identity.Entities;

/// <summary>
/// A tenant-scoped, tenant-customizable set of permissions (see Domain.Common.Permissions
/// for the fixed code list). Every tenant gets three seeded roles on signup -- Owner,
/// Manager, Cashier -- which can be edited or supplemented with custom roles.
/// Permissions are stored as a comma-separated list rather than a join table: the
/// permission catalog is small, fixed, and read far more often than written, so a CSV
/// column keeps the schema simpler without a real downside here.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = default!;
    public bool IsSystemRole { get; set; } = false; // seeded roles -- protected from deletion
    public string PermissionsCsv { get; set; } = string.Empty;
}
