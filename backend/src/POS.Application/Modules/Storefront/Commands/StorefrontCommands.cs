using MediatR;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Services;

namespace POS.Application.Modules.Storefront.Commands;

public record CustomerRegisterCommand(Guid TenantId, CustomerRegisterRequest Request, string? Ip, string? UserAgent) : IRequest<CustomerAuthResponse?>;
public record CustomerRefreshTokenCommand(string RefreshToken, string? Ip, string? UserAgent) : IRequest<CustomerAuthResponse?>;
public record CustomerLogoutCommand(string RefreshToken) : IRequest<bool>;

public class CustomerRegisterCommandHandler : IRequestHandler<CustomerRegisterCommand, CustomerAuthResponse?>
{
    private readonly CustomerAuthService _customerAuthService;
    public CustomerRegisterCommandHandler(CustomerAuthService customerAuthService) => _customerAuthService = customerAuthService;
    public Task<CustomerAuthResponse?> Handle(CustomerRegisterCommand request, CancellationToken ct) =>
        _customerAuthService.RegisterAsync(request.TenantId, request.Request, request.Ip, request.UserAgent, ct);
}

public class CustomerRefreshTokenCommandHandler : IRequestHandler<CustomerRefreshTokenCommand, CustomerAuthResponse?>
{
    private readonly CustomerSessionService _sessionService;
    public CustomerRefreshTokenCommandHandler(CustomerSessionService sessionService) => _sessionService = sessionService;
    public Task<CustomerAuthResponse?> Handle(CustomerRefreshTokenCommand request, CancellationToken ct) =>
        _sessionService.RefreshAsync(request.RefreshToken, request.Ip, request.UserAgent, ct);
}

public class CustomerLogoutCommandHandler : IRequestHandler<CustomerLogoutCommand, bool>
{
    private readonly CustomerSessionService _sessionService;
    public CustomerLogoutCommandHandler(CustomerSessionService sessionService) => _sessionService = sessionService;
    public Task<bool> Handle(CustomerLogoutCommand request, CancellationToken ct) =>
        _sessionService.RevokeAsync(request.RefreshToken, ct);
}
