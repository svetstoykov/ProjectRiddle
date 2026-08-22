using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Api.Models.Riddles.Authoring;
using ProjectRiddle.Core.Interfaces.Services;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes administrative riddle authoring and publication operations.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.Admin)]
[Route("api/admin/riddles")]
public sealed class AdminRiddlesController : BaseController
{
    private readonly IAdminRiddlesService _adminRiddlesService;

    /// <summary>
    /// Initializes the administrative riddles controller.
    /// </summary>
    /// <param name="adminRiddlesService">The Core administrative riddles service.</param>
    public AdminRiddlesController(IAdminRiddlesService adminRiddlesService)
    {
        ArgumentNullException.ThrowIfNull(adminRiddlesService);
        this._adminRiddlesService = adminRiddlesService;
    }

    /// <summary>
    /// Lists every riddle for administration.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The administrative riddle list.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(RiddleListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RiddleListResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await _adminRiddlesService.ListAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleListResponse>(result.Error!);
        }

        return Ok(RiddleListResponse.FromCoreListRiddlesOutput(result.Value!));
    }

    /// <summary>
    /// Gets one riddle for administration.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The administrative riddle projection.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RiddleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiddleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRiddlesService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleResponse>(result.Error!);
        }

        return Ok(RiddleResponse.FromCoreRiddleOutput(result.Value!));
    }

    /// <summary>
    /// Creates a draft riddle.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The created riddle.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RiddleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RiddleResponse>> CreateAsync(
        [FromBody] CreateRiddleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _adminRiddlesService.CreateAsync(request.ToCoreCreateRiddleInput(), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleResponse>(result.Error!);
        }

        var response = RiddleResponse.FromCoreRiddleOutput(result.Value!);
        return Created($"/api/admin/riddles/{response.Id}", response);
    }

    /// <summary>
    /// Schedules a riddle onto a Sofia calendar date.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The schedule request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The scheduled riddle.</returns>
    [HttpPost("{id:guid}/schedule")]
    [ProducesResponseType(typeof(RiddleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RiddleResponse>> ScheduleAsync(
        Guid id,
        [FromBody] ScheduleRiddleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _adminRiddlesService.ScheduleAsync(request.ToCoreScheduleRiddleInput(id), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleResponse>(result.Error!);
        }

        return Ok(RiddleResponse.FromCoreRiddleOutput(result.Value!));
    }

    /// <summary>
    /// Publishes a riddle onto a Sofia calendar date.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The optional publish request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The published riddle.</returns>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(RiddleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RiddleResponse>> PublishAsync(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PublishRiddleRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new PublishRiddleRequest();

        var result = await _adminRiddlesService.PublishAsync(request.ToCorePublishRiddleInput(id), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleResponse>(result.Error!);
        }

        return Ok(RiddleResponse.FromCoreRiddleOutput(result.Value!));
    }

    /// <summary>
    /// Unpublishes a scheduled or published riddle.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The unpublished riddle.</returns>
    [HttpPost("{id:guid}/unpublish")]
    [ProducesResponseType(typeof(RiddleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RiddleResponse>> UnpublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRiddlesService.UnpublishAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleResponse>(result.Error!);
        }

        return Ok(RiddleResponse.FromCoreRiddleOutput(result.Value!));
    }

    /// <summary>
    /// Deletes a draft or unpublished riddle.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>A bodyless success response.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRiddlesService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure(result.Error!);
        }

        return NoContent();
    }
}
