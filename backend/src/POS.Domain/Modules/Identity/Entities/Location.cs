using POS.Domain.Common;

namespace POS.Domain.Modules.Identity.Entities;

public class Location : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
