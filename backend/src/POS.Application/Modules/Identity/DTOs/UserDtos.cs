namespace POS.Application.Modules.Identity.DTOs;

public record UserDto(Guid Id, string FullName, string Email, bool IsActive, List<RoleSummary> Roles);

public record RoleSummary(Guid Id, string Name);

public record CreateUserRequest(string FullName, string Email, string Password, List<Guid> RoleIds);

public record UpdateUserRequest(string FullName, bool IsActive, List<Guid> RoleIds);
