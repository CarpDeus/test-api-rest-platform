using Microsoft.AspNetCore.Mvc;
using TestApiRestPlatform.Models;

namespace TestApiRestPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeleteController : ControllerBase
{
    private readonly ILogger<DeleteController> _logger;
    private readonly IConfiguration _configuration;

    public DeleteController(ILogger<DeleteController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpDelete("{status}")]
    public IActionResult DeleteStatus(int status)
    {
        _logger.LogInformation("DELETE request received for status: {Status}", status);
        return StatusCode(status, new { message = $"Returning HTTP status {status}" });
    }

    [HttpDelete("authenticate")]
    public IActionResult Authenticate()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var expectedAuth = _configuration["Authentication:ExpectedAuthHeader"];

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

    [HttpDelete("validate")]
    public IActionResult Validate([FromQuery] string? name, [FromQuery] string? email)
    {
        _logger.LogInformation("Validate request received");

        var model = new ValidationModel { Name = name ?? string.Empty, Email = email ?? string.Empty };
        
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Validation failed - name is required");
            return BadRequest(new { message = "Validation failed", errors = new { Name = new[] { "The Name field is required." } } });
        }
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Validation failed - email is required");
            return BadRequest(new { message = "Validation failed", errors = new { Email = new[] { "The Email field is required." } } });
        }
        
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            _logger.LogWarning("Validation failed - invalid email");
            return BadRequest(new { message = "Validation failed", errors = new { Email = new[] { "The Email field is not a valid e-mail address." } } });
        }

        _logger.LogInformation("Validation successful");
        return Ok(new { message = "Validation successful", data = model });
    }
}
