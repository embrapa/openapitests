# openapi-tests


[![NuGet](https://img.shields.io/nuget/v/openapi-tests.svg)](https://www.nuget.org/packages/openapi-tests/)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET global tool for generating and executing HTTP-based contract tests from OpenAPI specifications.

## 🎯 Features

- ✅ Generate HTTP test files automatically from OpenAPI/Swagger specifications

- ✅ Execute contract tests with full control over test data

- ✅ A .NET global tool for generating and executing HTTP-based contract tests from OpenAPI specifications.

- ✅ Validate API responses against OpenAPI contracts

- ✅ Generate beautiful HTML test reports

- ✅ Support for custom test scenarios with real data

- ✅ Works with any REST API that provides an OpenAPI specification


## 📦 Installation


Install as a global .NET tool:



```bash

dotnet tool install --global openapi-tests

```

Update to the latest version:


```bash
dotnet tool update --global openapi-tests

```

Uninstall:



```bash

dotnet tool uninstall --global openapi-tests

```


## 🚀 Quick Start

### 1. Generate HTTP Test File

Generate an `endpoints.http` file from your API's OpenAPI specification:
```bash

dotnet openapi-tests --generate --api-url http://localhost:5000 --http-file TestFiles/endpoints.http

```


### 2. Customize Your Tests

Edit the generated `TestFiles/endpoints.http` file with real IDs and data if necessary:


```http

### Get User by ID (Valid)

GET {{baseUrl}}/api/users/123

### Get User by ID (Not Found - 404)

GET {{baseUrl}}/api/users/999999

# Expected: 404

### Create New User

POST {{baseUrl}}/api/users## 

Content-Type: application/json

{

  "name": "John Doe",### 1. Generate HTTP Test Filecd Tests

  "email": "john@example.com"

}

```

### 3. Run Tests

Execute the contract tests:


```bash

dotnet openapi-tests --api-url http://localhost:5000 --http-file TestFiles/endpoints.http --output Reports

```


### 4. View Results

Open the generated HTML report at `Reports/contract-test-report.html` or in the folder you specified with the `--output` parameter.

### Options

This will:

- `--generate` or `-g`: Generate HTTP file from OpenAPI specification

- `--api-url <url>`: API base URL (default: `http://localhost:5225`)

- `--openapi-path <path>`: Path to OpenAPI spec (default: `/openapi/v1.json`)

- `--http-file <path>`: Path to HTTP file (default: `TestFiles/endpoints.http`)

- `--output <path>`: Output path for reports (default: `Reports`)

- `--help` or `-h`: Show help information

## ✏️ HTTP File Format

The HTTP file format follows the standard REST Client format:

```http
### Get User by ID (Valid)
### Test NameGET {{baseUrl}}/api/users/123

METHOD {{baseUrl}}/path

Header: Valuedotnet tool uninstall --global OpenApiTestscd Tests

Content-Type: application/json

### Get User by ID (Not Found - 404)

{

  "body": "content"GET {{baseUrl}}/api/users/999999```dotnet run

}

# Expected: 404

### Another Test

GET {{baseUrl}}/another-path

# Expected: 200

# This is a comment### Create New User

POST {{baseUrl}}/api/users


```

### Special Comments

The system automatically infers the expected HTTP status from the test name:

- `# Expected: <status>` - Define expected HTTP status code

- `# Expected: <status1>,<status2>` - Multiple acceptable status codes### Ver Resultados

- Comments starting with `#` are ignored during execution


## 📊 Report Features


The generated HTML report includes:


- ✅ Test execution summary (passed/failed/total)

- ✅ Detailed test results with request/response data

- ✅ Error messages and validation failures

- ✅ Execution time for each test

- ✅ Color-coded results (green/red)Run tests against staging environment:

- ✅ Expandable request/response bodies

- ✅ HTTP status code validation

- ✅ OpenAPI contract validation results


## 🔍 Contract Validation

OpenApiTests validates:

- ✅ HTTP status codes match OpenAPI specification

- ✅ Response content types are correct

- ✅ Response schemas match OpenAPI definitions

- ✅ Required fields are present

- ✅ Data types are correct

- ✅ Enum values are validThe HTTP file format follows the standard REST Client format:

- ✅ Array items match expected schema


## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository

2. Create your feature branch (`git checkout -b feature/AmazingFeature`)

3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)

4. Push to the branch (`git push origin feature/AmazingFeature`)

5. Open a Pull Request

See [CONTRIBUTING.md](Docs/CONTRIBUTING.md) for detailed guidelines.


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏢 About

## 💡 Best Practices

Developed by [Embrapa](https://www.embrapa.br/) - Brazilian Agricultural Research Corporation


## 🐛 Issues

If you encounter any issues or have suggestions, please [open an issue](https://github.com/embrapa/openapitests/issues) on GitHub.

## 📚 Documentation

For more detailed documentation, see:

- [Contributing Guidelines](Docs/CONTRIBUTING.md)

- [Publishing Guide](Docs/PUBLISHING.md)

- [Command Reference](COMMANDS.md)

- [Changelog](CHANGELOG.md)


## ⭐ Support

If you find this tool helpful, please consider:

- Giving it a star on GitHubGET

- Sharing it with others

- Contributing to the project

- Reporting bugs and suggesting features

## 🔗 Links

- **NuGet Package**: https://www.nuget.org/packages/OpenApiTests

- **GitHub Repository**: https://github.com/embrapa/openapitests

- **Issue Tracker**: https://github.com/embrapa/openapitests/issues

- **Discussions**: https://github.com/embrapa/openapitests/discussionsThe


