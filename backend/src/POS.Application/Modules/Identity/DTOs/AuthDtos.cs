namespace POS.Application.Modules.Identity.DTOs;

public record RegisterTenantRequest(
    string BusinessName,
    string BusinessType,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPassword);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string FullName,
    Guid TenantId,
    bool IsPlatformAdmin,
    List<string> RoleNames,
    List<string> Permissions);
