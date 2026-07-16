using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Auth;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;

namespace POS.Application.Services;

public class AuthService
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterTenantAsync(RegisterTenantRequest request, CancellationToken ct = default)
    {
        var tenant = new Tenant
        {
            BusinessName = request.BusinessName,
            BusinessType = request.BusinessType
        };
        _db.Tenants.Add(tenant);

        var defaultLocation = new Location
        {
            TenantId = tenant.Id,
            Name = "Main Location"
        };
        _db.Locations.Add(defaultLocation);

        var owner = new User
        {
            TenantId = tenant.Id,
            FullName = request.OwnerFullName,
            Email = request.OwnerEmail.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
            Role = UserRole.Owner
        };
        _db.Users.Add(owner);

        await _db.SaveChangesAsync(ct);

        var token = _tokenService.GenerateToken(owner);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), owner.FullName, owner.Role.ToString(), tenant.Id);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), user.FullName, user.Role.ToString(), user.TenantId);
    }
}
