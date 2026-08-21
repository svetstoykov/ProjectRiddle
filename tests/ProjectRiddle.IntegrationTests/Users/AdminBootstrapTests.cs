using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Core.Services.Users;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Users;

/// <summary>
/// Verifies first-administrator bootstrap without overwriting existing accounts.
/// </summary>
public sealed class AdminBootstrapTests
{
    /// <summary>
    /// Verifies that bootstrap creates an administrator when the email is unused.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task BootstrapCreatesAnAdministratorWhenTheAccountDoesNotExist()
    {
        await using var workspace = TestWorkspace.Create(
            bootstrapEmail: "admin@example.com",
            bootstrapPassword: "password1");
        using var scope = workspace.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var administrator = await users.GetByNormalizedEmailAsync(
            EmailNormalizer.Normalize("admin@example.com"),
            CancellationToken.None);

        Assert.NotNull(administrator);
        Assert.Equal(UserRole.Admin, administrator.Role);
        Assert.False(string.Equals(administrator.PasswordHash, "password1", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that bootstrap does not overwrite an existing account or role.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task BootstrapDoesNotOverwriteAnExistingAccountOrRole()
    {
        var directory = Directory.CreateTempSubdirectory("project-riddle-");
        var databasePath = Path.Combine(directory.FullName, "project-riddle.db");

        try
        {
            await using (var firstFactory = new ApplicationFactory(databasePath, TestWorkspace.TimeZoneId))
            {
                using var scope = firstFactory.Services.CreateScope();
                var users = scope.ServiceProvider.GetRequiredService<IUsersService>();
                var registered = await users.RegisterAsync(
                    new RegisterUserInput("admin@example.com", "password1"),
                    CancellationToken.None);
                Assert.True(registered.IsSuccess);
                Assert.Equal(UserRole.User, registered.Value!.Role);
            }

            await using var secondFactory = new ApplicationFactory(
                databasePath,
                TestWorkspace.TimeZoneId,
                bootstrapEmail: "admin@example.com",
                bootstrapPassword: "other-password");
            using var verifyScope = secondFactory.Services.CreateScope();
            var userRepository = verifyScope.ServiceProvider.GetRequiredService<IUserRepository>();
            var existing = await userRepository.GetByNormalizedEmailAsync(
                EmailNormalizer.Normalize("admin@example.com"),
                CancellationToken.None);

            Assert.NotNull(existing);
            Assert.Equal(UserRole.User, existing.Role);

            var usersService = verifyScope.ServiceProvider.GetRequiredService<IUsersService>();
            var originalPassword = await usersService.AuthenticateAsync(
                new AuthenticateUserInput("admin@example.com", "password1"),
                CancellationToken.None);
            var bootstrapPassword = await usersService.AuthenticateAsync(
                new AuthenticateUserInput("admin@example.com", "other-password"),
                CancellationToken.None);

            Assert.True(originalPassword.IsSuccess);
            Assert.True(bootstrapPassword.IsFailure);
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }
}
