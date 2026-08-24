using ProjectRiddle.Core.Models.Courses.Catalog;
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
}
