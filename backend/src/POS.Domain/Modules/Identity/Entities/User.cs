using POS.Domain.Common;
using POS.Domain.Modules.Identity.Enums;

namespace POS.Domain.Modules.Identity.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Cashier;
    public bool IsActive { get; set; } = true;
}
