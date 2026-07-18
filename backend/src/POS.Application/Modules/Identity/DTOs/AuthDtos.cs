namespace POS.Application.Modules.Identity.DTOs;

public record RegisterTenantRequest(
    string BusinessName,
    string BusinessType,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPassword);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    string FullName,
    Guid TenantId,
    bool IsPlatformAdmin,
    List<string> RoleNames,
    List<string> Permissions);
