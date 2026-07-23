using MediatR;
using POS.Application.Modules.Insights.Interfaces;

namespace POS.Application.Modules.Insights.Queries;

public record GetReorderSuggestionsQuery(Guid TenantId, Guid LocationId) : IRequest<List<DemandForecast>>;

public class GetReorderSuggestionsQueryHandler : IRequestHandler<GetReorderSuggestionsQuery, List<DemandForecast>>
{
    private readonly IForecastingService _forecastingService;
    public GetReorderSuggestionsQueryHandler(IForecastingService forecastingService) => _forecastingService = forecastingService;
    public Task<List<DemandForecast>> Handle(GetReorderSuggestionsQuery request, CancellationToken ct) =>
        _forecastingService.GetLowStockForecastsAsync(request.TenantId, request.LocationId, ct);
}
