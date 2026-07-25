using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using POS.Application.Modules.Identity.Interfaces;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Infrastructure.Modules.Identity;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IEnumerable<string> roleNames, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenantId", user.TenantId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("actorType", "staff"),
            new("isPlatformAdmin", user.IsPlatformAdmin.ToString().ToLowerInvariant())
        };
        claims.AddRange(roleNames.Select(r => new Claim("role", r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        return Write(claims, TokenLifetimes.StaffAccessToken);
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateCustomerAccessToken(Customer customer, Guid tenantId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new("tenantId", tenantId.ToString()),
            new(ClaimTypes.Email, customer.Email ?? string.Empty),
            new("actorType", "customer")
        };

        return Write(claims, TokenLifetimes.CustomerAccessToken);
    }

    private (string Token, DateTime ExpiresAtUtc) Write(List<Claim> claims, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.Add(lifetime);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
