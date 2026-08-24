using Microsoft.Extensions.Logging.Abstractions;
using ProjectRiddle.Core.Interfaces.Randomness;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Courses.Manifest;
using ProjectRiddle.Core.Services.Courses;
using ProjectRiddle.Core.Services.Play;
using ProjectRiddle.Core.Validators.Courses;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Owns the Core course collaborators for one domain test.
/// </summary>
public sealed class CourseWorkspace
{
    /// <summary>
    /// Identifies the application time zone used by tests.
    /// </summary>
    public const string TimeZoneId = "Europe/Sofia";

    private readonly InMemoryCourseRepository _courses;
    private readonly InMemoryRiddleRepository _riddles;

    private CourseWorkspace(
        FixedDateTimeProvider clock,
        MutableCurrentAccount account,
        InMemoryCourseRepository courses,
        InMemoryRiddleRepository riddles,
        InMemoryRiddleProgressRepository progress,
        ICluePlayEngine playEngine,
        ICoursesService service)
    {
        Clock = clock;
        Account = account;
        Progress = progress;
        PlayEngine = playEngine;
        Service = service;
        _courses = courses;
        _riddles = riddles;
    }

    /// <summary>
    /// Gets the controllable clock.
    /// </summary>
    public FixedDateTimeProvider Clock { get; }

    /// <summary>
    /// Gets the controllable current-account identity.
    /// </summary>
    public MutableCurrentAccount Account { get; }

    /// <summary>
    /// Gets the in-memory progress store.
    /// </summary>
    public InMemoryRiddleProgressRepository Progress { get; }

    /// <summary>
    /// Gets the shared clue play engine.
    /// </summary>
    public ICluePlayEngine PlayEngine { get; }

    /// <summary>
    /// Gets the Core courses service under test.
    /// </summary>
    public ICoursesService Service { get; }

    /// <summary>
    /// Creates a workspace seeded with the supplied manifest.
    /// </summary>
    /// <param name="utcNow">The fixed UTC instant.</param>
    /// <param name="accountId">The current account identifier, or <see langword="null" /> for an anonymous caller.</param>
    /// <param name="manifest">The manifest to seed, or <see langword="null" /> to use the complete fixture.</param>
    /// <param name="randomNumberGenerator">The optional scripted random source for letter reveals.</param>
    /// <returns>A seeded workspace.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the supplied manifest does not validate.</exception>
    public static async Task<CourseWorkspace> CreateAsync(
        DateTimeOffset utcNow,
        Guid? accountId = null,
        CourseManifest? manifest = null,
        IRandomNumberGenerator? randomNumberGenerator = null)
    {
        var clock = new FixedDateTimeProvider(utcNow, TimeZoneId);
        var account = new MutableCurrentAccount(accountId);
        var courses = new InMemoryCourseRepository();
        var riddles = new InMemoryRiddleRepository();
        var progress = new InMemoryRiddleProgressRepository(riddles);
        var playEngine = new CluePlayEngine(
            progress,
            account,
            clock,
            randomNumberGenerator ?? new ScriptedRandomNumberGenerator());
        var service = new CoursesService(
            courses,
            riddles,
            progress,
            playEngine,
            account,
            NullLogger<CoursesService>.Instance);

        var validated = CourseManifestValidator.Validate(manifest ?? CourseManifestBuilder.Complete(), utcNow);
        if (validated.IsFailure)
        {
            throw new InvalidOperationException($"The test manifest is invalid: {validated.Error!.Message}");
        }

        await courses.SeedCurriculumAsync(validated.Value!, CancellationToken.None);
        foreach (var riddle in validated.Value!.LessonRiddles)
        {
            await riddles.AddAsync(riddle, CancellationToken.None);
        }

        return new CourseWorkspace(clock, account, courses, riddles, progress, playEngine, service);
    }

    /// <summary>
    /// Gets the identifier of the lesson with the supplied key.
    /// </summary>
    /// <param name="lessonKey">The lesson key.</param>
    /// <returns>The lesson identifier.</returns>
    public Guid LessonId(string lessonKey)
    {
        return _courses.LessonByKey(lessonKey).Id;
    }

    /// <summary>
    /// Gets the identifier of one exercise within a lesson.
    /// </summary>
    /// <param name="lessonKey">The lesson key.</param>
    /// <param name="ordinal">The one-based exercise ordinal.</param>
    /// <returns>The exercise identifier.</returns>
    public Guid ExerciseId(string lessonKey, int ordinal)
    {
        return _courses.LessonByKey(lessonKey).Exercises.Single(exercise => exercise.Ordinal == ordinal).Id;
    }

    /// <summary>
    /// Solves one exercise by submitting its authored answer through the shared engine.
    /// </summary>
    /// <param name="lessonKey">The lesson key.</param>
    /// <param name="ordinal">The one-based exercise ordinal.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task SolveExerciseAsync(string lessonKey, int ordinal)
    {
        var exercise = _courses.LessonByKey(lessonKey).Exercises.Single(candidate => candidate.Ordinal == ordinal);
        var riddle = await _riddles.GetByIdAsync(exercise.RiddleId, CancellationToken.None);
        var result = await PlayEngine.SubmitAnswerAsync(riddle!, riddle!.Answer, null, CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Finishes one exercise by revealing every letter through the shared engine.
    /// </summary>
    /// <param name="lessonKey">The lesson key.</param>
    /// <param name="ordinal">The one-based exercise ordinal.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task FullyRevealExerciseAsync(string lessonKey, int ordinal)
    {
        var exercise = _courses.LessonByKey(lessonKey).Exercises.Single(candidate => candidate.Ordinal == ordinal);
        var riddle = await _riddles.GetByIdAsync(exercise.RiddleId, CancellationToken.None);
        var letterCount = riddle!.Answer.Count(char.IsLetter);
        for (var reveal = 0; reveal < letterCount; reveal++)
        {
            var result = await PlayEngine.RevealLetterAsync(riddle, null, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
    }

    /// <summary>
    /// Solves every exercise in a lesson.
    /// </summary>
    /// <param name="lessonKey">The lesson key.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task CompleteLessonAsync(string lessonKey)
    {
        foreach (var exercise in _courses.LessonByKey(lessonKey).Exercises.OrderBy(candidate => candidate.Ordinal))
        {
            await SolveExerciseAsync(lessonKey, exercise.Ordinal);
        }
    }
}
