namespace POS.Application.Modules.Storefront.DTOs;

public record CustomerRegisterRequest(string FullName, string Email, string Password, string? Phone);
public record CustomerLoginRequest(string Email, string Password);

public record CustomerAuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string FullName,
    Guid TenantId);

public record CustomerRefreshTokenRequest(string RefreshToken);
