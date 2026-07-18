using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Modules.Identity.Interfaces;

public interface ITokenService
{
    /// <summary>Staff token: includes flattened permission claims from the user's assigned roles.</summary>
    string GenerateToken(User user, IEnumerable<string> roleNames, IEnumerable<string> permissions);

    /// <summary>Customer (storefront) token: no permissions, scoped only to that tenant + customer id.</summary>
    string GenerateCustomerToken(Customer customer, Guid tenantId);
}
