using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Domain.Common;

namespace POS.API.Controllers.Identity;

/// <summary>Read-only catalog of assignable permission codes, for populating the Roles editor UI.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<string>> GetAll() => Ok(Permissions.TenantAssignable.ToList());
}
