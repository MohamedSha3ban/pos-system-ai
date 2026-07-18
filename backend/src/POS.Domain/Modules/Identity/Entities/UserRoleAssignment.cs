using POS.Domain.Common;

namespace POS.Domain.Modules.Identity.Entities;

public class UserRoleAssignment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
