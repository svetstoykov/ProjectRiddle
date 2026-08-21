using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectRiddle.Api.Models.System;
using ProjectRiddle.Core.Interfaces.Services;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes the trivial Phase 0 walking-skeleton endpoint.
/// </summary>
[ApiController]
[Route("api/system")]
public sealed class SystemController : BaseController
{
    private readonly IWalkingSkeletonService walkingSkeletonService;

    /// <summary>
    /// Initializes the system controller.
    /// </summary>
    /// <param name="walkingSkeletonService">The Core service for the walking-skeleton operation.</param>
    public SystemController(IWalkingSkeletonService walkingSkeletonService)
    {
        ArgumentNullException.ThrowIfNull(walkingSkeletonService);

        this.walkingSkeletonService = walkingSkeletonService;
    }

    /// <summary>
    /// Returns the application readiness response or a deterministic sample failure.
    /// </summary>
    /// <param name="request">The endpoint query input.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The readiness response when the operation succeeds.</returns>
    [HttpGet("ping")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WalkingSkeletonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalkingSkeletonResponse>> PingAsync(
        [FromQuery] WalkingSkeletonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await walkingSkeletonService.ExecuteAsync(request.ToCoreInput(), cancellationToken);

        if (result.IsFailure)
        {
            return FromFailure<WalkingSkeletonResponse>(result.Error!);
        }

        return Ok(WalkingSkeletonResponse.FromCoreOutput(result.Value!));
    }
}
