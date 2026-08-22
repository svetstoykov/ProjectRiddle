using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Validates that labelled clue ranges fall inside the clue and have a positive length.
/// </summary>
public static class RiddleRangeValidator
{
    /// <summary>
    /// Validates the supplied ranges against the clue text.
    /// </summary>
    /// <param name="clue">The clue text. Cannot be <see langword="null" />.</param>
    /// <param name="ranges">The ranges to validate. Cannot be <see langword="null" />.</param>
    /// <returns>A successful result when every range is valid; otherwise a validation failure.</returns>
    public static Result Validate(string clue, IReadOnlyList<RiddleRangeInput> ranges)
    {
        ArgumentNullException.ThrowIfNull(clue);
        ArgumentNullException.ThrowIfNull(ranges);

        foreach (var range in ranges)
        {
            if (range.Start < 0 || range.End <= range.Start || range.End > clue.Length)
            {
                return Result.Failure(
                    new OperationError(
                        "Each range must be a non-empty span within the clue text.",
                        ErrorType.Validation,
                        RiddleErrorCodes.RangeInvalid));
            }

            if (!Enum.IsDefined(range.Kind))
            {
                return Result.Failure(
                    new OperationError(
                        "Each range must use a recognized structural kind.",
                        ErrorType.Validation,
                        RiddleErrorCodes.RangeInvalid));
            }
        }

        return Result.Success();
    }
}
