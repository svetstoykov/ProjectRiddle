namespace ProjectRiddle.Core.Results.Models;

/// <summary>
/// Defines the expected failure categories returned by Core operations.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Indicates that the requested resource does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// Indicates that an operation input is invalid.
    /// </summary>
    Validation,

    /// <summary>
    /// Indicates that an input cannot be parsed or has an invalid shape.
    /// </summary>
    MalformedInput,

    /// <summary>
    /// Indicates that an otherwise well-formed input cannot be processed.
    /// </summary>
    UnprocessableInput,

    /// <summary>
    /// Indicates that the operation conflicts with existing state.
    /// </summary>
    Conflict,

    /// <summary>
    /// Indicates that the requested operation is invalid for the current state.
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// Indicates that the caller is not authenticated.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Indicates that the authenticated caller lacks permission.
    /// </summary>
    Forbidden,

    /// <summary>
    /// Indicates that a dependency did not complete in time.
    /// </summary>
    Timeout,

    /// <summary>
    /// Indicates that an external dependency failed.
    /// </summary>
    ExternalDependencyFailure,

    /// <summary>
    /// Indicates that an expected internal operation failed.
    /// </summary>
    InternalError
}
