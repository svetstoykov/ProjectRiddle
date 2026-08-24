using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;
using ProjectRiddle.Core.Validators.Courses;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Courses;

/// <summary>
/// Verifies that the shipped-manifest contract is enforced before any curriculum row is written.
/// </summary>
public sealed class CourseManifestValidatorTests
{
    private static readonly DateTimeOffset SeedInstant = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Verifies that a complete manifest projects into the full hierarchy in stable order with every reference
    /// resolving to a riddle that carries the authored clue.
    /// </summary>
    [Fact]
    public void CompleteManifestProjectsTheFullHierarchy()
    {
        var result = CourseManifestValidator.Validate(CourseManifestBuilder.Complete(), SeedInstant);

        Assert.True(result.IsSuccess);
        var curriculum = result.Value!;

        Assert.Equal(2, curriculum.Courses.Count);
        Assert.Equal([1, 2], curriculum.Courses.Select(course => course.Ordinal).ToArray());
        Assert.Equal(["letterplay", "finale"], curriculum.Courses.Select(course => course.Key).ToArray());

        var letterplay = curriculum.Courses[0];
        Assert.Equal(["anagrams", "hiddens", "letterplay-mix"], letterplay.Lessons.Select(lesson => lesson.Key).ToArray());
        Assert.Equal([1, 2, 3], letterplay.Lessons.Select(lesson => lesson.Ordinal).ToArray());
        Assert.All(letterplay.Lessons, lesson => Assert.True(lesson.IsActive));
        Assert.All(letterplay.Lessons, lesson => Assert.NotEmpty(lesson.Exercises));

        var mix = letterplay.Lessons.Single(lesson => lesson.Kind is LessonKind.Mix);
        Assert.Equal(
            ["anagrams", "hiddens"],
            mix.Prerequisites.Select(prerequisite => prerequisite.LessonKey).ToArray());

        var finalMix = curriculum.Courses[1].Lessons.Single();
        Assert.Equal(LessonKind.FinalMix, finalMix.Kind);
        Assert.Equal(["letterplay-mix"], finalMix.Prerequisites.Select(prerequisite => prerequisite.LessonKey).ToArray());

        var riddleIds = curriculum.LessonRiddles.Select(riddle => riddle.Id).ToHashSet();
        var referenced = curriculum.Courses
            .SelectMany(course => course.Lessons)
            .SelectMany(lesson => lesson.Exercises)
            .Select(exercise => exercise.RiddleId)
            .ToArray();
        Assert.All(referenced, riddleId => Assert.Contains(riddleId, riddleIds));
        Assert.All(curriculum.LessonRiddles, riddle => Assert.True(riddle.IsLesson));
        Assert.All(curriculum.LessonRiddles, riddle => Assert.Null(riddle.SofiaPublicationDate));
        Assert.All(
            curriculum.LessonRiddles,
            riddle => Assert.Equal(RiddlePublicationState.Draft, riddle.PublicationState));

        Assert.Equal([1, 2, 3], curriculum.PrimerPages.Select(page => page.Ordinal).ToArray());
        Assert.Equal("clue-anatomy", curriculum.PrimerPages[1].Figure);
    }

    /// <summary>
    /// Verifies that the answer pattern is derived from the authored answer rather than authored separately.
    /// </summary>
    [Fact]
    public void AnswerPatternIsDerivedFromTheAuthoredAnswer()
    {
        var result = CourseManifestValidator.Validate(CourseManifestBuilder.Complete(), SeedInstant);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value!.LessonRiddles, riddle => Assert.Equal("бяла врана", riddle.Answer));
        Assert.All(result.Value.LessonRiddles, riddle => Assert.Equal("4,5", riddle.AnswerPattern));
    }

    /// <summary>
    /// Verifies that a duplicate identifier of any kind is rejected and that the message names the offender.
    /// </summary>
    [Fact]
    public void DuplicateIdentifiersAreRejected()
    {
        var duplicateExercise = CourseManifestBuilder.Complete();
        var shared = duplicateExercise.Courses![0].Lessons![0].Exercises![0].Id;
        var manifest = CourseManifestBuilder.WithExerciseId(duplicateExercise, "hiddens", 1, shared);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains(shared.ToString(), result.Error!.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a duplicate lesson key is rejected across courses, not only within one.
    /// </summary>
    [Fact]
    public void LessonKeysAreUniqueAcrossTheWholeManifest()
    {
        var manifest = CourseManifestBuilder.WithLessonKey(CourseManifestBuilder.Complete(), "final-mix", "anagrams");

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("anagrams", result.Error!.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a course without a mixed set is rejected.
    /// </summary>
    [Fact]
    public void EveryOrdinaryCourseNeedsExactlyOneMix()
    {
        var manifest = CourseManifestBuilder.WithLessonKind(
            CourseManifestBuilder.Complete(),
            "letterplay-mix",
            LessonKind.Technique);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("letterplay", result.Error!.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a second final mixed set is rejected.
    /// </summary>
    [Fact]
    public void OnlyOneFinalMixMayExist()
    {
        var manifest = CourseManifestBuilder.WithLessonKind(
            CourseManifestBuilder.Complete(),
            "letterplay-mix",
            LessonKind.FinalMix);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Verifies that ordinals must be contiguous and start at one.
    /// </summary>
    [Fact]
    public void NonContiguousOrdinalsAreRejected()
    {
        var manifest = CourseManifestBuilder.WithLessonOrdinal(CourseManifestBuilder.Complete(), "hiddens", 7);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("letterplay", result.Error!.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a prerequisite must resolve, must not be the lesson itself, and must not repeat.
    /// </summary>
    [Fact]
    public void PrerequisitesMustResolveAndStayDistinct()
    {
        var unresolved = CourseManifestBuilder.WithPrerequisites(
            CourseManifestBuilder.Complete(),
            "letterplay-mix",
            ["anagrams", "does-not-exist"]);
        var unresolvedResult = CourseManifestValidator.Validate(unresolved, SeedInstant);
        Assert.True(unresolvedResult.IsFailure);
        Assert.Contains("does-not-exist", unresolvedResult.Error!.Message, StringComparison.Ordinal);

        var selfReferencing = CourseManifestBuilder.WithPrerequisites(
            CourseManifestBuilder.Complete(),
            "letterplay-mix",
            ["letterplay-mix"]);
        Assert.True(CourseManifestValidator.Validate(selfReferencing, SeedInstant).IsFailure);

        var repeated = CourseManifestBuilder.WithPrerequisites(
            CourseManifestBuilder.Complete(),
            "letterplay-mix",
            ["anagrams", "anagrams"]);
        Assert.True(CourseManifestValidator.Validate(repeated, SeedInstant).IsFailure);
    }

    /// <summary>
    /// Verifies that a prerequisite chain closing into a cycle is rejected, so no lesson can gate itself.
    /// </summary>
    [Fact]
    public void CyclicPrerequisitesAreRejected()
    {
        var manifest = CourseManifestBuilder.WithPrerequisites(
            CourseManifestBuilder.WithPrerequisites(CourseManifestBuilder.Complete(), "anagrams", ["hiddens"]),
            "hiddens",
            ["anagrams"]);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("cycle", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that a lesson without exercises is rejected.
    /// </summary>
    [Fact]
    public void EveryLessonNeedsAtLeastOneExercise()
    {
        var manifest = CourseManifestBuilder.WithoutExercises(CourseManifestBuilder.Complete(), "hiddens");

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("hiddens", result.Error!.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that an answer the letter grid cannot express is rejected while it is being authored.
    /// </summary>
    [Fact]
    public void AnswersMustSatisfyTheAuthoredFormat()
    {
        var manifest = CourseManifestBuilder.WithAnswer(CourseManifestBuilder.Complete(), "anagrams", 1, "бяла-врана");

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Verifies that a structural range reaching past the end of its clue is rejected.
    /// </summary>
    [Fact]
    public void StructuralRangesMustFallInsideTheirClue()
    {
        var manifest = CourseManifestBuilder.WithRanges(
            CourseManifestBuilder.Complete(),
            "anagrams",
            1,
            [new RiddleRangeInput(RiddleRangeKind.Definition, 0, 500)]);

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Verifies that clue and explanation text is required.
    /// </summary>
    [Fact]
    public void ClueAndExplanationAreRequired()
    {
        Assert.True(
            CourseManifestValidator.Validate(
                CourseManifestBuilder.WithClue(CourseManifestBuilder.Complete(), "anagrams", 1, "   "),
                SeedInstant).IsFailure);
        Assert.True(
            CourseManifestValidator.Validate(
                CourseManifestBuilder.WithExplanation(CourseManifestBuilder.Complete(), "anagrams", 1, ""),
                SeedInstant).IsFailure);
    }

    /// <summary>
    /// Verifies that an unsupported schema version is rejected before anything else is inspected.
    /// </summary>
    [Fact]
    public void UnsupportedSchemaVersionsAreRejected()
    {
        var manifest = CourseManifestBuilder.Complete() with { SchemaVersion = 99 };

        var result = CourseManifestValidator.Validate(manifest, SeedInstant);

        Assert.True(result.IsFailure);
        Assert.Contains("99", result.Error!.Message, StringComparison.Ordinal);
    }
}
