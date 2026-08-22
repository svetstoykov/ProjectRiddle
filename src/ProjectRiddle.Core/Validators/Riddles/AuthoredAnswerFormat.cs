using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Validates that an authored answer can be reconstructed from a letter tile grid.
/// </summary>
/// <remarks>
/// The public answer pattern carries letter counts per word and nothing about separators, so a solver has no way to
/// place a hyphen or an apostrophe. An answer containing any character other than a letter or a word-separating space
/// is unsolvable through the grid and is rejected while it is being authored.
/// </remarks>
public static class AuthoredAnswerFormat
{
    /// <summary>
    /// Validates that <paramref name="answer" /> contains only letters separated by single interior spaces.
    /// </summary>
    /// <param name="answer">The authored answer. Cannot be <see langword="null" />.</param>
    /// <returns>A successful result when the answer is solvable; otherwise an expected validation failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="answer" /> is <see langword="null" />.</exception>
    public static Result Validate(string answer)
    {
        ArgumentNullException.ThrowIfNull(answer);

        var normalizedAnswer = AnswerNormalizer.Normalize(answer);
        var hasUnsupportedCharacter = normalizedAnswer
            .Any(character => !char.IsLetter(character) && character != ' ');

        if (normalizedAnswer.Length == 0 || hasUnsupportedCharacter)
        {
            return Result.Failure(
                new OperationError(
                    "The answer may contain only letters separated by single spaces.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerFormatInvalid));
        }

        return Result.Success();
    }
}
