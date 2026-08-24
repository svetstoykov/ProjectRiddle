using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Courses;

/// <summary>
/// Verifies that lesson availability is a containment check over authored prerequisites and completion facts.
/// </summary>
public sealed class CourseAvailabilityTests
{
    private static readonly DateTimeOffset NoonUtc = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Verifies that an anonymous caller receives the catalog and its prerequisite keys but no completion or
    /// availability, which is what lets the client derive locks for itself.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnonymousCatalogCarriesPrerequisitesWithoutProgress()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);

        var catalog = await workspace.Service.GetCatalogAsync(CancellationToken.None);

        Assert.True(catalog.IsSuccess);
        Assert.Equal(["letterplay", "finale"], catalog.Value!.Courses.Select(course => course.Key).ToArray());
        Assert.All(
            catalog.Value.Courses.SelectMany(course => course.Lessons),
            lesson => Assert.Null(lesson.Progress));

        var mix = catalog.Value.Courses[0].Lessons.Single(lesson => lesson.Kind is LessonKind.Mix);
        Assert.Equal(["anagrams", "hiddens"], mix.PrerequisiteLessonKeys);
        Assert.Equal(1, mix.ExerciseCount);
    }

    /// <summary>
    /// Verifies that a lesson with no prerequisites is always available and that a gated lesson becomes available
    /// exactly when every prerequisite in its own list is complete, and not one completion before.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AvailabilityFollowsTheAuthoredPrerequisiteList()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);

        var initial = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(initial.IsSuccess);
        Assert.True(Lesson(initial.Value!, "anagrams").Progress!.IsAvailable);
        Assert.True(Lesson(initial.Value!, "hiddens").Progress!.IsAvailable);
        Assert.False(Lesson(initial.Value!, "letterplay-mix").Progress!.IsAvailable);
        Assert.False(Lesson(initial.Value!, "final-mix").Progress!.IsAvailable);

        await workspace.CompleteLessonAsync("anagrams");
        var partial = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(partial.IsSuccess);
        Assert.False(Lesson(partial.Value!, "letterplay-mix").Progress!.IsAvailable);

        await workspace.CompleteLessonAsync("hiddens");
        var unlocked = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(unlocked.IsSuccess);
        Assert.True(Lesson(unlocked.Value!, "letterplay-mix").Progress!.IsAvailable);
        Assert.False(Lesson(unlocked.Value!, "final-mix").Progress!.IsAvailable);

        await workspace.CompleteLessonAsync("letterplay-mix");
        var finished = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(finished.IsSuccess);
        Assert.True(Lesson(finished.Value!, "final-mix").Progress!.IsAvailable);
    }

    /// <summary>
    /// Verifies that a lesson counter advances one exercise at a time and that the lesson only counts as complete
    /// when every active exercise beneath it is complete.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LessonCountersFollowExerciseCompletion()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);

        await workspace.SolveExerciseAsync("anagrams", 1);

        var catalog = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(catalog.IsSuccess);
        var anagrams = Lesson(catalog.Value!, "anagrams");
        Assert.Equal(2, anagrams.ExerciseCount);
        Assert.Equal(1, anagrams.Progress!.CompletedExerciseCount);
        Assert.Single(anagrams.Progress.CompletedExerciseIds);
        Assert.False(Lesson(catalog.Value!, "letterplay-mix").Progress!.IsAvailable);
    }

    /// <summary>
    /// Verifies that an exercise finished by revealing every letter counts toward completion, because the courses
    /// teach rather than grade.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task FullyRevealingAnExerciseCompletesIt()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);

        await workspace.FullyRevealExerciseAsync("anagrams", 1);
        await workspace.FullyRevealExerciseAsync("anagrams", 2);

        var catalog = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(catalog.IsSuccess);
        Assert.Equal(2, Lesson(catalog.Value!, "anagrams").Progress!.CompletedExerciseCount);
    }

    /// <summary>
    /// Verifies that the lesson read carries teaching prose, prerequisite keys, and its exercises in order for any
    /// caller, and that a missing lesson is a non-disclosing miss.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LessonReadsCarryOrderedExercisesForEveryCaller()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var lessonId = workspace.LessonId("letterplay-mix");

        var lesson = await workspace.Service.GetLessonAsync(lessonId, CancellationToken.None);

        Assert.True(lesson.IsSuccess);
        Assert.Equal("letterplay-mix", lesson.Value!.Key);
        Assert.Equal(LessonKind.Mix, lesson.Value.Kind);
        Assert.Equal(["anagrams", "hiddens"], lesson.Value.PrerequisiteLessonKeys);
        Assert.Equal([1], lesson.Value.Exercises.Select(exercise => exercise.Ordinal).ToArray());
        Assert.All(lesson.Value.Exercises, exercise => Assert.Null(exercise.IsComplete));

        var missing = await workspace.Service.GetLessonAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.True(missing.IsFailure);
        Assert.Equal(CourseErrorCodes.LessonNotFound, missing.Error!.Code);
    }

    /// <summary>
    /// Verifies that a locked lesson still returns its exercises, because availability is reported as data rather
    /// than raised as a failure.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LockedLessonsStillReturnTheirExercises()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);

        var lesson = await workspace.Service.GetLessonAsync(
            workspace.LessonId("final-mix"),
            CancellationToken.None);

        Assert.True(lesson.IsSuccess);
        Assert.NotEmpty(lesson.Value!.Exercises);
    }

    /// <summary>
    /// Verifies that the primer is served in page order for an anonymous caller.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task PrimerIsServedInPageOrder()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);

        var primer = await workspace.Service.GetPrimerAsync(CancellationToken.None);

        Assert.True(primer.IsSuccess);
        Assert.Equal([1, 2, 3], primer.Value!.Pages.Select(page => page.Ordinal).ToArray());
        Assert.Equal("clue-anatomy", primer.Value.Pages[1].Figure);
    }

    private static ProjectRiddle.Core.Models.Courses.Catalog.LessonOutput Lesson(
        ProjectRiddle.Core.Models.Courses.Catalog.CourseCatalogOutput catalog,
        string key)
    {
        return catalog.Courses.SelectMany(course => course.Lessons).Single(lesson => lesson.Key == key);
    }
}
