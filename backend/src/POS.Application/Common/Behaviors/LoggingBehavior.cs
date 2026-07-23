using MediatR;
using Microsoft.Extensions.Logging;

namespace POS.Application.Common.Behaviors;

/// <summary>
/// One MediatR pipeline behavior wraps every command/query -- this is the concrete payoff
/// of introducing the mediator: cross-cutting concerns (logging here; validation, caching,
/// authorization, or transaction-wrapping could each be added the same way) live in ONE
/// place instead of being copy-pasted into every controller action or service method.
/// Registered once in DependencyInjection.cs and it applies to all ~25 requests across
/// every module automatically.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{RequestName} failed", requestName);
            throw;
        }
    }
}
