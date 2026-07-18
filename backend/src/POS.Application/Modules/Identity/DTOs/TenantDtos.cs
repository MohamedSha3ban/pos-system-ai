namespace POS.Application.Modules.Identity.DTOs;

/// <summary>Platform-admin view of a tenant -- see TenantService (platform scope, not tenant-scoped).</summary>
public record TenantSummaryDto(
    Guid Id,
    string BusinessName,
    string BusinessType,
    bool IsActive,
    DateTime CreatedAtUtc,
    int UserCount,
    int ProductCount);
