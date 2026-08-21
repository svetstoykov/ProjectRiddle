using System.Net.Mail;
using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Interfaces.Users;
using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Services.Users;

/// <summary>
/// Coordinates local-account registration, credential verification, and session lookup.
/// </summary>
public sealed partial class UsersService : IUsersService
{
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 256;
    private const int MaximumEmailLength = 256;

    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;
    private readonly ICurrentUser currentUser;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ILogger<UsersService> logger;

    /// <summary>
    /// Initializes the users service.
    /// </summary>
    /// <param name="userRepository">The account persistence boundary.</param>
    /// <param name="passwordHasher">The provider-neutral password hasher.</param>
    /// <param name="currentUser">The current caller identity.</param>
    /// <param name="dateTimeProvider">The clock used for account timestamps.</param>
    /// <param name="logger">The logger for safe account lifecycle events.</param>
    public UsersService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        ILogger<UsersService> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
        this.currentUser = currentUser;
        this.dateTimeProvider = dateTimeProvider;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<RegisterUserOutput>> RegisterAsync(
        RegisterUserInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var emailResult = ValidateEmail(input.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<RegisterUserOutput>(emailResult.Error!);
        }

        var passwordResult = ValidatePassword(input.Password);
        if (passwordResult.IsFailure)
        {
            return Result.Failure<RegisterUserOutput>(passwordResult.Error!);
        }

        var displayEmail = input.Email.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(displayEmail);
        var existing = await userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
        {
            return EmailConflict<RegisterUserOutput>();
        }

        var user = new User(
            Guid.NewGuid(),
            displayEmail,
            normalizedEmail,
            passwordHasher.HashPassword(input.Password),
            UserRole.User,
            dateTimeProvider.UtcDateTime);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        catch (DuplicateNormalizedEmailException)
        {
            return EmailConflict<RegisterUserOutput>();
        }

        LogUserRegistered(logger, user.Id);
        return Result.Success(new RegisterUserOutput(user.Id, user.Email, user.Role));
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticateUserOutput>> AuthenticateAsync(
        AuthenticateUserInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        {
            return InvalidCredentials<AuthenticateUserOutput>();
        }

        var normalizedEmail = EmailNormalizer.Normalize(input.Email);
        var user = await userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.VerifyHashedPassword(user.PasswordHash, input.Password))
        {
            return InvalidCredentials<AuthenticateUserOutput>();
        }

        return Result.Success(new AuthenticateUserOutput(user.Id, user.Email, user.Role));
    }

    /// <inheritdoc />
    public async Task<Result<CurrentSessionOutput>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<CurrentSessionOutput>(
                new OperationError(
                    "Authentication is required.",
                    ErrorType.Unauthorized,
                    UserErrorCodes.Unauthorized));
        }

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure<CurrentSessionOutput>(
                new OperationError(
                    "Authentication is required.",
                    ErrorType.Unauthorized,
                    UserErrorCodes.Unauthorized));
        }

        return Result.Success(new CurrentSessionOutput(user.Id, user.Email, user.Role));
    }

    private static Result ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > MaximumEmailLength)
        {
            return InvalidEmail();
        }

        try
        {
            var parsed = new MailAddress(email.Trim());
            if (!parsed.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return InvalidEmail();
            }
        }
        catch (FormatException)
        {
            return InvalidEmail();
        }

        return Result.Success();
    }

    private static Result ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password)
            || password.Length < MinimumPasswordLength
            || password.Length > MaximumPasswordLength)
        {
            return Result.Failure(
                new OperationError(
                    "Password must be between 8 and 256 characters.",
                    ErrorType.Validation,
                    UserErrorCodes.PasswordInvalid));
        }

        return Result.Success();
    }

    private static Result InvalidEmail()
    {
        return Result.Failure(
            new OperationError(
                "A valid email address is required.",
                ErrorType.Validation,
                UserErrorCodes.EmailInvalid));
    }

    private static Result<T> EmailConflict<T>()
    {
        return Result.Failure<T>(
            new OperationError(
                "An account with this email address already exists.",
                ErrorType.Conflict,
                UserErrorCodes.EmailConflict));
    }

    private static Result<T> InvalidCredentials<T>()
    {
        return Result.Failure<T>(
            new OperationError(
                "Invalid email or password.",
                ErrorType.Unauthorized,
                UserErrorCodes.CredentialsInvalid));
    }

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Registered a local user account. UserId: {UserId}")]
    private static partial void LogUserRegistered(ILogger logger, Guid userId);
}
