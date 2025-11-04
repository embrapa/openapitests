# Contributing to OpenApiTests

Thank you for your interest in contributing to OpenApiTests! We welcome contributions from the community.

## How to Contribute

### Reporting Bugs

If you find a bug, please open an issue on GitHub with:
- A clear title and description
- Steps to reproduce the issue
- Expected vs actual behavior
- Your environment (OS, .NET version, etc.)
- Any relevant logs or error messages

### Suggesting Features

We love new ideas! To suggest a feature:
1. Check if it's already been suggested in existing issues
2. Open a new issue with the "enhancement" label
3. Describe the feature and its use case
4. Explain how it would benefit users

### Submitting Pull Requests

1. **Fork the repository** and create your branch from `main`
   ```bash
   git checkout -b feature/amazing-feature
   ```

2. **Make your changes** following our coding standards:
   - Use meaningful variable and method names
   - Add comments for complex logic
   - Follow C# naming conventions
   - Keep methods focused and single-purpose

3. **Test your changes**
   ```bash
   dotnet build
   dotnet pack
   dotnet tool install --global --add-source ./bin/Release OpenApiTests
   ```

4. **Update documentation** if needed:
   - Update README.md for user-facing changes
   - Update CHANGELOG.md following [Keep a Changelog](https://keepachangelog.com/)
   - Add XML documentation comments to public APIs

5. **Commit your changes** with a clear message:
   ```bash
   git commit -m "feat: add support for OAuth authentication"
   ```

   Use conventional commit messages:
   - `feat:` for new features
   - `fix:` for bug fixes
   - `docs:` for documentation changes
   - `refactor:` for code refactoring
   - `test:` for adding tests
   - `chore:` for maintenance tasks

6. **Push to your fork** and submit a pull request
   ```bash
   git push origin feature/amazing-feature
   ```

7. **Respond to feedback** during the review process

## Development Setup

### Prerequisites
- .NET 9.0 SDK or later
- Git
- A code editor (VS Code, Visual Studio, or Rider)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/openapitests.git
cd openapitests

# Build the project
dotnet build

# Run locally
dotnet run -- --generate --api-url http://localhost:5000

# Create package
dotnet pack -c Release

# Install locally for testing
dotnet tool install --global --add-source ./bin/Release OpenApiTests
```

### Project Structure

```
OpenApiTests/
├── Models/           # Data models (TestCase, TestResult)
├── Services/         # Core services
│   ├── ContractValidator.cs     # API contract validation
│   ├── HttpFileGenerator.cs     # HTTP file generation
│   ├── HttpFileParser.cs        # HTTP file parsing
│   ├── OpenApiTestGenerator.cs  # OpenAPI test generation
│   └── ReportGenerator.cs       # HTML report generation
├── TestFiles/        # Sample test files
├── Program.cs        # Entry point
└── OpenApiTests.csproj
```

## Coding Standards

### C# Conventions
- Use PascalCase for class names and public members
- Use camelCase for local variables and private fields
- Use `_` prefix for private fields
- Add `async` suffix to async methods
- Use `var` when type is obvious

### Code Style
```csharp
// Good
public async Task<TestResult> ExecuteTestAsync(TestCase testCase)
{
    var result = new TestResult
    {
        TestName = testCase.Name,
        Success = false
    };
    
    // Implementation
    return result;
}

// Avoid
public async Task<TestResult> ExecuteTest(TestCase testCase)
{
    TestResult result=new TestResult();
    result.TestName=testCase.Name;
    return result;
}
```

## Code Review Process

All submissions require review. We use GitHub pull requests for this purpose:
1. A maintainer will review your code
2. Address any requested changes
3. Once approved, your PR will be merged

## Community Guidelines

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Follow the [Code of Conduct](CODE_OF_CONDUCT.md)

## Questions?

Feel free to open an issue with the "question" label or reach out to the maintainers.

Thank you for contributing! 🎉
