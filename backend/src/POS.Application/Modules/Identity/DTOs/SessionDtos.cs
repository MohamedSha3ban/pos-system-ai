namespace POS.Application.Modules.Identity.DTOs;

public record RefreshTokenRequest(string RefreshToken);

public record SessionDto(
    Guid Id,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    string? CreatedByIp,
    string? UserAgent,
    bool IsCurrent);
