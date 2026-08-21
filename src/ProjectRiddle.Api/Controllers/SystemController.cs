using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectRiddle.Api.Models.System;
using ProjectRiddle.Core.Interfaces.Services;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes the internal application health endpoint.
/// </summary>
[ApiController]
[Route("api/system")]
public sealed class SystemController : BaseController
{
    private readonly IInternalStatusService _internalStatusService;

    /// <summary>
    /// Initializes the system controller.
    /// </summary>
    /// <param name="internalStatusService">The Core service for the internal application status.</param>
    public SystemController(IInternalStatusService internalStatusService)
    {
        ArgumentNullException.ThrowIfNull(internalStatusService);

        this._internalStatusService = internalStatusService;
    }

    /// <summary>
    /// Gets the current internal application health status.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The current health status when the operation succeeds.</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InternalStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InternalStatusResponse>> GetHealthAsync(
        CancellationToken cancellationToken)
    {
        var result = await _internalStatusService.GetAsync(cancellationToken);

        if (result.IsFailure)
        {
            return FromFailure<InternalStatusResponse>(result.Error!);
        }

        return Ok(InternalStatusResponse.FromCoreOutput(result.Value!));
    }
}
