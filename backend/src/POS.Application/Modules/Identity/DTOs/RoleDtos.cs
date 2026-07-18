namespace POS.Application.Modules.Identity.DTOs;

public record RoleDto(Guid Id, string Name, bool IsSystemRole, List<string> Permissions);

public record UpsertRoleRequest(string Name, List<string> Permissions);

public record PermissionDto(string Code);
