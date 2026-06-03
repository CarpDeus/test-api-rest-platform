using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TestApiRestPlatform.Authentication;

public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationOptions>
{
    private readonly IConfiguration _configuration;

    public CustomAuthenticationHandler(
        IOptionsMonitor<CustomAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if Authorization header exists
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("No Authorization header provided"));
        }

        var authHeader = Request.Headers["Authorization"].ToString();

        // Get expected auth header from environment variable or configuration
        var expectedAuth = Environment.GetEnvironmentVariable("ExpectedAuthHeader")
            ?? _configuration["Authentication:ExpectedAuthHeader"];

        // Validate the authorization header
        if (string.IsNullOrEmpty(authHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("No Authorization header provided"));
        }

        if (authHeader != expectedAuth)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header"));
        }

        // Create claims principal for successful authentication
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "AuthenticatedUser"),
            new Claim(ClaimTypes.AuthenticationMethod, "CustomScheme")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
