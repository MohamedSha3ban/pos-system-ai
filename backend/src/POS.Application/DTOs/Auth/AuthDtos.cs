namespace POS.Application.DTOs.Auth;

public record RegisterTenantRequest(
    string BusinessName,
    string BusinessType,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPassword);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string FullName, string Role, Guid TenantId);
