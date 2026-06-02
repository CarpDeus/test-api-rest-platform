using Microsoft.AspNetCore.Mvc;
using TestApiRestPlatform.Models;

namespace TestApiRestPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GetController : ControllerBase
{
    private static readonly int[] AllowedStatusCodes =
    [
        StatusCodes.Status200OK,
        StatusCodes.Status206PartialContent,
        StatusCodes.Status301MovedPermanently,
        StatusCodes.Status302Found,
        StatusCodes.Status304NotModified,
        StatusCodes.Status400BadRequest,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status500InternalServerError
    ];

    private readonly ILogger<GetController> _logger;
    private readonly IConfiguration _configuration;

    public GetController(ILogger<GetController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns the requested HTTP status code.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <remarks>Allowed GET status codes: 200, 206, 301, 302, 304, 400, 401, 403, 404, 500.</remarks>
    [HttpGet("{status}")]
    public IActionResult GetStatus(int status)
    {
        var statusValidationResult = ValidateStatusCode(status);
        if (statusValidationResult is not null)
        {
            return statusValidationResult;
        }

        _logger.LogInformation("GET request received for status: {Status}", status);
        return StatusCode(status, new { message = $"Returning HTTP status {status}" });
    }

    /// <summary>
    /// Validates the supplied Authorization header value.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="authorization">The Authorization header value expected by the API.</param>
    /// <remarks>Allowed GET status codes: 200, 206, 301, 302, 304, 400, 401, 403, 404, 500.</remarks>
    [HttpGet("authenticate")]
    [HttpGet("authenticate/{status}")]
    public IActionResult Authenticate(int status = StatusCodes.Status200OK, [FromHeader(Name = "Authorization")] string? authorization)
    {
        var statusValidationResult = ValidateStatusCode(status);
        if (statusValidationResult is not null)
        {
            return statusValidationResult;
        }

        var expectedAuth = _configuration["Authentication:ExpectedAuthHeader"];
        var authHeader = authorization;

        var sanitizedAuthHeader = authHeader?.Replace("\n", "").Replace("\r", "") ?? "null";
        _logger.LogInformation("Authenticate request received with auth header: {AuthHeader}", sanitizedAuthHeader);

        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogWarning("No authorization header provided");
            return StatusCode(401, new { message = "Unauthorized - No authorization header provided" });
        }

        if (authHeader == expectedAuth)
        {
            _logger.LogInformation("Authentication successful");
            return StatusCode(status, new { message = $"Returning authorized HTTP status {status}" });
        }

        _logger.LogWarning("Authentication failed - header mismatch");
        return StatusCode(403, new { message = "Forbidden - Invalid authorization header" });
    }

    private IActionResult? ValidateStatusCode(int status)
    {
        if (AllowedStatusCodes.Contains(status))
        {
            return null;
        }

        return BadRequest(new
        {
            message = $"Status code {status} is not valid for GET requests.",
            allowedStatusCodes = AllowedStatusCodes
        });
    }
}
