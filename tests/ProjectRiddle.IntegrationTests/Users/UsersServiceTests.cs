using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Services.Users;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Users;

/// <summary>
/// Verifies registration, uniqueness, and credential-verification behavior.
/// </summary>
public sealed class UsersServiceTests
{
    /// <summary>
    /// Verifies that self-registration assigns the user role.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task RegistrationAssignsTheUserRole()
    {
        await using var workspace = TestWorkspace.Create();
        var (scope, users) = workspace.GetScopedService<IUsersService>();
        using (scope)
        {
            var result = await users.RegisterAsync(
                new RegisterUserInput("Visitor@example.com", "password1"),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(UserRole.User, result.Value!.Role);
            Assert.Equal("Visitor@example.com", result.Value.Email);
        }
    }

    /// <summary>
    /// Verifies that differently cased duplicate emails conflict.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task DifferentlyCasedDuplicateEmailsConflict()
    {
        await using var workspace = TestWorkspace.Create();
        var (scope, users) = workspace.GetScopedService<IUsersService>();
        using (scope)
        {
            var first = await users.RegisterAsync(
                new RegisterUserInput("visitor@example.com", "password1"),
                CancellationToken.None);
            var second = await users.RegisterAsync(
                new RegisterUserInput("Visitor@Example.com", "password1"),
                CancellationToken.None);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsFailure);
            Assert.Equal(ErrorType.Conflict, second.Error!.Type);
            Assert.Equal(UserErrorCodes.EmailConflict, second.Error.Code);
        }
    }

    /// <summary>
    /// Verifies that invalid credentials use the same failure whether or not the account exists.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task InvalidCredentialsDoNotDiscloseAccountDetails()
    {
        await using var workspace = TestWorkspace.Create();
        var (scope, users) = workspace.GetScopedService<IUsersService>();
        using (scope)
        {
            var registered = await users.RegisterAsync(
                new RegisterUserInput("visitor@example.com", "password1"),
                CancellationToken.None);
            Assert.True(registered.IsSuccess);

            var unknownAccount = await users.AuthenticateAsync(
                new AuthenticateUserInput("missing@example.com", "password1"),
                CancellationToken.None);
            var wrongPassword = await users.AuthenticateAsync(
                new AuthenticateUserInput("visitor@example.com", "wrongpass"),
                CancellationToken.None);

            Assert.True(unknownAccount.IsFailure);
            Assert.True(wrongPassword.IsFailure);
            Assert.Equal(unknownAccount.Error!.Type, wrongPassword.Error!.Type);
            Assert.Equal(unknownAccount.Error.Code, wrongPassword.Error.Code);
            Assert.Equal(unknownAccount.Error.Message, wrongPassword.Error.Message);
            Assert.Equal(ErrorType.Unauthorized, unknownAccount.Error.Type);
            Assert.Equal(UserErrorCodes.CredentialsInvalid, unknownAccount.Error.Code);
        }
    }

    /// <summary>
    /// Verifies that valid credentials return the stored account.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ValidCredentialsReturnTheAccount()
    {
        await using var workspace = TestWorkspace.Create();
        var (scope, users) = workspace.GetScopedService<IUsersService>();
        using (scope)
        {
            var registered = await users.RegisterAsync(
                new RegisterUserInput("visitor@example.com", "password1"),
                CancellationToken.None);
            var authenticated = await users.AuthenticateAsync(
                new AuthenticateUserInput("Visitor@example.com", "password1"),
                CancellationToken.None);

            Assert.True(registered.IsSuccess);
            Assert.True(authenticated.IsSuccess);
            Assert.Equal(registered.Value!.Id, authenticated.Value!.Id);
            Assert.Equal(UserRole.User, authenticated.Value.Role);
        }
    }
}
