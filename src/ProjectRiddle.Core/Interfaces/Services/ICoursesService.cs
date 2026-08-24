using ProjectRiddle.Core.Models.Courses.Catalog;
using ProjectRiddle.Core.Models.Courses.Play;
using ProjectRiddle.Core.Models.Courses.Progress;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides guided-course discovery, practice, and account completion operations.
/// </summary>
public interface ICoursesService
{
    /// <summary>
    /// Gets the active curriculum, with completion and derived availability when the caller is signed in.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The catalog, or an expected failure.</returns>
    Task<Result<CourseCatalogOutput>> GetCatalogAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a lesson's teaching prose and its ordered safe exercise projections.
    /// </summary>
    /// <param name="lessonId">The lesson identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The lesson, or an expected failure.</returns>
    /// <remarks>
    /// A lesson returns its exercises whether or not it is available. The lock is presentation, not an
    /// authorization boundary; answer-sensitive content is gated separately by the terminal-state rule.
    /// </remarks>
    Task<Result<LessonDetailOutput>> GetLessonAsync(Guid lessonId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the ordered primer pages.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The primer, or an expected failure.</returns>
    Task<Result<CoursePrimerOutput>> GetPrimerAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks a submitted answer for a lesson exercise and updates progress.
    /// </summary>
    /// <param name="input">The answer input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<CoursePlayStateOutput>> SubmitAnswerAsync(
        SubmitCourseAnswerInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records one structural hint kind on a lesson exercise.
    /// </summary>
    /// <param name="input">The hint input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<CoursePlayStateOutput>> UseHintAsync(UseCourseHintInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Reveals one previously unrevealed letter of a lesson exercise.
    /// </summary>
    /// <param name="input">The reveal input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<CoursePlayStateOutput>> RevealLetterAsync(
        RevealCourseLetterInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rehydrates permitted play state for a lesson exercise.
    /// </summary>
    /// <param name="input">The resume input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<CoursePlayStateOutput>> ResumeAsync(
        ResumeCourseExerciseInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current account's course completion.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account's completion, or an expected failure.</returns>
    Task<Result<AccountCourseProgressOutput>> GetProgressAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Merges a bounded anonymous completion snapshot into the current account's progress.
    /// </summary>
    /// <param name="input">The imported snapshot. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account's completion after the merge, or an expected failure.</returns>
    /// <remarks>
    /// The whole payload is validated and resolved before the first write, so an invalid entry rejects the import
    /// and leaves stored progress untouched.
    /// </remarks>
    Task<Result<AccountCourseProgressOutput>> ImportProgressAsync(
        AnonymousCourseProgressInput input,
        CancellationToken cancellationToken);
}
