using Microsoft.AspNetCore.Mvc;
using TestApiRestPlatform.Models;

namespace TestApiRestPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GetController : ControllerBase
{
    private readonly ILogger<GetController> _logger;
    private readonly IConfiguration _configuration;

    public GetController(ILogger<GetController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("{status}")]
    public IActionResult GetStatus(int status)
    {
        _logger.LogInformation("GET request received for status: {Status}", status);
        return StatusCode(status, new { message = $"Returning HTTP status {status}" });
    }

    [HttpGet("authenticate")]
    public IActionResult Authenticate()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var expectedAuth = _configuration["Authentication:ExpectedAuthHeader"];

        _logger.LogInformation("Authenticate request received with auth header: {AuthHeader}", authHeader);

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

    [HttpGet("validate")]
    public IActionResult Validate([FromBody] ValidationModel model)
    {
        _logger.LogInformation("Validate request received");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validation failed");
            return BadRequest(new { message = "Validation failed", errors = ModelState });
        }

        _logger.LogInformation("Validation successful");
        return Ok(new { message = "Validation successful", data = model });
    }
}
