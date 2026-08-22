using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Progress;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Validates a self-asserted anonymous riddle progress snapshot against an immutable riddle.
/// </summary>
public static class AnonymousRiddleProgressValidator
{
    /// <summary>
    /// Validates <paramref name="input" /> against <paramref name="riddle" /> and the answer letter count.
    /// </summary>
    /// <param name="input">The snapshot to validate. Cannot be <see langword="null" />.</param>
    /// <param name="riddle">The immutable riddle. Cannot be <see langword="null" />.</param>
    /// <param name="letterCount">The number of letters in the normalized answer. Cannot be negative.</param>
    /// <returns>A successful result when the snapshot is usable; otherwise an expected failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input" /> or <paramref name="riddle" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="letterCount" /> is negative.</exception>
    public static Result Validate(AnonymousRiddleProgressInput input, Riddle riddle, int letterCount)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(riddle);
        ArgumentOutOfRangeException.ThrowIfNegative(letterCount);

        if (input.SchemaVersion != PublicRiddleLimits.AnonymousProgressSchemaVersion
            || input.UsedHints is null
            || input.RevealedPositions is null
            || input.AnswerAttemptCount < 0
            || !Enum.IsDefined(input.Status)
            || input.UsedHints.Any(kind => !Enum.IsDefined(kind))
            || input.UsedHints.Distinct().Count() != input.UsedHints.Count
            || input.RevealedPositions.Distinct().Count() != input.RevealedPositions.Count)
        {
            return InvalidShape();
        }

        if (input.RiddleId != riddle.Id
            || riddle.SofiaPublicationDate is null
            || input.PublicationDate != riddle.SofiaPublicationDate.Value)
        {
            return Result.Failure(
                new OperationError(
                    "The progress snapshot does not match a public riddle.",
                    ErrorType.UnprocessableInput,
                    RiddleErrorCodes.ProgressReferenceInvalid));
        }

        if (input.RevealedPositions.Any(position => position < 0 || position >= letterCount))
        {
            return InvalidPosition();
        }

        if (input.Status is RiddleProgressStatus.FullyRevealed && input.RevealedPositions.Count != letterCount)
        {
            return InvalidPosition();
        }

        if (input.Status is RiddleProgressStatus.InProgress && input.RevealedPositions.Count == letterCount && letterCount > 0)
        {
            return InvalidPosition();
        }

        return Result.Success();
    }

    private static Result InvalidShape()
    {
        return Result.Failure(
            new OperationError(
                "The progress snapshot is missing required fields or has an invalid shape.",
                ErrorType.MalformedInput,
                RiddleErrorCodes.ProgressInvalid));
    }

    private static Result InvalidPosition()
    {
        return Result.Failure(
            new OperationError(
                "The progress snapshot contains an invalid revealed position.",
                ErrorType.UnprocessableInput,
                RiddleErrorCodes.ProgressPositionInvalid));
    }
}
