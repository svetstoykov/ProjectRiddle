using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Infrastructure.Identity;
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
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var administrator = await users.FindByEmailAsync("admin@example.com");
        Assert.NotNull(administrator);
        var roles = await users.GetRolesAsync(administrator);
        Assert.Equal(UserRole.Admin, RoleClaimValues.ToUserRole(roles));
        Assert.False(string.Equals(administrator.PasswordHash, "password1", StringComparison.Ordinal));
        Assert.True(await users.CheckPasswordAsync(administrator, "password1"));
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
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var registered = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com"
                };
                var created = await users.CreateAsync(registered, "password1");
                Assert.True(created.Succeeded);
                var role = await users.AddToRoleAsync(registered, RoleClaimValues.User);
                Assert.True(role.Succeeded);
            }

            await using var secondFactory = new ApplicationFactory(
                databasePath,
                TestWorkspace.TimeZoneId,
                bootstrapEmail: "admin@example.com",
                bootstrapPassword: "other-password");
            using var verifyScope = secondFactory.Services.CreateScope();
            var userManager = verifyScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existing = await userManager.FindByEmailAsync("admin@example.com");

            Assert.NotNull(existing);
            var roles = await userManager.GetRolesAsync(existing);
            Assert.Equal(UserRole.User, RoleClaimValues.ToUserRole(roles));
            Assert.True(await userManager.CheckPasswordAsync(existing, "password1"));
            Assert.False(await userManager.CheckPasswordAsync(existing, "other-password"));
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }
}
