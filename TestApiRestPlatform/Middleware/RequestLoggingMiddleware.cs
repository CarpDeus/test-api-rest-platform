using System.Text;
using TestApiRestPlatform.Logging;

namespace TestApiRestPlatform.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Enable buffering so we can read the body multiple times
        context.Request.EnableBuffering();

        var requestLog = new ApiRequestLog
        {
            Endpoint = context.Request.Path,
            HttpMethod = context.Request.Method,
            SourceIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Capture headers (exclude sensitive ones or sanitize)
        foreach (var header in context.Request.Headers)
        {
            // Sanitize authorization header
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = header.Value.ToString();
                requestLog.Headers[header.Key] = value.Replace("\n", "").Replace("\r", "");
            }
            else
            {
                requestLog.Headers[header.Key] = header.Value.ToString();
            }
        }

        // Capture query parameters
        foreach (var query in context.Request.Query)
        {
            requestLog.QueryParameters[query.Key] = query.Value.ToString();
        }

        // Capture request body
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            try
            {
                context.Request.Body.Position = 0;
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                
                requestLog.RequestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
            }
        }

        // Capture response status code
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            requestLog.StatusCode = context.Response.StatusCode;
        }
        finally
        {
            // Log the request with structured data
            _logger.LogInformation(
                "API Request: {Endpoint} {HttpMethod} from {SourceIpAddress} - Status: {StatusCode}. " +
                "Headers: {@Headers}, QueryParams: {@QueryParameters}, Body: {RequestBody}",
                requestLog.Endpoint,
                requestLog.HttpMethod,
                requestLog.SourceIpAddress,
                requestLog.StatusCode,
                requestLog.Headers,
                requestLog.QueryParameters,
                requestLog.RequestBody ?? "(none)");

            // Copy the response back
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
