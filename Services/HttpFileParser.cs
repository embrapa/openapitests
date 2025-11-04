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
                trimmedLine.StartsWith("### SEGAPI") ||
                trimmedLine.StartsWith("### Use REST") ||
                string.IsNullOrWhiteSpace(trimmedLine))
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
        var lines = requestText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        string? testName = null;
        string? method = null;
        string? url = null;
        var headers = new Dictionary<string, string>();
        string? body = null;
        var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
        
        var bodyLines = new List<string>();
        bool inBody = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Nome do teste (comentário ###)
            if (trimmedLine.StartsWith("###") && !trimmedLine.Contains("SEGAPI") && !trimmedLine.Contains("Use REST"))
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
            }
            // Headers
            else if (trimmedLine.Contains(":") && !inBody && !string.IsNullOrEmpty(method))
            {
                var headerParts = trimmedLine.Split(':', 2);
                if (headerParts.Length == 2)
                {
                    headers[headerParts[0].Trim()] = headerParts[1].Trim();
                }
            }
            // Body (JSON)
            else if (!string.IsNullOrEmpty(trimmedLine) && (trimmedLine.StartsWith("{") || inBody))
            {
                inBody = true;
                bodyLines.Add(line);
            }
        }
        
        // Consolidar body
        if (bodyLines.Count > 0)
        {
            body = string.Join("\n", bodyLines);
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