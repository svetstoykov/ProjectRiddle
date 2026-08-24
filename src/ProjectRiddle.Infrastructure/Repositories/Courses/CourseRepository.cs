using Microsoft.EntityFrameworkCore;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Courses;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Repositories.Courses;

/// <summary>
/// Persists the course curriculum through EF Core.
/// </summary>
public sealed class CourseRepository : ICourseRepository
{
    private readonly ProjectRiddleDbContext _dbContext;

    /// <summary>
    /// Initializes the course repository.
    /// </summary>
    /// <param name="dbContext">The persistence context.</param>
    public CourseRepository(ProjectRiddleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        this._dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Course>> ListActiveCurriculumAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Course>()
            .Include(course => course.Lessons)
                .ThenInclude(lesson => lesson.Prerequisites)
            .Include(course => course.Lessons)
                .ThenInclude(lesson => lesson.Exercises)
            .AsNoTracking()
            .Where(course => course.IsActive)
            .OrderBy(course => course.Ordinal)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Lesson?> GetActiveLessonAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<Lesson>()
            .Include(lesson => lesson.Prerequisites)
            .Include(lesson => lesson.Exercises)
            .AsNoTracking()
            .SingleOrDefaultAsync(lesson => lesson.Id == lessonId && lesson.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LessonExercise?> GetActiveExerciseAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<LessonExercise>()
            .AsNoTracking()
            .SingleOrDefaultAsync(exercise => exercise.Id == exerciseId && exercise.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LessonExercise>> ListActiveExercisesByIdsAsync(
        IReadOnlyCollection<Guid> exerciseIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exerciseIds);
        if (exerciseIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Set<LessonExercise>()
            .AsNoTracking()
            .Where(exercise => exercise.IsActive && exerciseIds.Contains(exercise.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrimerPage>> ListActivePrimerPagesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<PrimerPage>()
            .AsNoTracking()
            .Where(page => page.IsActive)
            .OrderBy(page => page.Ordinal)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SeedCurriculumAsync(CourseCurriculum curriculum, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(curriculum);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await UpsertLessonRiddlesAsync(curriculum.LessonRiddles, cancellationToken);
        await UpsertCoursesAsync(curriculum.Courses, cancellationToken);
        await UpsertPrimerAsync(curriculum.PrimerPages, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Upserts the clues behind the lesson exercises without disturbing the progress that references them.
    /// </summary>
    /// <param name="riddles">The projected lesson riddles.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    /// <remarks>
    /// A lesson riddle absent from the manifest is left alone rather than deleted. Deleting it would cascade away
    /// the progress rows a learner earned on it; deactivating its exercise already hides it.
    /// </remarks>
    private async Task UpsertLessonRiddlesAsync(IReadOnlyList<Riddle> riddles, CancellationToken cancellationToken)
    {
        var ids = riddles.Select(riddle => riddle.Id).ToArray();
        var existing = await _dbContext.Set<Riddle>()
            .Include(riddle => riddle.Ranges)
            .Where(riddle => ids.Contains(riddle.Id))
            .ToDictionaryAsync(riddle => riddle.Id, cancellationToken);

        foreach (var riddle in riddles)
        {
            if (!existing.TryGetValue(riddle.Id, out var stored))
            {
                _dbContext.Set<Riddle>().Add(riddle);
                continue;
            }

            stored.ReplaceContent(
                riddle.Clue,
                riddle.Answer,
                riddle.AnswerPattern,
                riddle.Explanation,
                riddle.UpdatedAtUtc);

            // Ranges are replaced only when they actually differ. Rewriting identical owned rows on every restart
            // would churn the table and regenerate their identifiers for nothing.
            if (!RangesMatch(stored.Ranges, riddle.Ranges))
            {
                stored.ReplaceRanges(riddle.Ranges);
            }
        }
    }

    private async Task UpsertCoursesAsync(IReadOnlyList<Course> courses, CancellationToken cancellationToken)
    {
        var stored = await _dbContext.Set<Course>()
            .Include(course => course.Lessons)
                .ThenInclude(lesson => lesson.Prerequisites)
            .Include(course => course.Lessons)
                .ThenInclude(lesson => lesson.Exercises)
            .ToListAsync(cancellationToken);
        var storedById = stored.ToDictionary(course => course.Id);
        var manifestCourseIds = courses.Select(course => course.Id).ToHashSet();

        foreach (var course in courses)
        {
            if (!storedById.TryGetValue(course.Id, out var target))
            {
                _dbContext.Set<Course>().Add(course);
                continue;
            }

            target.ReplaceContent(course.Key, course.Ordinal, course.Title, course.Intro);
            MergeLessons(target, course);
        }

        foreach (var absent in stored.Where(course => !manifestCourseIds.Contains(course.Id)))
        {
            absent.Deactivate();
        }
    }

    /// <summary>
    /// Merges the manifest's lessons into a stored course, adding, updating, and deactivating in place.
    /// </summary>
    /// <param name="target">The stored course.</param>
    /// <param name="source">The projected course from the manifest.</param>
    private static void MergeLessons(Course target, Course source)
    {
        var storedLessons = target.Lessons.ToDictionary(lesson => lesson.Id);
        var manifestLessonIds = source.Lessons.Select(lesson => lesson.Id).ToHashSet();
        var merged = new List<Lesson>();

        foreach (var lesson in source.Lessons)
        {
            if (!storedLessons.TryGetValue(lesson.Id, out var storedLesson))
            {
                merged.Add(lesson);
                continue;
            }

            storedLesson.ReplaceContent(lesson.Key, lesson.Ordinal, lesson.Title, lesson.Intro, lesson.Kind);
            storedLesson.ReplacePrerequisites(lesson.Prerequisites.ToArray());
            MergeExercises(storedLesson, lesson);
            merged.Add(storedLesson);
        }

        foreach (var absent in target.Lessons.Where(lesson => !manifestLessonIds.Contains(lesson.Id)))
        {
            absent.Deactivate();
            merged.Add(absent);
        }

        target.ReplaceLessons(merged);
    }

    // ReplaceLessons and ReplaceExercises clear their backing list before re-adding the same tracked instances.
    // Change detection runs at SaveChanges against the final state, so nothing is orphaned. Task 11's idempotency
    // check is what confirms it: a second start must leave the row counts unchanged.
    private static void MergeExercises(Lesson target, Lesson source)
    {
        var storedExercises = target.Exercises.ToDictionary(exercise => exercise.Id);
        var manifestExerciseIds = source.Exercises.Select(exercise => exercise.Id).ToHashSet();
        var merged = new List<LessonExercise>();

        foreach (var exercise in source.Exercises)
        {
            if (!storedExercises.TryGetValue(exercise.Id, out var storedExercise))
            {
                merged.Add(exercise);
                continue;
            }

            storedExercise.ReplaceContent(
                exercise.RiddleId,
                exercise.Ordinal,
                exercise.Setup,
                exercise.TeachingNote);
            merged.Add(storedExercise);
        }

        foreach (var absent in target.Exercises.Where(exercise => !manifestExerciseIds.Contains(exercise.Id)))
        {
            absent.Deactivate();
            merged.Add(absent);
        }

        target.ReplaceExercises(merged);
    }

    private async Task UpsertPrimerAsync(IReadOnlyList<PrimerPage> pages, CancellationToken cancellationToken)
    {
        var stored = await _dbContext.Set<PrimerPage>().ToListAsync(cancellationToken);
        var storedByOrdinal = stored.ToDictionary(page => page.Ordinal);
        var manifestOrdinals = pages.Select(page => page.Ordinal).ToHashSet();

        foreach (var page in pages)
        {
            if (!storedByOrdinal.TryGetValue(page.Ordinal, out var target))
            {
                _dbContext.Set<PrimerPage>().Add(page);
                continue;
            }

            target.ReplaceContent(page.Title, page.Body, page.Figure);
        }

        foreach (var absent in stored.Where(page => !manifestOrdinals.Contains(page.Ordinal)))
        {
            absent.Deactivate();
        }
    }

    private static bool RangesMatch(IReadOnlyList<RiddleRange> stored, IReadOnlyList<RiddleRange> incoming)
    {
        if (stored.Count != incoming.Count)
        {
            return false;
        }

        var storedKeys = stored
            .Select(range => (range.Kind, range.Start, range.End))
            .OrderBy(key => key)
            .ToArray();
        var incomingKeys = incoming
            .Select(range => (range.Kind, range.Start, range.End))
            .OrderBy(key => key)
            .ToArray();

        return storedKeys.SequenceEqual(incomingKeys);
    }
}
