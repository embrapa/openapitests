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
                
                // Inferir status esperado do nome do teste
                if (testName.ToLower().Contains("invalid") || testName.ToLower().Contains("400"))
                {
                    expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.BadRequest };
                }
                else if (testName.ToLower().Contains("not found") || testName.ToLower().Contains("404"))
                {
                    expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
                }
                else if (testName.ToLower().Contains("unauthorized") || testName.ToLower().Contains("401"))
                {
                    expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Unauthorized };
                }
            }
            // Expected status comment
            else if (trimmedLine.StartsWith("#") && trimmedLine.ToLower().Contains("expected:"))
            {
                // Parse expected status codes
                continue;
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
    
    private string SanitizeTestName(string name)
    {
        // Remover caracteres especiais e espaços
        return Regex.Replace(name, @"[^\w\s-]", "")
                   .Replace(" ", "_")
                   .Replace("-", "_");
    }
}