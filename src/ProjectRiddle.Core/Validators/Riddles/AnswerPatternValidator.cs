using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Validates that an answer pattern is a comma-separated list of positive letter counts matching the answer.
/// </summary>
public static class AnswerPatternValidator
{
    /// <summary>
    /// Validates that <paramref name="answerPattern" /> describes the letter counts of <paramref name="answer" />.
    /// </summary>
    /// <param name="answer">The answer text. Cannot be <see langword="null" />.</param>
    /// <param name="answerPattern">The answer pattern. Cannot be <see langword="null" />.</param>
    /// <returns>A successful result when the pattern matches; otherwise a validation failure.</returns>
    public static Result Validate(string answer, string answerPattern)
    {
        ArgumentNullException.ThrowIfNull(answer);
        ArgumentNullException.ThrowIfNull(answerPattern);

        var trimmedPattern = answerPattern.Trim();
        if (trimmedPattern.Length == 0)
        {
            return Result.Failure(
                new OperationError(
                    "Answer pattern is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerPatternInvalid));
        }

        var groups = trimmedPattern.Split(',', StringSplitOptions.TrimEntries);
        var expectedCounts = new int[groups.Length];

        for (var index = 0; index < groups.Length; index++)
        {
            if (!int.TryParse(groups[index], out var count) || count <= 0)
            {
                return Result.Failure(
                    new OperationError(
                        "Answer pattern must be a comma-separated list of positive integers.",
                        ErrorType.Validation,
                        RiddleErrorCodes.AnswerPatternInvalid));
            }

            expectedCounts[index] = count;
        }

        var normalizedAnswer = AnswerNormalizer.Normalize(answer);
        if (normalizedAnswer.Length == 0)
        {
            return Result.Failure(
                new OperationError(
                    "Answer is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerInvalid));
        }

        var words = normalizedAnswer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != expectedCounts.Length)
        {
            return Result.Failure(
                new OperationError(
                    "Answer pattern does not match the number of words in the answer.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerPatternInvalid));
        }

        for (var index = 0; index < words.Length; index++)
        {
            var letterCount = words[index].Count(char.IsLetter);
            if (letterCount != expectedCounts[index])
            {
                return Result.Failure(
                    new OperationError(
                        "Answer pattern does not match the letter counts in the answer.",
                        ErrorType.Validation,
                        RiddleErrorCodes.AnswerPatternInvalid));
            }
        }

        return Result.Success();
    }
}
