using System.Net;

namespace OpenApiTests.Models;

public class TestResult
{
    public string TestName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public HttpStatusCode? ActualStatusCode { get; set; }
    public HttpStatusCode? ExpectedStatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseContent { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; set; } = new();
}