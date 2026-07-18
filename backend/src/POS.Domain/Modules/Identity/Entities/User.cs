using POS.Domain.Common;

namespace POS.Domain.Modules.Identity.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Grants cross-tenant access to the platform admin portal's Tenants screen.
    /// Not settable via any API in this starter -- flip it directly in the DB for the
    /// first platform operator (see README "Seeding a platform admin").
    /// </summary>
    public bool IsPlatformAdmin { get; set; } = false;
}
