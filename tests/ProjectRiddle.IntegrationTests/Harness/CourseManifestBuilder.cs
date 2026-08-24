using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Manifest;
using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Builds valid course manifests and single-rule mutations of them for validator and service tests.
/// </summary>
/// <remarks>
/// The shape here is deliberately not the shipped curriculum's. Availability is a containment check over an
/// authored graph, so the tests exercise an arbitrary graph rather than encoding the progression that ships.
/// </remarks>
public static class CourseManifestBuilder
{
    /// <summary>
    /// Builds a manifest satisfying every validation rule: one ordinary course with two techniques and a mix,
    /// and a second course holding only the final mixed set.
    /// </summary>
    /// <returns>A complete, valid manifest.</returns>
    public static CourseManifest Complete()
    {
        return new CourseManifest(
            1,
            [
                new PrimerPageManifestEntry(1, "Двете части", "Определение и словесна игра.", null),
                new PrimerPageManifestEntry(2, "Съставките", "Материал и индикатори.", "clue-anatomy"),
                new PrimerPageManifestEntry(3, "Повърхността", "Как уликата заблуждава.", null)
            ],
            [
                new CourseManifestEntry(
                    Id("c1"),
                    "letterplay",
                    1,
                    "Буквена игра",
                    "Първите техники.",
                    [
                        Technique("anagrams", 1, "Анаграми", "l1"),
                        Technique("hiddens", 2, "Скрити думи", "l2"),
                        new LessonManifestEntry(
                            Id("l3"),
                            "letterplay-mix",
                            3,
                            LessonKind.Mix,
                            "Смесени улики",
                            null,
                            ["anagrams", "hiddens"],
                            [Exercise("e3a", "r3a", 1)])
                    ]),
                new CourseManifestEntry(
                    Id("c2"),
                    "finale",
                    2,
                    "Голямото смесване",
                    "Всичко наведнъж.",
                    [
                        new LessonManifestEntry(
                            Id("l4"),
                            "final-mix",
                            1,
                            LessonKind.FinalMix,
                            "Финален смесен набор",
                            null,
                            ["letterplay-mix"],
                            [Exercise("e4a", "r4a", 1)])
                    ])
            ]);
    }

    /// <summary>
    /// Replaces one exercise's identifier.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson holding the exercise.</param>
    /// <param name="ordinal">The exercise ordinal.</param>
    /// <param name="id">The replacement identifier.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithExerciseId(CourseManifest manifest, string lessonKey, int ordinal, Guid id)
    {
        return MapExercise(manifest, lessonKey, ordinal, exercise => exercise with { Id = id });
    }

    /// <summary>
    /// Replaces one exercise's clue text.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson holding the exercise.</param>
    /// <param name="ordinal">The exercise ordinal.</param>
    /// <param name="clue">The replacement clue.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithClue(CourseManifest manifest, string lessonKey, int ordinal, string clue)
    {
        return MapExercise(manifest, lessonKey, ordinal, exercise => exercise with { Clue = clue });
    }

    /// <summary>
    /// Replaces one exercise's answer.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson holding the exercise.</param>
    /// <param name="ordinal">The exercise ordinal.</param>
    /// <param name="answer">The replacement answer.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithAnswer(CourseManifest manifest, string lessonKey, int ordinal, string answer)
    {
        return MapExercise(manifest, lessonKey, ordinal, exercise => exercise with { Answer = answer });
    }

    /// <summary>
    /// Replaces one exercise's explanation.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson holding the exercise.</param>
    /// <param name="ordinal">The exercise ordinal.</param>
    /// <param name="explanation">The replacement explanation.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithExplanation(
        CourseManifest manifest,
        string lessonKey,
        int ordinal,
        string explanation)
    {
        return MapExercise(manifest, lessonKey, ordinal, exercise => exercise with { Explanation = explanation });
    }

    /// <summary>
    /// Replaces one exercise's structural ranges.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson holding the exercise.</param>
    /// <param name="ordinal">The exercise ordinal.</param>
    /// <param name="ranges">The replacement ranges.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithRanges(
        CourseManifest manifest,
        string lessonKey,
        int ordinal,
        IReadOnlyList<RiddleRangeInput> ranges)
    {
        return MapExercise(manifest, lessonKey, ordinal, exercise => exercise with { Ranges = ranges });
    }

    /// <summary>
    /// Replaces one lesson's key.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson to change.</param>
    /// <param name="replacementKey">The replacement key.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithLessonKey(CourseManifest manifest, string lessonKey, string replacementKey)
    {
        return MapLesson(manifest, lessonKey, lesson => lesson with { Key = replacementKey });
    }

    /// <summary>
    /// Replaces one lesson's kind.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson to change.</param>
    /// <param name="kind">The replacement kind.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithLessonKind(CourseManifest manifest, string lessonKey, LessonKind kind)
    {
        return MapLesson(manifest, lessonKey, lesson => lesson with { Kind = kind });
    }

    /// <summary>
    /// Replaces one lesson's ordinal.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson to change.</param>
    /// <param name="ordinal">The replacement ordinal.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithLessonOrdinal(CourseManifest manifest, string lessonKey, int ordinal)
    {
        return MapLesson(manifest, lessonKey, lesson => lesson with { Ordinal = ordinal });
    }

    /// <summary>
    /// Replaces one lesson's prerequisite list.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson to change.</param>
    /// <param name="prerequisiteLessonKeys">The replacement prerequisite keys.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithPrerequisites(
        CourseManifest manifest,
        string lessonKey,
        IReadOnlyList<string> prerequisiteLessonKeys)
    {
        return MapLesson(manifest, lessonKey, lesson => lesson with { PrerequisiteLessonKeys = prerequisiteLessonKeys });
    }

    /// <summary>
    /// Empties one lesson's exercise list.
    /// </summary>
    /// <param name="manifest">The manifest to copy. Cannot be <see langword="null" />.</param>
    /// <param name="lessonKey">The lesson to change.</param>
    /// <returns>A copy with the change applied.</returns>
    public static CourseManifest WithoutExercises(CourseManifest manifest, string lessonKey)
    {
        return MapLesson(manifest, lessonKey, lesson => lesson with { Exercises = [] });
    }

    /// <summary>
    /// Produces a stable identifier from a short label so a fixture reads the same on every run.
    /// </summary>
    /// <param name="label">The label. Cannot be <see langword="null" /> or whitespace.</param>
    /// <returns>A deterministic identifier for the label.</returns>
    public static Guid Id(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        var bytes = new byte[16];
        var source = System.Text.Encoding.UTF8.GetBytes(label);
        Array.Copy(source, bytes, Math.Min(source.Length, bytes.Length));
        bytes[15] = (byte)(bytes[15] == 0 ? 1 : bytes[15]);
        return new Guid(bytes);
    }

    private static LessonManifestEntry Technique(string key, int ordinal, string title, string idLabel)
    {
        return new LessonManifestEntry(
            Id(idLabel),
            key,
            ordinal,
            LessonKind.Technique,
            title,
            "Кратко въведение в техниката.",
            [],
            [Exercise($"{idLabel}e1", $"{idLabel}r1", 1), Exercise($"{idLabel}e2", $"{idLabel}r2", 2)]);
    }

    private static LessonExerciseManifestEntry Exercise(string idLabel, string riddleLabel, int ordinal)
    {
        return new LessonExerciseManifestEntry(
            Id(idLabel),
            Id(riddleLabel),
            ordinal,
            "Потърси индикатора.",
            "Индикаторът беше в началото.",
            "бяла врана лети високо",
            "бяла врана",
            "Обяснение на уликата.",
            [
                new RiddleRangeInput(RiddleRangeKind.Definition, 0, 4),
                new RiddleRangeInput(RiddleRangeKind.Fodder, 5, 10)
            ]);
    }

    private static CourseManifest MapLesson(
        CourseManifest manifest,
        string lessonKey,
        Func<LessonManifestEntry, LessonManifestEntry> map)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var courses = manifest.Courses!
            .Select(course => course with
            {
                Lessons = course.Lessons!
                    .Select(lesson => lesson.Key == lessonKey ? map(lesson) : lesson)
                    .ToArray()
            })
            .ToArray();
        return manifest with { Courses = courses };
    }

    private static CourseManifest MapExercise(
        CourseManifest manifest,
        string lessonKey,
        int ordinal,
        Func<LessonExerciseManifestEntry, LessonExerciseManifestEntry> map)
    {
        return MapLesson(
            manifest,
            lessonKey,
            lesson => lesson with
            {
                Exercises = lesson.Exercises!
                    .Select(exercise => exercise.Ordinal == ordinal ? map(exercise) : exercise)
                    .ToArray()
            });
    }
}
