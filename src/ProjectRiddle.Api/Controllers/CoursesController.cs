using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProjectRiddle.Api.Models.Courses.Catalog;
using ProjectRiddle.Api.Models.Courses.Play;
using ProjectRiddle.Api.Models.Courses.Progress;
using ProjectRiddle.Core.Interfaces.Services;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes guided-course discovery, practice, and account completion operations.
/// </summary>
/// <remarks>
/// Exercises are addressed by exercise identifier throughout. The riddle behind a lesson clue is never part of
/// this contract.
/// </remarks>
[ApiController]
[Route("api/courses")]
public sealed class CoursesController : BaseController
{
    private readonly ICoursesService _coursesService;

    /// <summary>
    /// Initializes the courses controller.
    /// </summary>
    /// <param name="coursesService">The Core courses service.</param>
    public CoursesController(ICoursesService coursesService)
    {
        ArgumentNullException.ThrowIfNull(coursesService);
        this._coursesService = coursesService;
    }

    /// <summary>
    /// Gets the active curriculum with prerequisites, plus completion and availability when signed in.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The catalog.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseCatalogResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseCatalogResponse>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var result = await _coursesService.GetCatalogAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CourseCatalogResponse>(result.Error!);
        }

        return Ok(CourseCatalogResponse.FromCoreCourseCatalogOutput(result.Value!));
    }

    /// <summary>
    /// Gets the ordered primer pages.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The primer.</returns>
    [HttpGet("primer")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CoursePrimerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoursePrimerResponse>> GetPrimerAsync(CancellationToken cancellationToken)
    {
        var result = await _coursesService.GetPrimerAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CoursePrimerResponse>(result.Error!);
        }

        return Ok(CoursePrimerResponse.FromCoreCoursePrimerOutput(result.Value!));
    }

    /// <summary>
    /// Gets a lesson's teaching prose and its ordered safe exercise projections.
    /// </summary>
    /// <param name="lessonId">The lesson identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The lesson.</returns>
    [HttpGet("lessons/{lessonId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LessonDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LessonDetailResponse>> GetLessonAsync(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _coursesService.GetLessonAsync(lessonId, cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<LessonDetailResponse>(result.Error!);
        }

        return Ok(LessonDetailResponse.FromCoreLessonDetailOutput(result.Value!));
    }

    /// <summary>
    /// Checks a submitted answer for a lesson exercise.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="request">The answer request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("exercises/{exerciseId:guid}/answer")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CoursePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoursePlayStateResponse>> SubmitAnswerAsync(
        Guid exerciseId,
        [FromBody] SubmitCourseAnswerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _coursesService.SubmitAnswerAsync(
            request.ToCoreSubmitCourseAnswerInput(exerciseId),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CoursePlayStateResponse>(result.Error!);
        }

        return Ok(CoursePlayStateResponse.FromCoreCoursePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Records one structural hint kind on a lesson exercise.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="request">The hint request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("exercises/{exerciseId:guid}/hint")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CoursePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoursePlayStateResponse>> UseHintAsync(
        Guid exerciseId,
        [FromBody] UseCourseHintRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _coursesService.UseHintAsync(
            request.ToCoreUseCourseHintInput(exerciseId),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CoursePlayStateResponse>(result.Error!);
        }

        return Ok(CoursePlayStateResponse.FromCoreCoursePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Reveals one previously unrevealed letter of a lesson exercise.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="request">The optional reveal request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("exercises/{exerciseId:guid}/reveal")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CoursePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoursePlayStateResponse>> RevealLetterAsync(
        Guid exerciseId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RevealCourseLetterRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new RevealCourseLetterRequest();

        var result = await _coursesService.RevealLetterAsync(
            request.ToCoreRevealCourseLetterInput(exerciseId),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CoursePlayStateResponse>(result.Error!);
        }

        return Ok(CoursePlayStateResponse.FromCoreCoursePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Rehydrates permitted play state for a lesson exercise.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="request">The optional resume request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The resulting play state.</returns>
    [HttpPost("exercises/{exerciseId:guid}/resume")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CoursePlayStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoursePlayStateResponse>> ResumeAsync(
        Guid exerciseId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ResumeCourseExerciseRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new ResumeCourseExerciseRequest();

        var result = await _coursesService.ResumeAsync(
            request.ToCoreResumeCourseExerciseInput(exerciseId),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<CoursePlayStateResponse>(result.Error!);
        }

        return Ok(CoursePlayStateResponse.FromCoreCoursePlayStateOutput(result.Value!));
    }

    /// <summary>
    /// Gets the current account's course completion.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The account's completion.</returns>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(AccountCourseProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountCourseProgressResponse>> GetProgressAsync(
        CancellationToken cancellationToken)
    {
        var result = await _coursesService.GetProgressAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<AccountCourseProgressResponse>(result.Error!);
        }

        return Ok(AccountCourseProgressResponse.FromCoreAccountCourseProgressOutput(result.Value!));
    }

    /// <summary>
    /// Merges a bounded anonymous completion snapshot into the current account's progress.
    /// </summary>
    /// <param name="request">The imported snapshot.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The account's completion after the merge.</returns>
    [HttpPost("progress/import")]
    [ProducesResponseType(typeof(AccountCourseProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountCourseProgressResponse>> ImportProgressAsync(
        [FromBody] ImportCourseProgressRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _coursesService.ImportProgressAsync(
            request.ToCoreAnonymousCourseProgressInput(),
            cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<AccountCourseProgressResponse>(result.Error!);
        }

        return Ok(AccountCourseProgressResponse.FromCoreAccountCourseProgressOutput(result.Value!));
    }
}
