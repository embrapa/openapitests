using OpenApiTests.Models;
using System.Text;
using TestResult = OpenApiTests.Models.TestResult;

namespace OpenApiTests.Services;

public class ReportGenerator
{
    public async Task GenerateReport(List<TestResult> results, string outputPath)
    {
        Directory.CreateDirectory(outputPath);
        
        // Gerar relatório HTML
        await GenerateHtmlReport(results, Path.Combine(outputPath, "contract-test-report.html"));
        
        // Gerar relatório JSON
        await GenerateJsonReport(results, Path.Combine(outputPath, "test-results.json"));
        
        // Gerar arquivo de resumo
        await GenerateSummaryReport(results, Path.Combine(outputPath, "summary.txt"));
    }

    private async Task GenerateHtmlReport(List<TestResult> results, string filePath)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <title>API Contract Test Report</title>");
        html.AppendLine("    <meta charset='utf-8'>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("        .header { background: #f5f5f5; padding: 20px; border-radius: 5px; }");
        html.AppendLine("        .summary { margin: 20px 0; }");
        html.AppendLine("        .passed { color: green; font-weight: bold; }");
        html.AppendLine("        .failed { color: red; font-weight: bold; }");
        html.AppendLine("        .test-item { margin: 10px 0; padding: 10px; border: 1px solid #ddd; border-radius: 3px; }");
        html.AppendLine("        .test-name { font-weight: bold; }");
        html.AppendLine("        .test-details { margin-top: 5px; font-size: 0.9em; color: #666; }");
        html.AppendLine("        .error { color: red; margin-top: 5px; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <h1>📊 API Contract Test Report</h1>");
        html.AppendLine($"        <p>Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine("    </div>");
        
        // Summary
        var passed = results.Count(r => r.Passed);
        var failed = results.Count(r => !r.Passed);
        var totalTime = results.Sum(r => r.ExecutionTime.TotalMilliseconds);
        
        html.AppendLine("    <div class='summary'>");
        html.AppendLine("        <h2>📋 Summary</h2>");
        html.AppendLine($"        <p><span class='passed'>✅ Passed: {passed}</span></p>");
        html.AppendLine($"        <p><span class='failed'>❌ Failed: {failed}</span></p>");
        html.AppendLine($"        <p>📊 Total: {results.Count}</p>");
        html.AppendLine($"        <p>⏱️ Total Time: {totalTime:F2}ms</p>");
        html.AppendLine("    </div>");
        
        // Detailed Results
        html.AppendLine("    <h2>🔍 Detailed Results</h2>");
        
        foreach (var result in results.OrderBy(r => r.TestName))
        {
            var statusClass = result.Passed ? "passed" : "failed";
            var statusIcon = result.Passed ? "✅" : "❌";
            
            html.AppendLine("    <div class='test-item'>");
            html.AppendLine($"        <div class='test-name {statusClass}'>{statusIcon} {result.TestName}</div>");
            html.AppendLine("        <div class='test-details'>");
            html.AppendLine($"            Status: {result.ActualStatusCode} | Time: {result.ExecutionTime.TotalMilliseconds:F2}ms");
            html.AppendLine("        </div>");
            
            if (!result.Passed && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                html.AppendLine($"        <div class='error'>Error: {result.ErrorMessage}</div>");
            }
            
            html.AppendLine("    </div>");
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        await File.WriteAllTextAsync(filePath, html.ToString());
    }

    private async Task GenerateJsonReport(List<TestResult> results, string filePath)
    {
        var report = new
        {
            generatedAt = DateTime.UtcNow,
            summary = new
            {
                total = results.Count,
                passed = results.Count(r => r.Passed),
                failed = results.Count(r => !r.Passed),
                totalExecutionTime = results.Sum(r => r.ExecutionTime.TotalMilliseconds)
            },
            results = results.Select(r => new
            {
                testName = r.TestName,
                passed = r.Passed,
                actualStatusCode = r.ActualStatusCode?.ToString(),
                expectedStatusCode = r.ExpectedStatusCode?.ToString(),
                executionTime = r.ExecutionTime.TotalMilliseconds,
                errorMessage = r.ErrorMessage,
                executedAt = r.ExecutedAt
            })
        };

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task GenerateSummaryReport(List<TestResult> results, string filePath)
    {
        var summary = new StringBuilder();
        
        summary.AppendLine("API Contract Tests - Summary Report");
        summary.AppendLine("=" + new string('=', 50));
        summary.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine();
        
        var passed = results.Count(r => r.Passed);
        var failed = results.Count(r => !r.Passed);
        
        summary.AppendLine("RESULTS:");
        summary.AppendLine($"✅ Passed: {passed}");
        summary.AppendLine($"❌ Failed: {failed}");
        summary.AppendLine($"📊 Total: {results.Count}");
        summary.AppendLine($"📈 Success Rate: {(passed * 100.0 / results.Count):F1}%");
        
        if (failed > 0)
        {
            summary.AppendLine();
            summary.AppendLine("FAILED TESTS:");
            foreach (var result in results.Where(r => !r.Passed))
            {
                summary.AppendLine($"❌ {result.TestName}");
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    summary.AppendLine($"   Error: {result.ErrorMessage}");
                }
            }
        }
        
        await File.WriteAllTextAsync(filePath, summary.ToString());
    }
}