using MediatR;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Services;

namespace POS.Application.Modules.Storefront.Queries;

public record CustomerLoginQuery(Guid TenantId, CustomerLoginRequest Request, string? Ip, string? UserAgent) : IRequest<CustomerAuthResponse?>;

public class CustomerLoginQueryHandler : IRequestHandler<CustomerLoginQuery, CustomerAuthResponse?>
{
    private readonly CustomerAuthService _customerAuthService;
    public CustomerLoginQueryHandler(CustomerAuthService customerAuthService) => _customerAuthService = customerAuthService;
    public Task<CustomerAuthResponse?> Handle(CustomerLoginQuery request, CancellationToken ct) =>
        _customerAuthService.LoginAsync(request.TenantId, request.Request, request.Ip, request.UserAgent, ct);
}
