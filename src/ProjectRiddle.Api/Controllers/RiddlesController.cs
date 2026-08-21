using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProjectRiddle.Api.Models.Riddles;
using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes public riddle discovery, play, and account progress operations.
/// </summary>
[ApiController]
[Route("api/riddles")]
public sealed class RiddlesController : BaseController
{
    private readonly IPublicRiddlesService _publicRiddlesService;

    /// <summary>
    /// Initializes the public riddles controller.
    /// </summary>
    /// <param name="publicRiddlesService">The Core public riddles service.</param>
    public RiddlesController(IPublicRiddlesService publicRiddlesService)
    {
        ArgumentNullException.ThrowIfNull(publicRiddlesService);
        this._publicRiddlesService = publicRiddlesService;
    }

    /// <summary>
    /// Lists a page of safe archive metadata.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The archive page.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicRiddleListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicRiddleListResponse>> ListArchiveAsync(
        [FromQuery] int page = PublicRiddleLimits.DefaultPage,
        [FromQuery] int pageSize = PublicRiddleLimits.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _publicRiddlesService.ListArchiveAsync(
            new ListPublicRiddlesInput(page, pageSize),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<PublicRiddleListResponse>(result.Error!);
        }

        return Ok(PublicRiddleListResponse.FromCorePublicRiddleListOutput(result.Value!));
    }

    /// <summary>
    /// Gets today's eligible public play projection.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>Today's play projection.</returns>
    [HttpGet("today")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicRiddlePlayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicRiddlePlayResponse>> GetTodayAsync(CancellationToken cancellationToken)
    {
        var result = await _publicRiddlesService.GetTodayAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<PublicRiddlePlayResponse>(result.Error!);
        }

        return Ok(PublicRiddlePlayResponse.FromCorePublicRiddlePlayOutput(result.Value!));
    }

    /// <summary>
    /// Lists safe metadata for published riddles in the current local week through today.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The week discovery items.</returns>
    [HttpGet("week")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicRiddleWeekResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicRiddleWeekResponse>> ListWeekAsync(CancellationToken cancellationToken)
    {
        var result = await _publicRiddlesService.ListWeekAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<PublicRiddleWeekResponse>(result.Error!);
        }

        return Ok(PublicRiddleWeekResponse.FromCoreWeekItems(result.Value!));
    }

    /// <summary>
    /// Gets the initial play projection for a public riddle.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The play projection.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicRiddlePlayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicRiddlePlayResponse>> GetPlayAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _publicRiddlesService.GetPlayAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<PublicRiddlePlayResponse>(result.Error!);
        }

        return Ok(PublicRiddlePlayResponse.FromCorePublicRiddlePlayOutput(result.Value!));
    }

    /// <summary>
    /// Checks a submitted answer and updates progress.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The answer request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("{id:guid}/answer")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RiddlePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiddlePlayStateResponse>> SubmitAnswerAsync(
        Guid id,
        [FromBody] SubmitRiddleAnswerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _publicRiddlesService.SubmitAnswerAsync(
            request.ToCoreSubmitRiddleAnswerInput(id),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddlePlayStateResponse>(result.Error!);
        }

        return Ok(RiddlePlayStateResponse.FromCoreRiddlePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Records one structural hint kind on progress.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The hint request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("{id:guid}/hint")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RiddlePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiddlePlayStateResponse>> UseHintAsync(
        Guid id,
        [FromBody] UseRiddleHintRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _publicRiddlesService.UseHintAsync(request.ToCoreUseRiddleHintInput(id), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddlePlayStateResponse>(result.Error!);
        }

        return Ok(RiddlePlayStateResponse.FromCoreRiddlePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Reveals one previously unrevealed letter.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The optional reveal request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("{id:guid}/reveal")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RiddlePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiddlePlayStateResponse>> RevealLetterAsync(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RevealRiddleLetterRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new RevealRiddleLetterRequest();

        var result = await _publicRiddlesService.RevealLetterAsync(
            request.ToCoreRevealRiddleLetterInput(id),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddlePlayStateResponse>(result.Error!);
        }

        return Ok(RiddlePlayStateResponse.FromCoreRiddlePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Rehydrates permitted play state from anonymous or account progress.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="request">The optional resume request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("{id:guid}/resume")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RiddlePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiddlePlayStateResponse>> ResumeAsync(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ResumeRiddleRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new ResumeRiddleRequest();

        var result = await _publicRiddlesService.ResumeAsync(request.ToCoreResumeRiddleInput(id), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddlePlayStateResponse>(result.Error!);
        }

        return Ok(RiddlePlayStateResponse.FromCoreRiddlePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Lists account-owned riddle progress for a bounded local-date range.
    /// </summary>
    /// <param name="fromDate">The inclusive start local date.</param>
    /// <param name="toDate">The inclusive end local date.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The account progress list.</returns>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(AccountRiddleProgressListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountRiddleProgressListResponse>> ListProgressAsync(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken cancellationToken)
    {
        var result = await _publicRiddlesService.ListProgressAsync(
            new ListAccountRiddleProgressInput(fromDate, toDate),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<AccountRiddleProgressListResponse>(result.Error!);
        }

        return Ok(AccountRiddleProgressListResponse.FromCoreAccountRiddleProgressListOutput(result.Value!));
    }

    /// <summary>
    /// Merges a typed anonymous progress snapshot into the current account record.
    /// </summary>
    /// <param name="request">The imported snapshot.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The merged progress snapshot.</returns>
    [HttpPost("progress/import")]
    [ProducesResponseType(typeof(RiddleProgressSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RiddleProgressSnapshotResponse>> ImportProgressAsync(
        [FromBody] AnonymousRiddleProgressRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _publicRiddlesService.ImportProgressAsync(
            request.ToCoreAnonymousRiddleProgressInput(),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<RiddleProgressSnapshotResponse>(result.Error!);
        }

        return Ok(RiddleProgressSnapshotResponse.FromCoreRiddleProgressSnapshotOutput(result.Value!));
    }
}
