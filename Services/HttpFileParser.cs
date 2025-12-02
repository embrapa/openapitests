using OpenApiTests.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace OpenApiTests.Services;

public class HttpFileParser
{
    public List<TestCase> ParseHttpFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Arquivo HTTP não encontrado: {filePath}");
        }

        var content = File.ReadAllText(filePath);
        var testCases = new List<TestCase>();
        
        // Processar variáveis
        var variables = ParseVariables(content);
        
        // Dividir por requests
        var requests = SplitRequests(content);
        
        foreach (var request in requests)
        {
            var testCase = ParseRequest(request, variables);
            if (testCase != null)
            {
                testCases.Add(testCase);
            }
        }
        
        return testCases;
    }
    
    private Dictionary<string, string> ParseVariables(string content)
    {
        var variables = new Dictionary<string, string>();
        
        // Padrão: @variableName = value
        var variablePattern = @"@(\w+)\s*=\s*(.+)";
        var matches = Regex.Matches(content, variablePattern);
        
        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value;
            var value = match.Groups[2].Value.Trim();
            variables[name] = value;
        }
        
        return variables;
    }
    
    private List<string> SplitRequests(string content)
    {
        var requests = new List<string>();
        
        // Dividir por linhas que começam com ### (comentários de teste)
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var currentRequest = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Ignorar variáveis e comentários de seção
            if (trimmedLine.StartsWith("@") || 
                trimmedLine.StartsWith("### API Contract Tests") ||
                trimmedLine.StartsWith("### Use REST") ||
                string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }
            
            // Ignorar linhas comentadas com # (exceto ### que define teste e # Expected: que define status)
            if (trimmedLine.StartsWith("#") && 
                !trimmedLine.StartsWith("###") && 
                !trimmedLine.ToLower().Contains("expected:"))
            {
                continue;
            }
            
            // Nova seção de teste
            if (trimmedLine.StartsWith("###"))
            {
                // Salvar request anterior se existir
                if (currentRequest.Count > 0)
                {
                    requests.Add(string.Join("\n", currentRequest));
                    currentRequest.Clear();
                }
            }
            
            currentRequest.Add(line);
        }
        
        // Adicionar último request
        if (currentRequest.Count > 0)
        {
            requests.Add(string.Join("\n", currentRequest));
        }
        
        return requests;
    }
    
    private TestCase? ParseRequest(string requestText, Dictionary<string, string> variables)
    {
        var lines = requestText.Split('\n');
        
        string? testName = null;
        string? method = null;
        string? url = null;
        var headers = new Dictionary<string, string>();
        string? body = null;
        var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
        
        var bodyLines = new List<string>();
        bool inBody = false;
        bool headersComplete = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines before body starts
            if (string.IsNullOrWhiteSpace(trimmedLine) && !inBody)
            {
                continue;
            }
            
            // Nome do teste (comentário ###)
            if (trimmedLine.StartsWith("###") && !trimmedLine.Contains("API Contract Tests") && !trimmedLine.Contains("Use REST"))
            {
                testName = trimmedLine.Substring(3).Trim();
                
                // Inferir status esperado do nome do teste (será sobrescrito se houver # Expected:)
                expectedStatusCodes = InferStatusCodesFromName(testName);
            }
            // Expected status comment - # Expected: 200 or # Expected: 200,201,204
            else if (trimmedLine.StartsWith("#") && trimmedLine.ToLower().Contains("expected:"))
            {
                var parsedCodes = ParseExpectedStatusCodes(trimmedLine);
                if (parsedCodes.Count > 0)
                {
                    expectedStatusCodes = parsedCodes;
                }
            }
            // Request line (GET, POST, etc.)
            else if (Regex.IsMatch(trimmedLine, @"^(GET|POST|PUT|DELETE|PATCH)\s+"))
            {
                var parts = trimmedLine.Split(' ', 2);
                method = parts[0];
                url = parts.Length > 1 ? parts[1] : "";
                
                // Substituir variáveis
                foreach (var variable in variables)
                {
                    url = url.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                }
                headersComplete = false;
            }
            // Headers (after method, before body)
            else if (!inBody && !headersComplete && !string.IsNullOrEmpty(method) && trimmedLine.Contains(":") && !trimmedLine.StartsWith("{") && !trimmedLine.StartsWith("["))
            {
                var colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex > 0)
                {
                    var headerName = trimmedLine.Substring(0, colonIndex).Trim();
                    var headerValue = trimmedLine.Substring(colonIndex + 1).Trim();
                    
                    // Validate it's a header (not part of JSON)
                    if (!string.IsNullOrWhiteSpace(headerName) && !headerName.Contains("{") && !headerName.Contains("["))
                    {
                        headers[headerName] = headerValue;
                    }
                }
            }
            // Body starts (JSON object or array)
            else if (!inBody && !string.IsNullOrEmpty(method) && (trimmedLine.StartsWith("{") || trimmedLine.StartsWith("[")))
            {
                inBody = true;
                headersComplete = true;
                bodyLines.Add(line);
            }
            // Body continuation
            else if (inBody)
            {
                bodyLines.Add(line);
            }
        }
        
        // Consolidar body
        if (bodyLines.Count > 0)
        {
            body = string.Join("\n", bodyLines).Trim();
        }
        
        // Validar se temos informações mínimas
        if (string.IsNullOrEmpty(method) || string.IsNullOrEmpty(url))
        {
            return null;
        }
        
        // Gerar nome do teste se não especificado
        if (string.IsNullOrEmpty(testName))
        {
            var pathPart = url.Split('?')[0].Replace("/", "_");
            testName = $"{method}_{pathPart}_Should_Return_SuccessStatusCode";
        }
        
        return new TestCase
        {
            Name = SanitizeTestName(testName),
            Method = method,
            Path = url,
            ExpectedStatusCodes = expectedStatusCodes,
            TestType = TestType.StatusCode,
            Headers = headers,
            RequestBody = body
        };
    }
    
    private List<HttpStatusCode> InferStatusCodesFromName(string testName)
    {
        var lowerName = testName.ToLower();
        
        // Check for specific status codes in name
        if (lowerName.Contains("invalid") || lowerName.Contains("400") || lowerName.Contains("bad request"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.BadRequest };
        }
        if (lowerName.Contains("not found") || lowerName.Contains("404"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.NotFound };
        }
        if (lowerName.Contains("unauthorized") || lowerName.Contains("401"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.Unauthorized };
        }
        if (lowerName.Contains("forbidden") || lowerName.Contains("403"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.Forbidden };
        }
        if (lowerName.Contains("no content") || lowerName.Contains("204"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.NoContent };
        }
        if (lowerName.Contains("created") || lowerName.Contains("201"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.Created };
        }
        if (lowerName.Contains("accepted") || lowerName.Contains("202"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.Accepted };
        }
        if (lowerName.Contains("conflict") || lowerName.Contains("409"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.Conflict };
        }
        if (lowerName.Contains("server error") || lowerName.Contains("500"))
        {
            return new List<HttpStatusCode> { HttpStatusCode.InternalServerError };
        }
        
        // Default to 200 OK
        return new List<HttpStatusCode> { HttpStatusCode.OK };
    }
    
    private List<HttpStatusCode> ParseExpectedStatusCodes(string line)
    {
        var statusCodes = new List<HttpStatusCode>();
        
        // Extract the part after "Expected:"
        var colonIndex = line.ToLower().IndexOf("expected:");
        if (colonIndex == -1) return statusCodes;
        
        var statusPart = line.Substring(colonIndex + 9).Trim();
        
        // Split by comma for multiple status codes
        var codes = statusPart.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var code in codes)
        {
            var trimmedCode = code.Trim();
            
            // Try to parse as integer
            if (int.TryParse(trimmedCode, out int statusInt))
            {
                if (Enum.IsDefined(typeof(HttpStatusCode), statusInt))
                {
                    statusCodes.Add((HttpStatusCode)statusInt);
                }
            }
            // Try to parse as status name
            else if (Enum.TryParse<HttpStatusCode>(trimmedCode.Replace(" ", ""), true, out var statusEnum))
            {
                statusCodes.Add(statusEnum);
            }
        }
        
        return statusCodes;
    }
    
    private string SanitizeTestName(string name)
    {
        // Remover caracteres especiais e espaços
        return Regex.Replace(name, @"[^\w\s-]", "")
                   .Replace(" ", "_")
                   .Replace("-", "_");
    }
}