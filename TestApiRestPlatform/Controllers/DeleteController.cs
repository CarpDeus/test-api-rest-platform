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
    public IActionResult Validate([FromBody] ValidationModel model)
    {
        _logger.LogInformation("Validate request received");

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
            
            _logger.LogWarning("Validation failed: {Errors}", errors);
            return BadRequest(new { message = "Validation failed", errors });
        }

        _logger.LogInformation("Validation successful");
        return Ok(new { message = "Validation successful", data = model });
    }
}
