namespace OpenApiTests;

public class CommandLineOptions
{
    public bool Generate { get; set; }
    public string ApiUrl { get; set; } = "http://localhost:5225";
    public string HttpFilePath { get; set; } = "TestFiles/endpoints.http";
    public string OutputPath { get; set; } = "Reports";
    public string OpenApiPath { get; set; } = "/openapi/v1.json";
    public bool ShowHelp { get; set; }

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLower();

            switch (arg)
            {
                case "--generate":
                case "-g":
                    options.Generate = true;
                    break;

                case "--api-url":
                    if (i + 1 < args.Length)
                    {
                        options.ApiUrl = args[++i];
                    }
                    break;

                case "--http-file":
                    if (i + 1 < args.Length)
                    {
                        options.HttpFilePath = args[++i];
                    }
                    break;

                case "--output":
                    if (i + 1 < args.Length)
                    {
                        options.OutputPath = args[++i];
                    }
                    break;

                case "--openapi-path":
                    if (i + 1 < args.Length)
                    {
                        options.OpenApiPath = args[++i];
                    }
                    break;

                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
            }
        }

        return options;
    }

    public static void DisplayHelp()
    {
        Console.WriteLine("OpenApiTests - Contract testing tool for OpenAPI specifications");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  openapi-tests [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --generate, -g              Generate HTTP file from OpenAPI specification");
        Console.WriteLine("  --api-url <url>             API base URL (default: http://localhost:5225)");
        Console.WriteLine("  --openapi-path <path>       Path to OpenAPI spec (default: /openapi/v1.json)");
        Console.WriteLine("  --http-file <path>          Path to HTTP file (default: TestFiles/endpoints.http)");
        Console.WriteLine("  --output <path>             Output path for reports (default: Reports)");
        Console.WriteLine("  --help, -h                  Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  openapi-tests --generate --api-url http://localhost:5000");
        Console.WriteLine("  openapi-tests --generate --api-url https://api.com --openapi-path /swagger/v1/swagger.json");
        Console.WriteLine("  openapi-tests --api-url http://localhost:5000 --http-file tests/api.http");
        Console.WriteLine("  openapi-tests");
    }
}
