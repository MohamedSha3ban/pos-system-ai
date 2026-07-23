using MediatR;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Services;

namespace POS.Application.Modules.Identity.Commands;

public record RegisterTenantCommand(RegisterTenantRequest Request) : IRequest<AuthResponse>;
public record CreateUserCommand(Guid TenantId, CreateUserRequest Request) : IRequest<UserDto>;
public record UpdateUserCommand(Guid TenantId, Guid UserId, UpdateUserRequest Request) : IRequest<UserDto?>;
public record DeactivateUserCommand(Guid TenantId, Guid UserId) : IRequest<bool>;
public record CreateRoleCommand(Guid TenantId, UpsertRoleRequest Request) : IRequest<RoleDto>;
public record UpdateRoleCommand(Guid TenantId, Guid RoleId, UpsertRoleRequest Request) : IRequest<RoleDto?>;
public record DeleteRoleCommand(Guid TenantId, Guid RoleId) : IRequest<(bool Success, string? Error)>;
public record SetTenantActiveCommand(Guid TenantId, bool IsActive) : IRequest<bool>;

// Handlers are thin by design -- all business logic already lives in the Services
// (and is already split across IWriteDbContext/IReadDbContext appropriately). The
// mediator's job here is purely to decouple controllers from concrete service types and
// give every request a place to pass through the shared pipeline (see LoggingBehavior).

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, AuthResponse>
{
    private readonly AuthService _authService;
    public RegisterTenantCommandHandler(AuthService authService) => _authService = authService;
    public Task<AuthResponse> Handle(RegisterTenantCommand request, CancellationToken ct) =>
        _authService.RegisterTenantAsync(request.Request, ct);
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly UserService _userService;
    public CreateUserCommandHandler(UserService userService) => _userService = userService;
    public Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct) =>
        _userService.CreateAsync(request.TenantId, request.Request, ct);
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly UserService _userService;
    public UpdateUserCommandHandler(UserService userService) => _userService = userService;
    public Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken ct) =>
        _userService.UpdateAsync(request.TenantId, request.UserId, request.Request, ct);
}

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly UserService _userService;
    public DeactivateUserCommandHandler(UserService userService) => _userService = userService;
    public Task<bool> Handle(DeactivateUserCommand request, CancellationToken ct) =>
        _userService.DeactivateAsync(request.TenantId, request.UserId, ct);
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly RoleService _roleService;
    public CreateRoleCommandHandler(RoleService roleService) => _roleService = roleService;
    public Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken ct) =>
        _roleService.CreateAsync(request.TenantId, request.Request, ct);
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto?>
{
    private readonly RoleService _roleService;
    public UpdateRoleCommandHandler(RoleService roleService) => _roleService = roleService;
    public Task<RoleDto?> Handle(UpdateRoleCommand request, CancellationToken ct) =>
        _roleService.UpdateAsync(request.TenantId, request.RoleId, request.Request, ct);
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, (bool Success, string? Error)>
{
    private readonly RoleService _roleService;
    public DeleteRoleCommandHandler(RoleService roleService) => _roleService = roleService;
    public Task<(bool Success, string? Error)> Handle(DeleteRoleCommand request, CancellationToken ct) =>
        _roleService.DeleteAsync(request.TenantId, request.RoleId, ct);
}

public class SetTenantActiveCommandHandler : IRequestHandler<SetTenantActiveCommand, bool>
{
    private readonly TenantService _tenantService;
    public SetTenantActiveCommandHandler(TenantService tenantService) => _tenantService = tenantService;
    public Task<bool> Handle(SetTenantActiveCommand request, CancellationToken ct) =>
        _tenantService.SetActiveAsync(request.TenantId, request.IsActive, ct);
}
