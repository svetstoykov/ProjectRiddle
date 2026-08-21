namespace ProjectRiddle.Core.Results.Models;

/// <summary>
/// Represents the success or expected failure of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of value produced by a successful operation.</typeparam>
public sealed class Result<T>
{
    internal Result(bool isSuccess, T? value, OperationError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
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
    /// Gets the value returned by a successful operation.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the expected failure when the operation did not succeed.
    /// </summary>
    public OperationError? Error { get; }

}
