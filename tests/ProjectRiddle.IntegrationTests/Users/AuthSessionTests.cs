using System.Net;
using System.Net.Http.Json;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Users;

/// <summary>
/// Verifies cookie session establishment, sign-out, and rejection of caller-supplied roles.
/// </summary>
public sealed class AuthSessionTests
{
    /// <summary>
    /// Verifies that sign-in establishes a session and sign-out clears it.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task SignInEstablishesASessionAndSignOutClearsIt()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var session = await TestWorkspace.RegisterAndSignInAsync(
            client,
            "visitor@example.com",
            "password1");
        Assert.Equal(UserRole.User, session.Role);

        var current = await client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        var currentSession = await current.Content.ReadFromJsonAsync<SessionResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(currentSession);
        Assert.Equal(session.Id, currentSession.Id);

        var signOut = await client.PostAsync("/api/auth/sign-out", content: null);
        Assert.Equal(HttpStatusCode.NoContent, signOut.StatusCode);

        var afterSignOut = await client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.Unauthorized, afterSignOut.StatusCode);
        Assert.Equal("application/problem+json", afterSignOut.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies that a caller-supplied admin role is ignored during registration.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task CallerSuppliedAdminRoleIsRejected()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                email = "visitor@example.com",
                password = "password1",
                role = "admin"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SessionResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(UserRole.User, created.Role);
    }
}
