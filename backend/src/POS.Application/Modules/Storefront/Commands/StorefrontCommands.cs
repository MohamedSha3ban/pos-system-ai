using MediatR;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Services;

namespace POS.Application.Modules.Storefront.Commands;

public record CustomerRegisterCommand(Guid TenantId, CustomerRegisterRequest Request) : IRequest<CustomerAuthResponse?>;

public class CustomerRegisterCommandHandler : IRequestHandler<CustomerRegisterCommand, CustomerAuthResponse?>
{
    private readonly CustomerAuthService _customerAuthService;
    public CustomerRegisterCommandHandler(CustomerAuthService customerAuthService) => _customerAuthService = customerAuthService;
    public Task<CustomerAuthResponse?> Handle(CustomerRegisterCommand request, CancellationToken ct) =>
        _customerAuthService.RegisterAsync(request.TenantId, request.Request, ct);
}
