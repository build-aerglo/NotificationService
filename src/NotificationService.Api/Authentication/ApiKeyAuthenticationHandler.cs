using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NotificationService.Api.Authentication;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationConstants.HeaderName, out var extractedApiKey))
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key header."));

        var configuredKey = configuration["ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey) || extractedApiKey != configuredKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}