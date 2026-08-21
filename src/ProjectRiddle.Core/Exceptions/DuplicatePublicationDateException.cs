namespace ProjectRiddle.Core.Exceptions;

/// <summary>
/// Represents a persistence conflict on the one-riddle-per-Sofia-date uniqueness constraint.
/// </summary>
public sealed class DuplicatePublicationDateException : Exception
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    /// <param name="publicationDate">The Sofia calendar date that is already occupied.</param>
    public DuplicatePublicationDateException(DateOnly publicationDate)
        : base("A riddle already occupies the requested Sofia publication date.")
    {
        PublicationDate = publicationDate;
    }

    /// <summary>
    /// Gets the Sofia calendar date that is already occupied.
    /// </summary>
    public DateOnly PublicationDate { get; }
}
