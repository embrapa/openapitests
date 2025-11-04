using OpenApiTests.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TestResult = OpenApiTests.Models.TestResult;

namespace OpenApiTests.Services;

public class ContractValidator
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ContractValidator(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<List<TestResult>> ExecuteTests(List<TestCase> testCases)
    {
        var results = new List<TestResult>();

        foreach (var testCase in testCases)
        {
            var result = await ExecuteTest(testCase);
            results.Add(result);
        }

        return results;
    }

    public async Task<TestResult> ExecuteTest(TestCase testCase)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new TestResult
        {
            TestName = testCase.Name,
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            // Executar request
            var response = await ExecuteRequest(testCase);
            
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.ActualStatusCode = response.StatusCode;
            result.ResponseContent = await response.Content.ReadAsStringAsync();

            // Validar status code
            if (testCase.TestType == TestType.StatusCode)
            {
                result.Passed = testCase.ExpectedStatusCodes.Contains(response.StatusCode);
                result.ExpectedStatusCode = testCase.ExpectedStatusCodes.FirstOrDefault();
                
                if (!result.Passed)
                {
                    result.ErrorMessage = $"Expected status codes: {string.Join(", ", testCase.ExpectedStatusCodes)}, but got: {response.StatusCode}";
                }
            }
            
            // Validar schema
            if (testCase.TestType == TestType.Schema && response.IsSuccessStatusCode)
            {
                result.Passed = await ValidateResponseSchema(result.ResponseContent, testCase.ExpectedSchema);
                
                if (!result.Passed)
                {
                    result.ErrorMessage = "Response schema validation failed";
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Passed = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<HttpResponseMessage> ExecuteRequest(TestCase testCase)
    {
        var request = new HttpRequestMessage(new HttpMethod(testCase.Method), testCase.Path);

        // Adicionar body se necessário
        if (!string.IsNullOrEmpty(testCase.RequestBody))
        {
            request.Content = new StringContent(testCase.RequestBody, System.Text.Encoding.UTF8, "application/json");
        }

        // Adicionar headers (exceto Content-Type que já é configurado no StringContent)
        foreach (var header in testCase.Headers)
        {
            // Ignorar Content-Type pois já está configurado no HttpContent
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                request.Headers.Add(header.Key, header.Value);
            }
            catch (InvalidOperationException)
            {
                // Se falhar ao adicionar no request.Headers, tentar no content.Headers
                if (request.Content != null)
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return await _httpClient.SendAsync(request);
    }

    private Task<bool> ValidateResponseSchema(string? responseContent, string? expectedSchema)
    {
        if (string.IsNullOrEmpty(responseContent) || string.IsNullOrEmpty(expectedSchema))
        {
            return Task.FromResult(true); // Skip validation if no content or schema
        }

        try
        {
            // Validação básica: verificar se é JSON válido
            using var document = JsonDocument.Parse(responseContent);
            return Task.FromResult(true); // Se chegou até aqui, o JSON é válido
        }
        catch (JsonException)
        {
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}