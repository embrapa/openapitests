using System.Net;

namespace OpenApiTests.Models;

public class TestCase
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<HttpStatusCode> ExpectedStatusCodes { get; set; } = new();
    public string? ExpectedSchema { get; set; }
    public TestType TestType { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public string? RequestBody { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public enum TestType
{
    StatusCode,
    Schema,
    Performance,
    Security
}