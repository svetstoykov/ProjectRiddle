using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Derives the public letter-count pattern from an authored answer.
/// </summary>
public static class AnswerPatternDeriver
{
    /// <summary>
    /// Builds a comma-separated list of letter counts for each word in <paramref name="answer" />.
    /// </summary>
    /// <param name="answer">The answer text. Cannot be <see langword="null" />.</param>
    /// <returns>The derived pattern when every word contains letters; otherwise a validation failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="answer" /> is <see langword="null" />.</exception>
    public static Result<string> FromAnswer(string answer)
    {
        ArgumentNullException.ThrowIfNull(answer);

        var normalizedAnswer = AnswerNormalizer.Normalize(answer);
        if (normalizedAnswer.Length == 0)
        {
            return Result.Failure<string>(
                new OperationError(
                    "Answer is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerInvalid));
        }

        var words = normalizedAnswer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var counts = new int[words.Length];

        for (var index = 0; index < words.Length; index++)
        {
            var letterCount = words[index].Count(char.IsLetter);
            if (letterCount <= 0)
            {
                return Result.Failure<string>(
                    new OperationError(
                        "Each word in the answer must contain at least one letter.",
                        ErrorType.Validation,
                        RiddleErrorCodes.AnswerInvalid));
            }

            counts[index] = letterCount;
        }

        return Result.Success(string.Join(',', counts));
    }
}
