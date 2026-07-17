using POS.Domain.Common;

namespace POS.Domain.Modules.Catalog.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
