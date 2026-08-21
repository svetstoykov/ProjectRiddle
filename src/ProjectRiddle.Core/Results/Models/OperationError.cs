namespace ProjectRiddle.Core.Results.Models;

/// <summary>
/// Represents a safe, transport-independent description of an expected operation failure.
/// </summary>
public sealed class OperationError
{
    /// <summary>
    /// Initializes an operation error.
    /// </summary>
    /// <param name="message">The safe message describing the failure. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="type">The category of failure.</param>
    /// <param name="code">The optional stable code for the failure.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message" /> is empty or whitespace.</exception>
    public OperationError(string message, ErrorType type, string? code = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Message = message;
        Type = type;
        Code = string.IsNullOrWhiteSpace(code) ? null : code;
    }

    /// <summary>
    /// Gets the safe message describing the failure.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the category of failure.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Gets the optional stable code for the failure.
    /// </summary>
    public string? Code { get; }
}
