using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Play;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Courses;

/// <summary>
/// Validates self-asserted anonymous course progress before it reaches the play engine or account storage.
/// </summary>
public static class AnonymousCourseProgressValidator
{
    /// <summary>
    /// Validates a per-exercise play snapshot against the exercise it claims and the answer letter count.
    /// </summary>
    /// <param name="input">The snapshot to validate. Cannot be <see langword="null" />.</param>
    /// <param name="exerciseId">The exercise being played.</param>
    /// <param name="letterCount">The number of letters in the normalized answer. Cannot be negative.</param>
    /// <returns>A successful result when the snapshot is usable; otherwise an expected failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="letterCount" /> is negative.</exception>
    public static Result ValidateExerciseSnapshot(
        AnonymousCourseExerciseProgressInput input,
        Guid exerciseId,
        int letterCount)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(letterCount);

        if (input.ExerciseId != exerciseId)
        {
            return Result.Failure(
                new OperationError(
                    "The progress snapshot does not describe this exercise.",
                    ErrorType.Validation,
                    CourseErrorCodes.ProgressReferenceInvalid));
        }

        if (input.SchemaVersion != CourseLimits.AnonymousProgressSchemaVersion
            || input.UsedHints is null
            || input.RevealedPositions is null
            || input.AnswerAttemptCount < 0
            || !Enum.IsDefined(input.Status)
            || input.UsedHints.Any(kind => !Enum.IsDefined(kind))
            || input.UsedHints.Distinct().Count() != input.UsedHints.Count
            || input.RevealedPositions.Distinct().Count() != input.RevealedPositions.Count)
        {
            return Invalid();
        }

        if (input.RevealedPositions.Any(position => position < 0 || position >= letterCount))
        {
            return Invalid();
        }

        if (input.Status is RiddleProgressStatus.FullyRevealed && input.RevealedPositions.Count != letterCount)
        {
            return Invalid();
        }

        if (input.Status is RiddleProgressStatus.InProgress
            && letterCount > 0
            && input.RevealedPositions.Count == letterCount)
        {
            return Invalid();
        }

        return Result.Success();
    }

    private static Result Invalid()
    {
        return Result.Failure(
            new OperationError(
                "The course progress snapshot is missing required fields or has an invalid shape.",
                ErrorType.Validation,
                CourseErrorCodes.ProgressInvalid));
    }
}
