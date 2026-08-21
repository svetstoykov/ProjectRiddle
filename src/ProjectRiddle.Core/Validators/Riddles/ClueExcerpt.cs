using ProjectRiddle.Core.Constants.Riddles;

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Builds a bounded public excerpt from clue text.
/// </summary>
public static class ClueExcerpt
{
    /// <summary>
    /// Returns the first <see cref="PublicRiddleLimits.ClueExcerptMaxChars" /> Unicode characters of the trimmed clue.
    /// </summary>
    /// <param name="clue">The clue text. Cannot be <see langword="null" />.</param>
    /// <returns>The excerpt.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clue" /> is <see langword="null" />.</exception>
    public static string FromClue(string clue)
    {
        ArgumentNullException.ThrowIfNull(clue);

        var trimmed = clue.Trim();
        var runes = trimmed.EnumerateRunes().Take(PublicRiddleLimits.ClueExcerptMaxChars);
        return string.Concat(runes);
    }
}
