# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.6] - 2026-05-07

### Changed
- Migrated project target framework from `net9.0` to `net10.0`
- Updated GitHub Actions workflows to use .NET SDK `10.0.x`
- Converted solution file from `.sln` to `.slnx` format

## [1.0.5] - 2025-12-02

### Added
- Implemented `# Expected:` directive to explicitly define expected HTTP status codes
- Support for multiple expected status codes: `# Expected: 200,201,204`
- New status codes inference from test names:
  - `no content` or `204` → 204 NoContent
  - `created` or `201` → 201 Created
  - `accepted` or `202` → 202 Accepted
  - `forbidden` or `403` → 403 Forbidden
  - `conflict` or `409` → 409 Conflict
  - `server error` or `500` → 500 InternalServerError

### Changed
- Refactored status code parsing into dedicated methods: `InferStatusCodesFromName()` and `ParseExpectedStatusCodes()`
- `# Expected:` directive now takes priority over test name inference

## [1.0.4] - 2025-12-02

### Added
- Support for commenting out tests using `#` prefix to disable them temporarily
- Tests can now be disabled by adding `#` at the beginning of each line

### Fixed
- Fixed issue where commented lines with `#` were being incorrectly parsed as headers
- Preserved special comments `# Expected:` for defining expected status codes

### Changed
- Updated `HttpFileParser.SplitRequests()` to ignore lines starting with `#` (except `###` test names and `# Expected:` directives)

## [1.0.3] - 2025-11-25

### Fixed
- Fixed POST requests with JSON arrays returning BadRequest (400) errors
- Fixed HTTP body parser to correctly detect JSON arrays starting with `[`
- Fixed line handling in request body parsing that was removing empty lines
- Improved header/body boundary detection to avoid misinterpreting JSON content as headers

### Changed
- Enhanced `HttpFileParser.ParseRequest()` method with better body detection logic
- Removed `StringSplitOptions.RemoveEmptyEntries` to preserve complete request bodies
- Added `headersComplete` flag for accurate header/body separation

## [1.0.2] - 2025-11-05

### Fixed
- Fixed GitHub Actions publish workflow to correctly extract version from release tag
- Fixed package versioning issue where all packages were generated as 1.0.0
- Added `permissions: contents: write` to publish workflow to fix GitHub Release asset upload error

### Changed
- Workflow now uses dynamic versioning based on release tag number
- Removed 'v' prefix requirement from release tags (now accepts tags like `1.0.2` directly)
- Version number in .csproj is now overridden by release tag during CI/CD builds

## [1.0.1] - 2025-11-05

### Changed
- Replaced "SEGAPI" branding with generic "API Contract Tests" terminology
- Updated report titles to be more generic and reusable
- Updated HTTP file header comments to use generic naming
- GitHub Actions publish workflow now triggers on release publication instead of tag push
- Improved publishing workflow clarity by removing automatic release creation

### Documentation
- Moved CONTRIBUTING.md and PUBLISHING.md to Docs/ folder
- Updated all documentation links to reflect new folder structure
- Added nupkg/ folder to .gitignore

## [1.0.0] - 2025-11-03

### Added
- Initial release as a .NET global tool
- Generate HTTP test files from OpenAPI specifications
- Execute contract tests with full control over test data
- Validate API responses against OpenAPI contracts
- Generate HTML test reports
- Support for custom test scenarios
- Command-line interface with options for generation and execution
- Support for expected status codes in HTTP file comments
- Automatic validation test generation (400, 404 scenarios)

### Features
- OpenAPI/Swagger specification parsing
- HTTP file generation and parsing
- Contract validation engine
- HTML report generation
- Support for GET, POST, PUT, DELETE, PATCH HTTP methods
- Custom headers and request bodies
- Multiple expected status codes per test
