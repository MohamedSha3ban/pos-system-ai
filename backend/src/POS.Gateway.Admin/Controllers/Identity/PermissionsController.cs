using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Identity.Queries;

namespace POS.Gateway.Admin.Controllers.Identity;

/// <summary>Read-only catalog of assignable permission codes, for populating the Roles editor UI.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PermissionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAvailablePermissionsQuery(), ct));
}
