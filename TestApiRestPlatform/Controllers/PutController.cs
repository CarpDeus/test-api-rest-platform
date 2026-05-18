using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using TestApiRestPlatform.Models;

namespace TestApiRestPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PutController : ControllerBase
{
    private readonly ILogger<PutController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IValidator<ValidationModel> _validationModelValidator;

    public PutController(ILogger<PutController> logger, IConfiguration configuration, IValidator<ValidationModel> validationModelValidator)
    {
        _logger = logger;
        _configuration = configuration;
        _validationModelValidator = validationModelValidator;
    }

    /// <summary>
    /// Returns the requested HTTP status code.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    [HttpPut("{status}")]
    public IActionResult PutStatus(int status)
    {
        _logger.LogInformation("PUT request received for status: {Status}", status);
        return StatusCode(status, new { message = $"Returning HTTP status {status}" });
    }

    /// <summary>
    /// Validates the supplied Authorization header value.
    /// </summary>
    /// <param name="authorization">The Authorization header value expected by the API.</param>
    [HttpPut("authenticate")]
    public IActionResult Authenticate([FromHeader(Name = "Authorization")] string? authorization)
    {
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
            return Ok(new { message = "Authentication successful" });
        }

        _logger.LogWarning("Authentication failed - header mismatch");
        return StatusCode(403, new { message = "Forbidden - Invalid authorization header" });
    }

    /// <summary>
    /// Validates the request body using FluentValidation rules.
    /// </summary>
    /// <param name="model">The payload to validate.</param>
    [HttpPut("validate")]
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
}
