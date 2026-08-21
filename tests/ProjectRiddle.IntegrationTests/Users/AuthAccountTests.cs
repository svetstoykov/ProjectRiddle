using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Users;

/// <summary>
/// Verifies registration, uniqueness, and credential-verification behavior through ASP.NET Identity.
/// </summary>
public sealed class AuthAccountTests
{
    /// <summary>
    /// Verifies that self-registration assigns the user role.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task RegistrationAssignsTheUserRole()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = "Visitor@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SessionResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(UserRole.User, created.Role);
        Assert.Equal("Visitor@example.com", created.Email);
    }

    /// <summary>
    /// Verifies that differently cased duplicate emails conflict.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task DifferentlyCasedDuplicateEmailsConflict()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var first = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = "visitor@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);
        var second = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = "Visitor@Example.com", Password = "password1" },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);
        Assert.Equal(UserErrorCodes.EmailConflict, await ReadProblemCodeAsync(second));
    }

    /// <summary>
    /// Verifies that invalid credentials use the same failure whether or not the account exists.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task InvalidCredentialsDoNotDiscloseAccountDetails()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var registered = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = "visitor@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        var unknownAccount = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new SignInRequest { Email = "missing@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);
        var wrongPassword = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new SignInRequest { Email = "visitor@example.com", Password = "wrongpass" },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, unknownAccount.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal("application/problem+json", unknownAccount.Content.Headers.ContentType?.MediaType);
        Assert.Equal("application/problem+json", wrongPassword.Content.Headers.ContentType?.MediaType);

        using var unknownProblem = JsonDocument.Parse(await unknownAccount.Content.ReadAsStringAsync());
        using var wrongProblem = JsonDocument.Parse(await wrongPassword.Content.ReadAsStringAsync());
        Assert.Equal(
            unknownProblem.RootElement.GetProperty("title").GetString(),
            wrongProblem.RootElement.GetProperty("title").GetString());
        Assert.Equal(
            unknownProblem.RootElement.GetProperty("detail").GetString(),
            wrongProblem.RootElement.GetProperty("detail").GetString());
        Assert.Equal(
            unknownProblem.RootElement.GetProperty("code").GetString(),
            wrongProblem.RootElement.GetProperty("code").GetString());
        Assert.Equal(UserErrorCodes.CredentialsInvalid, unknownProblem.RootElement.GetProperty("code").GetString());
    }

    /// <summary>
    /// Verifies that valid credentials return the stored account.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ValidCredentialsReturnTheAccount()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var registered = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = "visitor@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);
        var authenticated = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new SignInRequest { Email = "Visitor@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);

        var created = await registered.Content.ReadFromJsonAsync<SessionResponse>(TestWorkspace.JsonOptions);
        var session = await authenticated.Content.ReadFromJsonAsync<SessionResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(created);
        Assert.NotNull(session);
        Assert.Equal(created.Id, session.Id);
        Assert.Equal(UserRole.User, session.Role);
    }

    private static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
