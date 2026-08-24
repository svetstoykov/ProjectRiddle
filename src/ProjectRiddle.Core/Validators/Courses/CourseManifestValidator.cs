using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses;
using ProjectRiddle.Core.Models.Courses.Manifest;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Validators.Riddles;

namespace ProjectRiddle.Core.Validators.Courses;

/// <summary>
/// Validates the authored course manifest and projects it into the curriculum domain model.
/// </summary>
/// <remarks>
/// The manifest is untrusted input even though it ships with the application. Validating it before persistence keeps
/// malformed content from creating orphaned lesson riddles, broken prerequisite graphs, or unreachable curriculum
/// rows.
/// </remarks>
public static class CourseManifestValidator
{
    private const int SupportedSchemaVersion = 1;

    /// <summary>
    /// Validates and projects an authored course manifest.
    /// </summary>
    /// <param name="manifest">The authored manifest. Cannot be <see langword="null" />.</param>
    /// <param name="utcNow">The UTC timestamp assigned to newly projected lesson riddles.</param>
    /// <returns>A complete curriculum when the manifest satisfies every structural and content rule.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest" /> is <see langword="null" />.</exception>
    public static Result<CourseCurriculum> Validate(CourseManifest manifest, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        if (manifest.SchemaVersion != SupportedSchemaVersion)
        {
            return Invalid<CourseCurriculum>(
                $"The course manifest schema version {manifest.SchemaVersion} is not supported.");
        }

        var primerResult = ValidatePrimer(manifest.Primer);
        if (primerResult.IsFailure)
        {
            return Result.Failure<CourseCurriculum>(primerResult.Error!);
        }

        if (manifest.Courses is not { Count: > 0 })
        {
            return Invalid<CourseCurriculum>("The course manifest must contain at least one course.");
        }

        var registry = new IdentifierRegistry();
        var lessonEntriesByKey = new Dictionary<string, LessonManifestEntry>(StringComparer.Ordinal);
        var courses = new List<Course>();
        var riddles = new List<Riddle>();

        foreach (var courseEntry in manifest.Courses.OrderBy(course => course.Ordinal))
        {
            var courseResult = BuildCourse(
                courseEntry,
                registry,
                lessonEntriesByKey,
                riddles,
                utcNow);
            if (courseResult.IsFailure)
            {
                return Result.Failure<CourseCurriculum>(courseResult.Error!);
            }

            courses.Add(courseResult.Value!);
        }

        var shapeResult = ValidateCurriculumShape(courses);
        if (shapeResult.IsFailure)
        {
            return Result.Failure<CourseCurriculum>(shapeResult.Error!);
        }

        var graphResult = ValidatePrerequisiteGraph(lessonEntriesByKey);
        if (graphResult.IsFailure)
        {
            return Result.Failure<CourseCurriculum>(graphResult.Error!);
        }

        return Result.Success<CourseCurriculum>(
            new CourseCurriculum(courses, riddles, primerResult.Value!));
    }

    private static Result<Course> BuildCourse(
        CourseManifestEntry entry,
        IdentifierRegistry registry,
        Dictionary<string, LessonManifestEntry> lessonEntriesByKey,
        ICollection<Riddle> riddles,
        DateTimeOffset utcNow)
    {
        if (entry.Id == Guid.Empty)
        {
            return Invalid<Course>("Course identifiers cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(entry.Key))
        {
            return Invalid<Course>("Course keys are required.");
        }

        if (string.IsNullOrWhiteSpace(entry.Title))
        {
            return Invalid<Course>($"Course '{entry.Key}' must have a title.");
        }

        if (string.IsNullOrWhiteSpace(entry.Intro))
        {
            return Invalid<Course>($"Course '{entry.Key}' must have introductory prose.");
        }

        if (!registry.CourseIds.Add(entry.Id))
        {
            return Invalid<Course>($"Course identifier '{entry.Id}' is duplicated.");
        }

        if (!registry.CourseKeys.Add(entry.Key))
        {
            return Invalid<Course>($"Course key '{entry.Key}' is duplicated.");
        }

        if (entry.Lessons is not { Count: > 0 })
        {
            return Invalid<Course>($"Course '{entry.Key}' must contain at least one lesson.");
        }

        var ordinalResult = ValidateOrdinals(entry.Lessons.Select(lesson => lesson.Ordinal), $"course '{entry.Key}' lessons");
        if (ordinalResult.IsFailure)
        {
            return Result.Failure<Course>(ordinalResult.Error!);
        }

        var course = new Course(entry.Id, entry.Key, entry.Ordinal, entry.Title, entry.Intro, isActive: true);
        var lessons = new List<Lesson>();

        foreach (var lessonEntry in entry.Lessons.OrderBy(lesson => lesson.Ordinal))
        {
            var lessonResult = BuildLesson(lessonEntry, registry, riddles, utcNow);
            if (lessonResult.IsFailure)
            {
                return Result.Failure<Course>(lessonResult.Error!);
            }

            lessons.Add(lessonResult.Value!);
            lessonEntriesByKey.Add(lessonEntry.Key!, lessonEntry);
        }

        course.ReplaceLessons(lessons);
        return Result.Success(course);
    }

    private static Result<Lesson> BuildLesson(
        LessonManifestEntry entry,
        IdentifierRegistry registry,
        ICollection<Riddle> riddles,
        DateTimeOffset utcNow)
    {
        if (entry.Id == Guid.Empty)
        {
            return Invalid<Lesson>("Lesson identifiers cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(entry.Key))
        {
            return Invalid<Lesson>("Lesson keys are required.");
        }

        if (string.IsNullOrWhiteSpace(entry.Title))
        {
            return Invalid<Lesson>($"Lesson '{entry.Key}' must have a title.");
        }

        if (!Enum.IsDefined(entry.Kind))
        {
            return Invalid<Lesson>($"Lesson '{entry.Key}' uses an unsupported lesson kind.");
        }

        if (!registry.LessonIds.Add(entry.Id))
        {
            return Invalid<Lesson>($"Lesson identifier '{entry.Id}' is duplicated.");
        }

        if (!registry.LessonKeys.Add(entry.Key))
        {
            return Invalid<Lesson>($"Lesson key '{entry.Key}' is duplicated.");
        }

        if (entry.Exercises is not { Count: > 0 })
        {
            return Invalid<Lesson>($"Lesson '{entry.Key}' must contain at least one exercise.");
        }

        var ordinalResult = ValidateOrdinals(entry.Exercises.Select(exercise => exercise.Ordinal), $"lesson '{entry.Key}' exercises");
        if (ordinalResult.IsFailure)
        {
            return Result.Failure<Lesson>(ordinalResult.Error!);
        }

        var prerequisiteKeys = entry.PrerequisiteLessonKeys ?? [];
        var distinctPrerequisiteKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var prerequisiteKey in prerequisiteKeys)
        {
            if (string.IsNullOrWhiteSpace(prerequisiteKey))
            {
                return Invalid<Lesson>($"Lesson '{entry.Key}' has a blank prerequisite key.");
            }

            if (!distinctPrerequisiteKeys.Add(prerequisiteKey))
            {
                return Invalid<Lesson>($"Lesson '{entry.Key}' repeats prerequisite '{prerequisiteKey}'.");
            }

            if (string.Equals(prerequisiteKey, entry.Key, StringComparison.Ordinal))
            {
                return Invalid<Lesson>($"Lesson '{entry.Key}' cannot depend on itself.");
            }
        }

        var lesson = new Lesson(
            entry.Id,
            entry.Key,
            entry.Ordinal,
            entry.Title,
            entry.Intro,
            entry.Kind,
            isActive: true);
        lesson.ReplacePrerequisites(prerequisiteKeys.Select(key => new LessonPrerequisite(key)).ToArray());

        var exercises = new List<LessonExercise>();
        foreach (var exerciseEntry in entry.Exercises.OrderBy(exercise => exercise.Ordinal))
        {
            var exerciseResult = BuildExercise(exerciseEntry, registry, riddles, utcNow);
            if (exerciseResult.IsFailure)
            {
                return Result.Failure<Lesson>(exerciseResult.Error!);
            }

            exercises.Add(exerciseResult.Value!);
        }

        lesson.ReplaceExercises(exercises);
        return Result.Success(lesson);
    }

    private static Result<LessonExercise> BuildExercise(
        LessonExerciseManifestEntry entry,
        IdentifierRegistry registry,
        ICollection<Riddle> riddles,
        DateTimeOffset utcNow)
    {
        if (entry.Id == Guid.Empty)
        {
            return Invalid<LessonExercise>("Exercise identifiers cannot be empty.");
        }

        if (entry.RiddleId == Guid.Empty)
        {
            return Invalid<LessonExercise>("Exercise riddle identifiers cannot be empty.");
        }

        if (!registry.ExerciseIds.Add(entry.Id))
        {
            return Invalid<LessonExercise>($"Exercise identifier '{entry.Id}' is duplicated.");
        }

        if (!registry.RiddleIds.Add(entry.RiddleId))
        {
            return Invalid<LessonExercise>($"Riddle identifier '{entry.RiddleId}' is duplicated.");
        }

        if (string.IsNullOrWhiteSpace(entry.Clue))
        {
            return Invalid<LessonExercise>($"Exercise '{entry.Id}' must have clue text.");
        }

        if (string.IsNullOrWhiteSpace(entry.Answer))
        {
            return Invalid<LessonExercise>($"Exercise '{entry.Id}' must have an answer.");
        }

        if (string.IsNullOrWhiteSpace(entry.Explanation))
        {
            return Invalid<LessonExercise>($"Exercise '{entry.Id}' must have an explanation.");
        }

        var answer = entry.Answer!;
        var answerFormatResult = AuthoredAnswerFormat.Validate(answer);
        if (answerFormatResult.IsFailure)
        {
            return Result.Failure<LessonExercise>(answerFormatResult.Error!);
        }

        var answerPatternResult = AnswerPatternDeriver.FromAnswer(answer);
        if (answerPatternResult.IsFailure)
        {
            return Result.Failure<LessonExercise>(answerPatternResult.Error!);
        }

        var clue = entry.Clue!;
        var ranges = entry.Ranges ?? [];
        var rangesResult = RiddleRangeValidator.Validate(clue, ranges);
        if (rangesResult.IsFailure)
        {
            return Result.Failure<LessonExercise>(rangesResult.Error!);
        }

        var riddle = new Riddle(
            entry.RiddleId,
            clue,
            answer,
            answerPatternResult.Value!,
            entry.Explanation!,
            isLesson: true,
            RiddlePublicationState.Draft,
            sofiaPublicationDate: null,
            utcNow,
            utcNow);
        riddle.ReplaceRanges(
            ranges
                .Select(range => new RiddleRange(Guid.NewGuid(), range.Kind, range.Start, range.End))
                .ToArray());
        riddles.Add(riddle);

        return Result.Success(
            new LessonExercise(
                entry.Id,
                entry.RiddleId,
                entry.Ordinal,
                entry.Setup,
                entry.TeachingNote,
                isActive: true));
    }

    private static Result ValidateCurriculumShape(IReadOnlyList<Course> courses)
    {
        var finalMixes = courses
            .SelectMany(course => course.Lessons)
            .Where(lesson => lesson.Kind is LessonKind.FinalMix)
            .ToArray();

        if (finalMixes.Length != 1)
        {
            return Invalid(
                $"The curriculum must contain exactly one final mixed lesson; found {finalMixes.Length}.");
        }

        foreach (var course in courses)
        {
            if (course.Lessons.Any(lesson => lesson.Kind is LessonKind.FinalMix))
            {
                if (course.Lessons.Count != 1)
                {
                    return Invalid($"Final mixed course '{course.Key}' must contain exactly one lesson.");
                }

                continue;
            }

            var mixCount = course.Lessons.Count(lesson => lesson.Kind is LessonKind.Mix);
            if (mixCount != 1)
            {
                return Invalid($"Course '{course.Key}' must contain exactly one mixed lesson.");
            }

            if (!course.Lessons.Any(lesson => lesson.Kind is LessonKind.Technique))
            {
                return Invalid($"Course '{course.Key}' must contain at least one technique lesson.");
            }
        }

        return Result.Success();
    }

    private static Result ValidatePrerequisiteGraph(
        IReadOnlyDictionary<string, LessonManifestEntry> lessonEntriesByKey)
    {
        foreach (var (lessonKey, entry) in lessonEntriesByKey)
        {
            foreach (var prerequisiteKey in entry.PrerequisiteLessonKeys ?? [])
            {
                if (!lessonEntriesByKey.ContainsKey(prerequisiteKey))
                {
                    return Invalid(
                        $"Lesson '{lessonKey}' has an unresolved prerequisite '{prerequisiteKey}'.");
                }
            }
        }

        var states = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var lessonKey in lessonEntriesByKey.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            var cycleKey = FindCycle(lessonKey, lessonEntriesByKey, states);
            if (cycleKey is not null)
            {
                return Invalid($"The prerequisite graph contains a cycle at lesson '{cycleKey}'.");
            }
        }

        return Result.Success();
    }

    private static string? FindCycle(
        string lessonKey,
        IReadOnlyDictionary<string, LessonManifestEntry> lessonEntriesByKey,
        IDictionary<string, int> states)
    {
        if (states.TryGetValue(lessonKey, out var state))
        {
            return state == 1 ? lessonKey : null;
        }

        states[lessonKey] = 1;
        foreach (var prerequisiteKey in lessonEntriesByKey[lessonKey].PrerequisiteLessonKeys ?? [])
        {
            var cycleKey = FindCycle(prerequisiteKey, lessonEntriesByKey, states);
            if (cycleKey is not null)
            {
                return cycleKey;
            }
        }

        states[lessonKey] = 2;
        return null;
    }

    private static Result<IReadOnlyList<PrimerPage>> ValidatePrimer(
        IReadOnlyList<PrimerPageManifestEntry>? entries)
    {
        if (entries is not { Count: > 0 })
        {
            return Invalid<IReadOnlyList<PrimerPage>>("The course manifest must contain at least one primer page.");
        }

        var ordinalResult = ValidateOrdinals(entries.Select(page => page.Ordinal), "primer pages");
        if (ordinalResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<PrimerPage>>(ordinalResult.Error!);
        }

        var pages = new List<PrimerPage>();
        foreach (var entry in entries.OrderBy(page => page.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                return Invalid<IReadOnlyList<PrimerPage>>(
                    $"Primer page {entry.Ordinal} must have a title.");
            }

            if (string.IsNullOrWhiteSpace(entry.Body))
            {
                return Invalid<IReadOnlyList<PrimerPage>>(
                    $"Primer page {entry.Ordinal} must have body text.");
            }

            pages.Add(new PrimerPage(entry.Ordinal, entry.Title, entry.Body, entry.Figure, isActive: true));
        }

        return Result.Success<IReadOnlyList<PrimerPage>>(pages);
    }

    private static Result ValidateOrdinals(IEnumerable<int> ordinals, string collectionName)
    {
        var orderedOrdinals = ordinals.OrderBy(ordinal => ordinal).ToArray();
        for (var index = 0; index < orderedOrdinals.Length; index++)
        {
            var expectedOrdinal = index + 1;
            if (orderedOrdinals[index] != expectedOrdinal)
            {
                return Invalid(
                    $"The ordinals for {collectionName} must be contiguous and start at one; expected {expectedOrdinal}.");
            }
        }

        return Result.Success();
    }

    private static Result Invalid(string message)
    {
        return Result.Failure(new OperationError(message, ErrorType.Validation));
    }

    private static Result<T> Invalid<T>(string message)
    {
        return Result.Failure<T>(new OperationError(message, ErrorType.Validation));
    }

    private sealed class IdentifierRegistry
    {
        public HashSet<Guid> CourseIds { get; } = [];

        public HashSet<string> CourseKeys { get; } = new(StringComparer.Ordinal);

        public HashSet<Guid> LessonIds { get; } = [];

        public HashSet<string> LessonKeys { get; } = new(StringComparer.Ordinal);

        public HashSet<Guid> ExerciseIds { get; } = [];

        public HashSet<Guid> RiddleIds { get; } = [];
    }
}
