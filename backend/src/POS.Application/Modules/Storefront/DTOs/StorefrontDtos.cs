namespace POS.Application.Modules.Storefront.DTOs;

public record CustomerRegisterRequest(string FullName, string Email, string Password, string? Phone);
public record CustomerLoginRequest(string Email, string Password);
public record CustomerAuthResponse(string Token, DateTime ExpiresAtUtc, string FullName, Guid TenantId);
