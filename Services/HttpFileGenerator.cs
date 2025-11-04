using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text;

namespace OpenApiTests.Services;

public class HttpFileGenerator
{
    public string GenerateHttpFile(string openApiJson, string baseUrl = "http://localhost:5225")
    {
        var reader = new OpenApiStringReader();
        var document = reader.Read(openApiJson, out var diagnostic);
        
        var httpFile = new StringBuilder();
        
        // Header
        httpFile.AppendLine("### SEGAPI Contract Tests - Manual HTTP Tests");
        httpFile.AppendLine("### Use REST Client extension in VS Code to execute these requests");
        httpFile.AppendLine();
        httpFile.AppendLine($"@baseUrl = {baseUrl}");
        httpFile.AppendLine();
        
        // Agrupar por tags/controladores
        var groupedPaths = document.Paths
            .SelectMany(path => path.Value.Operations.Select(op => new { Path = path.Key, Operation = op.Key, Details = op.Value }))
            .GroupBy(x => x.Details.Tags?.FirstOrDefault()?.Name ?? "General")
            .OrderBy(g => g.Key);
        
        foreach (var group in groupedPaths)
        {
            httpFile.AppendLine($"### === {group.Key.ToUpper()} ENDPOINTS ===");
            httpFile.AppendLine();
            
            foreach (var endpoint in group.OrderBy(x => x.Path))
            {
                GenerateEndpointTests(httpFile, endpoint.Path, endpoint.Operation, endpoint.Details);
                httpFile.AppendLine();
            }
        }
        
        return httpFile.ToString();
    }
    
    private void GenerateEndpointTests(StringBuilder httpFile, string path, OperationType operationType, OpenApiOperation operation)
    {
        var method = operationType.ToString().ToUpper();
        var summary = operation.Summary ?? operation.OperationId ?? $"{method} {path}";
        
        // Teste principal (válido)
        httpFile.AppendLine($"### {summary}");
        httpFile.AppendLine($"{method} {{{{baseUrl}}}}{GenerateValidPath(path, operation)}");
        
        // Headers se necessário
        if (method == "POST" || method == "PUT" || method == "PATCH")
        {
            httpFile.AppendLine("Content-Type: application/json");
            httpFile.AppendLine();
            
            // Body de exemplo
            var requestBody = GenerateRequestBody(operation);
            if (!string.IsNullOrEmpty(requestBody))
            {
                httpFile.AppendLine(requestBody);
            }
        }
        
        httpFile.AppendLine();
        
        // Testes de validação (se há parâmetros)
        if (operation.Parameters.Any(p => p.In == ParameterLocation.Path))
        {
            GenerateValidationTests(httpFile, path, method, operation);
        }
    }
    
    private string GenerateValidPath(string path, OpenApiOperation operation)
    {
        var resultPath = path;
        
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
        {
            var sampleValue = GetSampleValueForParameter(parameter);
            resultPath = resultPath.Replace($"{{{parameter.Name}}}", sampleValue);
        }
        
        return resultPath;
    }
    
    private string GetSampleValueForParameter(OpenApiParameter parameter)
    {
        // Valores de exemplo mais realistas
        return parameter.Schema?.Type?.ToLower() switch
        {
            "integer" => parameter.Name.ToLower() switch
            {
                var name when name.Contains("id") => "1",
                var name when name.Contains("ano") => DateTime.Now.Year.ToString(),
                var name when name.Contains("unidade") => "1",
                var name when name.Contains("projeto") => "1",
                var name when name.Contains("plano") => "1",
                _ => "1"
            },
            "string" => parameter.Name.ToLower() switch
            {
                var name when name.Contains("codigo") => "TEST001",
                var name when name.Contains("data") => DateTime.Now.ToString("yyyy-MM-dd"),
                var name when name.Contains("id") => "1",
                _ => "test-value"
            },
            _ => "1"
        };
    }
    
    private void GenerateValidationTests(StringBuilder httpFile, string path, string method, OpenApiOperation operation)
    {
        var pathParams = operation.Parameters.Where(p => p.In == ParameterLocation.Path).ToList();
        
        if (pathParams.Count == 0) return;
        
        var mainParam = pathParams.First();
        
        // Teste com ID inválido (should return 400)
        httpFile.AppendLine($"### {operation.Summary ?? method} - Invalid (should return 400)");
        var invalidPath = path.Replace($"{{{mainParam.Name}}}", "0");
        httpFile.AppendLine($"{method} {{{{baseUrl}}}}{invalidPath}");
        httpFile.AppendLine();
        
        // Teste com ID não encontrado (should return 404)
        httpFile.AppendLine($"### {operation.Summary ?? method} - Not Found (should return 404)");
        var notFoundPath = path.Replace($"{{{mainParam.Name}}}", "999999");
        httpFile.AppendLine($"{method} {{{{baseUrl}}}}{notFoundPath}");
        httpFile.AppendLine();
    }
    
    private string GenerateRequestBody(OpenApiOperation operation)
    {
        var requestBody = operation.RequestBody?.Content?.FirstOrDefault().Value?.Schema;
        if (requestBody == null) return "";
        
        // Gerar JSON de exemplo baseado no schema
        return GenerateJsonExample(requestBody);
    }
    
    private string GenerateJsonExample(OpenApiSchema schema)
    {
        if (schema.Reference != null)
        {
            // Para referências, criar exemplo básico
            return "{\n  // TODO: Ajustar payload conforme necessário\n}";
        }
        
        var json = new StringBuilder();
        json.AppendLine("{");
        
        foreach (var property in schema.Properties?.Take(5) ?? new Dictionary<string, OpenApiSchema>())
        {
            var value = property.Value.Type?.ToLower() switch
            {
                "integer" => "1",
                "number" => "1.0",
                "boolean" => "true",
                "string" => $"\"{GetSampleStringValue(property.Key)}\"",
                "array" => "[1]",
                _ => "null"
            };
            
            json.AppendLine($"  \"{property.Key}\": {value},");
        }
        
        if (schema.Properties?.Any() == true)
        {
            json.Length -= 2; // Remove última vírgula
            json.AppendLine();
        }
        
        json.Append("}");
        
        return json.ToString();
    }
    
    private string GetSampleStringValue(string propertyName)
    {
        return propertyName.ToLower() switch
        {
            var name when name.Contains("id") => "1",
            var name when name.Contains("nome") => "Teste",
            var name when name.Contains("codigo") => "TEST001",
            var name when name.Contains("data") => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            var name when name.Contains("email") => "teste@example.com",
            var name when name.Contains("descricao") => "Descrição de teste",
            _ => "valor-teste"
        };
    }
}