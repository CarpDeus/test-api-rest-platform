using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using TestApiRestPlatform.Models;

namespace TestApiRestPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private static readonly int[] AllowedStatusCodes =
    [
        StatusCodes.Status201Created,
        StatusCodes.Status202Accepted,
        StatusCodes.Status400BadRequest,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status409Conflict,
        StatusCodes.Status422UnprocessableEntity,
        StatusCodes.Status500InternalServerError
    ];

    private readonly ILogger<PostController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IValidator<ValidationModel> _validationModelValidator;

    public PostController(ILogger<PostController> logger, IConfiguration configuration, IValidator<ValidationModel> validationModelValidator)
    {
        _logger = logger;
        _configuration = configuration;
        _validationModelValidator = validationModelValidator;
    }

    /// <summary>
    /// Returns the requested HTTP status code.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <remarks>Allowed POST status codes: 201, 202, 400, 401, 403, 409, 422, 500.</remarks>
    [HttpPost("{status}")]
    public IActionResult PostStatus(int status)
    {
        var statusValidationResult = ValidateStatusCode(status);
        if (statusValidationResult is not null)
        {
            return statusValidationResult;
        }

        _logger.LogInformation("POST request received for status: {Status}", status);
        return StatusCode(status, new { message = $"Returning HTTP status {status}" });
    }

    /// <summary>
    /// Validates the supplied Authorization header value.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="authorization">The Authorization header value expected by the API.</param>
    /// <remarks>Allowed POST status codes: 201, 202, 400, 401, 403, 409, 422, 500.</remarks>
    [HttpPost("authenticate")]
    [HttpPost("authenticate/{status}")]
    [Authorize]
    public IActionResult Authenticate([FromHeader(Name = "Authorization")] string? authorization, int status = StatusCodes.Status200OK)
    {
        var statusValidationResult = ValidateStatusCode(status);
        if (statusValidationResult is not null)
        {
            return statusValidationResult;
        }

        var expectedAuth = Environment.GetEnvironmentVariable("ExpectedAuthHeader")
            ?? _configuration["Authentication:ExpectedAuthHeader"];
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

    /// <summary>
    /// Validates the request body using FluentValidation rules.
    /// </summary>
    /// <param name="model">The payload to validate.</param>
    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidationModel model)
    {
        _logger.LogInformation("Validate request received");

        var validationResult = _validationModelValidator.Validate(model);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );

            _logger.LogWarning("Validation failed: {Errors}", errors);
            return BadRequest(new { message = "Validation failed", errors });
        }

        _logger.LogInformation("Validation successful");
        return Ok(new { message = "Validation successful", data = model });
    }

    private IActionResult? ValidateStatusCode(int status)
    {
        if (AllowedStatusCodes.Contains(status))
        {
            return null;
        }

        return BadRequest(new
        {
            message = $"Status code {status} is not valid for POST requests.",
            allowedStatusCodes = AllowedStatusCodes
        });
    }
}