using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using OpenApiTests.Models;
using System.Net;
using System.Text.Json;

namespace OpenApiTests.Services;

public class OpenApiTestGenerator
{
    public List<TestCase> GenerateTestCases(string openApiJson)
    {
        var reader = new OpenApiStringReader();
        var document = reader.Read(openApiJson, out var diagnostic);
        
        var testCases = new List<TestCase>();
        
        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                // Substituir parâmetros de path por valores padrão
                var testPath = ReplacePathParametersWithDefaults(path.Key, operation.Value);
                
                // Teste básico de status code
                testCases.Add(new TestCase
                {
                    Name = $"{operation.Value.OperationId ?? GenerateOperationId(operation.Key, path.Key)}_Should_Return_SuccessStatusCode",
                    Method = operation.Key.ToString().ToUpper(),
                    Path = testPath,
                    ExpectedStatusCodes = GetExpectedStatusCodes(operation.Value),
                    TestType = TestType.StatusCode
                });
                
                // Teste de schema de response (se há response 200)
                if (operation.Value.Responses.ContainsKey("200"))
                {
                    testCases.Add(new TestCase
                    {
                        Name = $"{operation.Value.OperationId ?? GenerateOperationId(operation.Key, path.Key)}_Should_Return_ValidSchema",
                        Method = operation.Key.ToString().ToUpper(),
                        Path = testPath,
                        ExpectedSchema = GetResponseSchema(operation.Value.Responses["200"]),
                        TestType = TestType.Schema
                    });
                }
                
                // Teste de parâmetros inválidos (se há parâmetros)
                if (operation.Value.Parameters.Any())
                {
                    testCases.AddRange(GenerateInvalidParameterTests(operation.Key, path.Key, operation.Value));
                }
            }
        }
        
        return testCases;
    }
    
    private List<HttpStatusCode> GetExpectedStatusCodes(OpenApiOperation operation)
    {
        return operation.Responses.Keys
            .Where(k => int.TryParse(k, out _))
            .Select(int.Parse)
            .Where(code => code >= 200 && code < 300)
            .Select(code => (HttpStatusCode)code)
            .ToList();
    }
    
    private string? GetResponseSchema(OpenApiResponse response)
    {
        // Extrair schema do response para validação
        if (response.Content?.Any() == true)
        {
            var content = response.Content.First();
            return content.Value?.Schema?.Reference?.Id ?? content.Value?.Schema?.Type;
        }
        return null;
    }
    
    private string GenerateOperationId(OperationType operationType, string path)
    {
        var cleanPath = path.Replace("/", "_").Replace("{", "").Replace("}", "");
        return $"{operationType}_{cleanPath}";
    }
    
    private string ReplacePathParametersWithDefaults(string path, OpenApiOperation operation)
    {
        var resultPath = path;
        
        // Substituir parâmetros de path por valores padrão
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
        {
            var defaultValue = GetDefaultValueForParameter(parameter);
            resultPath = resultPath.Replace($"{{{parameter.Name}}}", defaultValue);
        }
        
        return resultPath;
    }
    
    private string GetDefaultValueForParameter(OpenApiParameter parameter)
    {
        // Valores padrão baseados no tipo do parâmetro
        return parameter.Schema?.Type?.ToLower() switch
        {
            "integer" => "1",
            "number" => "1",
            "string" when parameter.Name.ToLower().Contains("id") => "1",
            "string" when parameter.Name.ToLower().Contains("codigo") => "TEST001",
            "string" when parameter.Name.ToLower().Contains("data") => DateTime.Now.ToString("yyyy-MM-dd"),
            "string" => "test-value",
            _ => "1"
        };
    }
    
    private List<TestCase> GenerateInvalidParameterTests(OperationType operationType, string path, OpenApiOperation operation)
    {
        var invalidTests = new List<TestCase>();
        
        // Para cada parâmetro path, gerar testes com valores inválidos
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
        {
            var invalidValues = new[] { "0", "-1", "abc", "999999" };
            
            foreach (var invalidValue in invalidValues)
            {
                var testPath = path.Replace($"{{{parameter.Name}}}", invalidValue);
                
                invalidTests.Add(new TestCase
                {
                    Name = $"{operation.OperationId ?? GenerateOperationId(operationType, path)}_With_Invalid_{parameter.Name}_{invalidValue}_Should_ReturnBadRequest",
                    Method = operationType.ToString().ToUpper(),
                    Path = testPath,
                    ExpectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.BadRequest, HttpStatusCode.NotFound },
                    TestType = TestType.StatusCode
                });
            }
        }
        
        return invalidTests;
    }
}