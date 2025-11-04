# Publishing Guide - OpenApiTests

This document describes the process of publishing OpenApiTests to NuGet.org.

## Prerequisites

1. **NuGet.org Account**
   - Create account at https://www.nuget.org/
   - Get API Key at https://www.nuget.org/account/apikeys

2. **.NET SDK 9.0+**
   ```bash
   dotnet --version
   ```

## Publishing Process

### 1. Update Version

Edit `OpenApiTests.csproj` and update the version number:

```xml
<Version>1.0.1</Version>
```

Follow [Semantic Versioning](https://semver.org/):
- **Major** (1.0.0 → 2.0.0): Breaking changes
- **Minor** (1.0.0 → 1.1.0): New backwards-compatible features
- **Patch** (1.0.0 → 1.0.1): Bug fixes

### 2. Update CHANGELOG.md

Add changes to the corresponding version:

```markdown
## [1.0.1] - 2025-11-04

### Fixed
- Fixed bug X
- Improved performance Y

### Added
- New feature Z
```

### 3. Commit and Tag

```bash
# Commit changes
git add .
git commit -m "chore: release version 1.0.1"

# Create tag
git tag -a v1.0.1 -m "Release version 1.0.1"

# Push with tags
git push origin main --tags
```

### 4. Build and Test

```bash
# Clean previous builds
dotnet clean
rm -rf bin/ obj/

# Build in Release mode
dotnet build -c Release

# Test locally
dotnet run -c Release -- --help

# Create package
dotnet pack -c Release
```

The package will be created at: `bin/Release/openapi-tests.{VERSION}.nupkg`

### 5. Test Local Installation

```bash
# Uninstall previous version (if exists)
dotnet tool uninstall --global openapi-tests

# Install from local folder
dotnet tool install --global --add-source ./bin/Release openapi-tests

# Test the tool
dotnet openapi-tests --help
dotnet openapi-tests --generate --api-url http://localhost:5000
```

### 6. Publish to NuGet.org

```bash
# Configure API Key (only once)
dotnet nuget push bin/Release/OpenApiTests.*.nupkg \
  --api-key YOUR_API_KEY_HERE \
  --source https://api.nuget.org/v3/index.json
```

**⚠️ IMPORTANT**: 
- Store your API Key in a secure location
- Never commit the API Key to the repository
- Use environment variables or GitHub Secrets for CI/CD

### 7. Verify Publication

1. Go to https://www.nuget.org/packages/openapi-tests
2. Wait a few minutes for indexing
3. Test global installation:
   ```bash
   dotnet tool install --global openapi-tests
   ```

## Automated Publishing (CI/CD)

### GitHub Actions

Crie `.github/workflows/publish.yml`:

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release --no-restore
    
    - name: Pack
      run: dotnet pack -c Release --no-build
    
    - name: Publish to NuGet
      run: dotnet nuget push bin/Release/*.nupkg \
           --api-key ${{ secrets.NUGET_API_KEY }} \
           --source https://api.nuget.org/v3/index.json \
           --skip-duplicate
```

Configure the `NUGET_API_KEY` secret on GitHub:
- Settings → Secrets and variables → Actions → New repository secret

## Pre-release Versioning

For beta/alpha versions:

```xml
<Version>1.1.0-beta.1</Version>
```

Publish:
```bash
dotnet pack -c Release
dotnet nuget push bin/Release/*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

Install beta version:
```bash
dotnet tool install --global openapi-tests --version 1.1.0-beta.1
```

## Remove Package from NuGet

**⚠️ Cannot be permanently deleted**

You can "unlist" (hide) a version:
1. Go to https://www.nuget.org/packages/openapi-tests
2. Login → Manage Package → Unlist

"Unlisted" versions don't appear in searches but can still be installed if you specify the exact version.

## Publishing Checklist

- [ ] Version updated in `OpenApiTests.csproj`
- [ ] CHANGELOG.md updated
- [ ] Code compiled and tested
- [ ] Local tests passing
- [ ] Commit and tag created
- [ ] Package tested locally
- [ ] Documentation updated (README.md)
- [ ] Published to NuGet.org
- [ ] Verified on nuget.org
- [ ] Global installation tested

## Support

For publishing issues:
- Official documentation: https://docs.microsoft.com/nuget/
- NuGet Gallery: https://www.nuget.org/
- GitHub Issues: https://github.com/embrapa/openapitests/issues
