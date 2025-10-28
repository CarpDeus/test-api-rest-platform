namespace TestApiRestPlatform.Logging;

public class ApiRequestLog
{
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string? SourceIpAddress { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public string? RequestBody { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
