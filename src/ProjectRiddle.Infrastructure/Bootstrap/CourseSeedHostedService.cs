using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Validators.Courses;
using ProjectRiddle.Infrastructure.Content;

namespace ProjectRiddle.Infrastructure.Bootstrap;

/// <summary>
/// Seeds the guided-course curriculum from the embedded manifest before the host serves traffic.
/// </summary>
/// <remarks>
/// The manifest is the only authoring surface for course content and the only writer of curriculum rows. Seeding
/// upserts by stable identifier, deactivates what the manifest no longer names, deletes nothing, and never writes,
/// updates, or removes a progress record.
/// </remarks>
public sealed class CourseSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CourseSeedHostedService> _logger;

    /// <summary>
    /// Initializes the course seeding hosted service.
    /// </summary>
    /// <param name="scopeFactory">The factory used to resolve scoped persistence services.</param>
    /// <param name="logger">The logger for safe seeding outcomes.</param>
    public CourseSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CourseSeedHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        this._scopeFactory = scopeFactory;
        this._logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="Exception">
    /// Propagates any seeding failure so the host cannot serve traffic against a curriculum it could not establish.
    /// </exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        try
        {
            var manifest = CourseManifestResource.Read();
            var curriculum = CourseManifestValidator.Validate(manifest, clock.UtcDateTime);
            if (curriculum.IsFailure)
            {
                throw new InvalidOperationException(
                    $"The shipped course manifest is invalid. {curriculum.Error!.Message}");
            }

            await repository.SeedCurriculumAsync(curriculum.Value!, cancellationToken);

            _logger.LogInformation(
                "Course curriculum seeded. Courses: {CourseCount} Lessons: {LessonCount} Exercises: {ExerciseCount}",
                curriculum.Value!.Courses.Count,
                curriculum.Value.Courses.Sum(course => course.Lessons.Count),
                curriculum.Value.LessonRiddles.Count);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogCritical(
                exception,
                "Course curriculum seeding failed; application startup is aborting.");
            throw;
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
