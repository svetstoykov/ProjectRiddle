namespace ProjectRiddle.Core.Results.Models;

/// <summary>
/// Represents the success or expected failure of an operation without a value.
/// </summary>
public sealed class Result
{
    private Result(bool isSuccess, OperationError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation returned an expected failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the expected failure when the operation did not succeed.
    /// </summary>
    public OperationError? Error { get; }

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The expected failure. Cannot be <see langword="null" />.</param>
    /// <returns>A failed result containing <paramref name="error" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error" /> is <see langword="null" />.</exception>
    public static Result Failure(OperationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(false, error);
    }

    /// <summary>
    /// Creates a successful result containing a value.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the operation.</typeparam>
    /// <param name="value">The value returned by the operation.</param>
    /// <returns>A successful result containing <paramref name="value" />.</returns>
    public static Result<T> Success<T>(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result containing a value type parameter.
    /// </summary>
    /// <typeparam name="T">The type of value that a successful operation would produce.</typeparam>
    /// <param name="error">The expected failure. Cannot be <see langword="null" />.</param>
    /// <returns>A failed result containing <paramref name="error" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error" /> is <see langword="null" />.</exception>
    public static Result<T> Failure<T>(OperationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(false, default, error);
    }
}
