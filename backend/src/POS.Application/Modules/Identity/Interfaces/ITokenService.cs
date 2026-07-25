using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Modules.Identity.Interfaces;

public interface ITokenService
{
    /// <summary>Staff access token: short-lived JWT, includes flattened permission claims
    /// from the user's assigned roles. See TokenLifetimes.StaffAccessToken.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IEnumerable<string> roleNames, IEnumerable<string> permissions);

    /// <summary>Customer (storefront) access token: short-lived JWT, no permissions, scoped
    /// only to that tenant + customer id. See TokenLifetimes.CustomerAccessToken.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateCustomerAccessToken(Customer customer, Guid tenantId);
}
