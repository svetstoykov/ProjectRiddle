namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Extracts letter characters from a normalized answer, excluding word separators.
/// </summary>
public static class AnswerLetters
{
    /// <summary>
    /// Returns the letters of <paramref name="normalizedAnswer" /> in zero-based reveal order.
    /// </summary>
    /// <param name="normalizedAnswer">The normalized answer. Cannot be <see langword="null" />.</param>
    /// <returns>The answer letters, excluding spaces and other separators.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizedAnswer" /> is <see langword="null" />.</exception>
    public static IReadOnlyList<char> FromNormalizedAnswer(string normalizedAnswer)
    {
        ArgumentNullException.ThrowIfNull(normalizedAnswer);
        return [.. normalizedAnswer.Where(char.IsLetter)];
    }
}
