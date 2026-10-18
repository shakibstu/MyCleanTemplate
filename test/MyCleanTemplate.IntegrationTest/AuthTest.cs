using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.API.Controllers;
using MyCleanTemplate.API.Models.Auth;
using MyCleanTemplate.Application.Contract.Auth;
using MyCleanTemplate.Infrastructure.Authentication;
using MyCleanTemplate.Infrastructure.Identity;
using MyCleanTemplate.IntegrationTest.Automation;
using Shouldly;
using Xunit;

namespace MyCleanTemplate.IntegrationTest;

public class AuthTest(IntegrationTestWebApplicationFactory applicationFactory)
    : BaseIntegrationTest(applicationFactory)
{
    private const string Password = "Passw0rd!";
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly string CreateUsersPermission =
        AppPermission.NameFor(AppService.Identity, AppFeature.Users, AppAction.Create);

    [Fact]
    public async Task MfaFlow_RequiresCodeAfterEnrollment_AndHidesKey()
    {
        var userName = await CreateUser(CreateUsersPermission);

        // Password-only token works before MFA is enabled, but not for [RequireMfa] endpoints
        var passwordOnly = await SignIn(userName, Password);
        passwordOnly.StatusCode.ShouldBe(HttpStatusCode.OK);
        var passwordOnlyToken = (await passwordOnly.Content.ReadFromJsonAsync<AuthToken>(Ct))!.Token;
        (await CreateProtectedResource(passwordOnlyToken)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Enroll
        var keyResponse = await Send(HttpMethod.Get, "/api/auth/GetMFAKey", passwordOnlyToken);
        keyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var key = (await keyResponse.Content.ReadFromJsonAsync<MFAKeyModel>(Ct))!.Key;
        var verify = await Send(HttpMethod.Post, "/api/auth/VerifyMFAKey", passwordOnlyToken,
            new VerifyMFATokenModel { SecurityCode = Totp(key) });
        verify.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The secret is no longer handed out
        (await Send(HttpMethod.Get, "/api/auth/GetMFAKey", passwordOnlyToken)).StatusCode
            .ShouldBe(HttpStatusCode.Forbidden);

        // Password alone is no longer enough
        (await SignIn(userName, Password)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var withCode = await SignIn(userName, Password, Totp(key));
        withCode.StatusCode.ShouldBe(HttpStatusCode.OK);
        var mfaToken = (await withCode.Content.ReadFromJsonAsync<AuthToken>(Ct))!.Token;
        (await CreateProtectedResource(mfaToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VerifyMfaKey_RequiresAuthentication()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/auth/VerifyMFAKey",
            new VerifyMFATokenModel { SecurityCode = "123456" }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignIn_LocksOut_AfterRepeatedFailures()
    {
        var userName = await CreateUser();
        var maxAttempts = ServiceProvider.GetRequiredService<UserManager<CustomIdentityUser>>()
            .Options.Lockout.MaxFailedAccessAttempts;

        for (var i = 0; i < maxAttempts; i++)
        {
            (await SignIn(userName, "wrong")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        (await SignIn(userName, Password)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<string> CreateUser(params string[] permissions)
    {
        using var scope = ServiceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomIdentityUser>>();
        var userName = $"user-{Guid.NewGuid():N}";
        var result = await userManager.CreateAsync(
            new CustomIdentityUser { UserName = userName, Email = $"{userName}@example.com", Permissions = [.. permissions] },
            Password);
        result.Succeeded.ShouldBeTrue();
        return userName;
    }

    private Task<HttpResponseMessage> SignIn(string userName, string password, string? mfaCode = null) =>
        HttpClient.PostAsJsonAsync("/api/auth/SignIn",
            new SignInModel { Username = userName, Password = password, Token = mfaCode }, Ct);

    private Task<HttpResponseMessage> CreateProtectedResource(string token) =>
        Send(HttpMethod.Post, "/api/auth/CreateProtectedResource", token, new { });

    private Task<HttpResponseMessage> Send(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return HttpClient.SendAsync(request, Ct);
    }

    /// <summary>
    /// RFC 6238 code for an ASP.NET Identity (base32) authenticator key.
    /// </summary>
    private static string Totp(string base32Key)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = string.Concat(base32Key.TrimEnd('=').ToUpperInvariant()
            .Select(c => Convert.ToString(alphabet.IndexOf(c), 2).PadLeft(5, '0')));
        var key = Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2)).ToArray();

        var counter = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30);
        if (BitConverter.IsLittleEndian) Array.Reverse(counter);

        var hash = HMACSHA1.HashData(key, counter);
        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24 | hash[offset + 1] << 16 | hash[offset + 2] << 8 | hash[offset + 3]) % 1_000_000;
        return code.ToString("D6");
    }
}
