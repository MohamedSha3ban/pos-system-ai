using MediatR;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Services;

namespace POS.Application.Modules.Identity.Queries;

public record LoginQuery(LoginRequest Request) : IRequest<AuthResponse?>;
public record GetUsersQuery(Guid TenantId) : IRequest<List<UserDto>>;
public record GetRolesQuery(Guid TenantId) : IRequest<List<RoleDto>>;
public record GetAvailablePermissionsQuery : IRequest<List<string>>;
public record GetTenantsQuery : IRequest<List<TenantSummaryDto>>;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse?>
{
    private readonly AuthService _authService;
    public LoginQueryHandler(AuthService authService) => _authService = authService;
    public Task<AuthResponse?> Handle(LoginQuery request, CancellationToken ct) =>
        _authService.LoginAsync(request.Request, ct);
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly UserService _userService;
    public GetUsersQueryHandler(UserService userService) => _userService = userService;
    public Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken ct) =>
        _userService.GetAllAsync(request.TenantId, ct);
}

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly RoleService _roleService;
    public GetRolesQueryHandler(RoleService roleService) => _roleService = roleService;
    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken ct) =>
        _roleService.GetAllAsync(request.TenantId, ct);
}

public class GetAvailablePermissionsQueryHandler : IRequestHandler<GetAvailablePermissionsQuery, List<string>>
{
    private readonly RoleService _roleService;
    public GetAvailablePermissionsQueryHandler(RoleService roleService) => _roleService = roleService;
    public Task<List<string>> Handle(GetAvailablePermissionsQuery request, CancellationToken ct) =>
        Task.FromResult(_roleService.GetAvailablePermissions());
}

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, List<TenantSummaryDto>>
{
    private readonly TenantService _tenantService;
    public GetTenantsQueryHandler(TenantService tenantService) => _tenantService = tenantService;
    public Task<List<TenantSummaryDto>> Handle(GetTenantsQuery request, CancellationToken ct) =>
        _tenantService.GetAllAsync(ct);
}
