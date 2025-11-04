using OpenApiTests.Services;

namespace OpenApiTests;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var options = CommandLineOptions.Parse(args);

        if (options.ShowHelp)
        {
            CommandLineOptions.DisplayHelp();
            return 0;
        }

        var apiUrl = options.ApiUrl;
        var httpFilePath = options.HttpFilePath;
        var generateMode = options.Generate;
        
        Console.WriteLine("OpenAPI Contract Tests");
        Console.WriteLine($"API URL: {apiUrl}");
        Console.WriteLine($"HTTP File: {httpFilePath}");
        Console.WriteLine();
        
        try
        {
            // Generate mode
            if (generateMode)
            {
                Console.WriteLine("Mode: GENERATE endpoints.http file");
                Console.WriteLine();
                
                Console.WriteLine("Downloading OpenAPI specification...");
                var openApiSpecUrl = $"{apiUrl}{options.OpenApiPath}";
                var openApiSpec = await DownloadOpenApiSpec(openApiSpecUrl);
                
                Console.WriteLine("Generating HTTP file...");
                var generator = new HttpFileGenerator();
                var httpContent = generator.GenerateHttpFile(openApiSpec, apiUrl);
                
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(httpFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Save file
                await File.WriteAllTextAsync(httpFilePath, httpContent);
                
                Console.WriteLine();
                Console.WriteLine($"File generated: {Path.GetFullPath(httpFilePath)}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Edit the file with real IDs from your database");
                Console.WriteLine("  2. Run: openapi-tests");
                
                return 0;
            }
            
            // Normal test execution mode
            Console.WriteLine("Loading tests from HTTP file...");
            var parser = new HttpFileParser();
            var testCases = parser.ParseHttpFile(httpFilePath);
            Console.WriteLine($"Test cases loaded: {testCases.Count}");
            
            if (testCases.Count == 0)
            {
                Console.WriteLine("No tests found!");
                Console.WriteLine();
                Console.WriteLine("Tip: Run 'openapi-tests --generate' to generate the initial file");
                return 1;
            }
            
            Console.WriteLine($"Running {testCases.Count} tests...");
            var validator = new ContractValidator(apiUrl);
            var results = await validator.ExecuteTests(testCases);
            
            Console.WriteLine("Generating report...");
            var reportGenerator = new ReportGenerator();
            await reportGenerator.GenerateReport(results, "Reports");
            
            var passed = results.Count(r => r.Passed);
            var failed = results.Count(r => !r.Passed);
            
            Console.WriteLine();
            Console.WriteLine("TEST SUMMARY");
            Console.WriteLine($"Passed: {passed}");
            Console.WriteLine($"Failed: {failed}");
            Console.WriteLine($"Total: {results.Count}");
            
            validator.Dispose();
            
            // Upload reports to API (only in container environment)
            var uploadApiUrl = Environment.GetEnvironmentVariable("API_UPLOAD_URL");
            if (!string.IsNullOrEmpty(uploadApiUrl))
            {
                Console.WriteLine();
                Console.WriteLine("Uploading reports to API...");
                await UploadReportsToApi(uploadApiUrl);
            }
            
            return results.All(r => r.Passed) ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Use: openapi-tests --help to see options");
            return 1;
        }
    }
    
    private static async Task<string> DownloadOpenApiSpec(string url)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        
        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to download OpenAPI spec: {response.StatusCode}");
        }
        
        return await response.Content.ReadAsStringAsync();
    }
    
    private static async Task UploadReportsToApi(string apiUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            using var form = new MultipartFormDataContent();
            
            var reportsPath = "Reports";
            var reportFiles = new[]
            {
                "contract-test-report.html",
                "test-results.json",
                "summary.txt"
            };
            
            foreach (var fileName in reportFiles)
            {
                var filePath = Path.Combine(reportsPath, fileName);
                if (File.Exists(filePath))
                {
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    form.Add(fileContent, "files", fileName);
                }
            }
            
            var response = await client.PostAsync(apiUrl, form);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Reports uploaded successfully to API");
            }
            else
            {
                Console.WriteLine($"⚠ Failed to upload reports: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error uploading reports to API: {ex.Message}");
        }
    }
}
