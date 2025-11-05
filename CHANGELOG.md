# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
