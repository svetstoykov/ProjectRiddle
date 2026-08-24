using System.Text.Json;
using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Models.Courses.Manifest;
using ProjectRiddle.Core.Validators.Courses;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Courses;

/// <summary>
/// Verifies that the manifest shipped with the application is valid and produces the intended progression.
/// </summary>
/// <remarks>
/// The manifest is data, not Infrastructure. It is read here as a copied content file so the test project keeps
/// its single project reference to Core.
/// </remarks>
public sealed class ShippedCurriculumTests
{
    private static readonly DateTimeOffset SeedInstant = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Verifies that the shipped manifest passes every validation rule.
    /// </summary>
    [Fact]
    public void ShippedManifestIsValid()
    {
        var result = CourseManifestValidator.Validate(Load(), SeedInstant);

        Assert.True(result.IsSuccess, result.Error?.Message);
    }

    /// <summary>
    /// Verifies that the shipped manifest holds the four approved courses, nineteen lessons, and fifty-nine
    /// exercises, in ordinal order.
    /// </summary>
    [Fact]
    public void ShippedManifestHoldsTheApprovedCurriculum()
    {
        var curriculum = CourseManifestValidator.Validate(Load(), SeedInstant).Value!;

        Assert.Equal(
            ["letterplay", "wordplay", "weirdplay", "finale"],
            curriculum.Courses.Select(course => course.Key).ToArray());
        Assert.Equal(19, curriculum.Courses.Sum(course => course.Lessons.Count));
        Assert.Equal(59, curriculum.LessonRiddles.Count);
        Assert.Equal(3, curriculum.PrimerPages.Count);

        var lessons = curriculum.Courses.SelectMany(course => course.Lessons).ToArray();
        Assert.Equal(15, lessons.Count(lesson => lesson.Kind is LessonKind.Technique));
        Assert.Equal(3, lessons.Count(lesson => lesson.Kind is LessonKind.Mix));
        Assert.Single(lessons, lesson => lesson.Kind is LessonKind.FinalMix);
    }

    /// <summary>
    /// Verifies that the shipped prerequisites gate each course mix by that course's technique lessons and the
    /// final mixed set by the three course mixes.
    /// </summary>
    [Fact]
    public void ShippedPrerequisitesGateEachMixByItsOwnCourse()
    {
        var curriculum = CourseManifestValidator.Validate(Load(), SeedInstant).Value!;

        foreach (var course in curriculum.Courses.Where(candidate => candidate.Key != "finale"))
        {
            var techniqueKeys = course.Lessons
                .Where(lesson => lesson.Kind is LessonKind.Technique)
                .Select(lesson => lesson.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            var mix = course.Lessons.Single(lesson => lesson.Kind is LessonKind.Mix);
            var mixPrerequisites = mix.Prerequisites
                .Select(prerequisite => prerequisite.LessonKey)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(techniqueKeys, mixPrerequisites);
            Assert.All(
                course.Lessons.Where(lesson => lesson.Kind is LessonKind.Technique),
                lesson => Assert.Empty(lesson.Prerequisites));
        }

        var finalMix = curriculum.Courses
            .SelectMany(course => course.Lessons)
            .Single(lesson => lesson.Kind is LessonKind.FinalMix);
        Assert.Equal(
            ["letterplay-mix", "weirdplay-mix", "wordplay-mix"],
            finalMix.Prerequisites
                .Select(prerequisite => prerequisite.LessonKey)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray());
    }

    /// <summary>
    /// Verifies end to end that the shipped progression unlocks in the intended order: every technique is open
    /// from the start, a course mix opens only when its own course is finished, and the final set opens only when
    /// all three mixes are.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ShippedProgressionUnlocksInTheIntendedOrder()
    {
        var workspace = await CourseWorkspace.CreateAsync(SeedInstant, AccountId, Load());

        var start = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(start.IsSuccess);
        Assert.All(
            start.Value!.Courses.SelectMany(course => course.Lessons)
                .Where(lesson => lesson.Kind is LessonKind.Technique),
            lesson => Assert.True(lesson.Progress!.IsAvailable));
        Assert.All(
            start.Value.Courses.SelectMany(course => course.Lessons)
                .Where(lesson => lesson.Kind is not LessonKind.Technique),
            lesson => Assert.False(lesson.Progress!.IsAvailable));

        foreach (var key in new[] { "basics", "anagrams", "selectors", "hiddens", "reversals" })
        {
            await workspace.CompleteLessonAsync(key);
        }

        var afterFirstCourse = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(afterFirstCourse.IsSuccess);
        Assert.True(Lesson(afterFirstCourse.Value!, "letterplay-mix").Progress!.IsAvailable);
        Assert.False(Lesson(afterFirstCourse.Value!, "wordplay-mix").Progress!.IsAvailable);
        Assert.False(Lesson(afterFirstCourse.Value!, "final-mix").Progress!.IsAvailable);

        foreach (var key in new[]
        {
            "letterplay-mix",
            "synonyms", "symbols", "containers", "deletions", "homophones", "wordplay-mix",
            "translations", "homoglyphs", "double-definitions", "and-lits", "rebuses"
        })
        {
            await workspace.CompleteLessonAsync(key);
        }

        var beforeFinal = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(beforeFinal.IsSuccess);
        Assert.True(Lesson(beforeFinal.Value!, "weirdplay-mix").Progress!.IsAvailable);
        Assert.False(Lesson(beforeFinal.Value!, "final-mix").Progress!.IsAvailable);

        await workspace.CompleteLessonAsync("weirdplay-mix");

        var afterEverything = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(afterEverything.IsSuccess);
        Assert.True(Lesson(afterEverything.Value!, "final-mix").Progress!.IsAvailable);
    }

    /// <summary>
    /// Verifies that no shipped clue, answer, or explanation carries Latin characters, which would mean text was
    /// pasted from the research extract rather than written for this project.
    /// </summary>
    [Fact]
    public void ShippedContentIsBulgarian()
    {
        var curriculum = CourseManifestValidator.Validate(Load(), SeedInstant).Value!;

        foreach (var riddle in curriculum.LessonRiddles)
        {
            Assert.DoesNotContain(riddle.Clue, char.IsAsciiLetter);
            Assert.DoesNotContain(riddle.Answer, char.IsAsciiLetter);
            Assert.DoesNotContain(riddle.Explanation, char.IsAsciiLetter);
        }

        foreach (var page in curriculum.PrimerPages)
        {
            Assert.DoesNotContain(page.Body, char.IsAsciiLetter);
        }
    }

    private static CourseManifest Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Content", "course-manifest.json");
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<CourseManifest>(stream, CourseManifestSerialization.Options)
            ?? throw new InvalidOperationException("The shipped course manifest deserialized to null.");
    }

    private static ProjectRiddle.Core.Models.Courses.Catalog.LessonOutput Lesson(
        ProjectRiddle.Core.Models.Courses.Catalog.CourseCatalogOutput catalog,
        string key)
    {
        return catalog.Courses.SelectMany(course => course.Lessons).Single(lesson => lesson.Key == key);
    }
}
