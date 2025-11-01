# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZZLD_Form is a C# application for managing ZZLD (ЗЗЛД - Заявление за Защита на Лични Данни) form submissions. The project uses Azure Blob Storage for document storage.

## Key Project Information

- **Language**: C# (.NET)
- **Storage**: Azure Blob Storage container `zzld-form` in account `aivideoprocessing.blob.core.windows.net`
- **GitHub Repository**: https://github.com/nuvoleta/ZZLD_Form
- **Main Branch**: `master` (not `main`)

## Git Workflow

This project follows strict Git standards defined in `documentation/GIT_STANDARDS.md`:

### Branch Naming
- Feature branches: `feature/description-of-feature`
- Bug fixes: `fix/description-of-bug`
- Hotfixes: `hotfix/critical-issue-description`

### Commit Message Format
```
<type>(<scope>): <description>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Important Git Rules
- **Never push directly to master branch**
- Always create feature branches from master
- Use `gh pr create` for pull requests
- Squash and merge for clean history
- Delete feature branches after merge

## Development Standards

**See `/documentation/DEVELOPMENT_STANDARDS.md` for complete development guidelines.**

### Testing Requirements
- **TDD Methodology**: Follow Test-Driven Development for all new features
- **Coverage**: Maintain minimum 85% test coverage (enforced with coverlet)
- **Test Types**: Unit tests (xUnit), integration tests, and E2E tests required
- **Test Framework**: xUnit + Moq + FluentAssertions

### Testing Commands
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura /p:Threshold=85

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=E2E

# Build and run tests
dotnet build && dotnet test
```

### Code Quality
- **Nullable Reference Types**: Enable for null safety
- **Code Analysis**: Enable all recommended analyzers, treat warnings as errors
- **Async/Await**: Use for all I/O operations
- **Error Handling**: Consistent error handling with specific exception types
- **Input Validation**: Use FluentValidation for all API inputs

### Security Requirements
- **Secrets Management**: Azure Key Vault for sensitive credentials
- **Input Validation**: FluentValidation for all API inputs
- **Parameterized Queries**: Use EF Core with parameterized queries
- **XSS Prevention**: Sanitize all user inputs
- **HTTPS Only**: Enforce HTTPS for all endpoints
- **GDPR Compliance**: Follow data protection requirements for ZZLD forms

## Architecture Guidelines

### Code Organization
- **Feature-based structure**: Organize by features, not file types
  ```
  Features/
  ├── FormSubmission/
  │   ├── FormSubmissionService.cs
  │   ├── FormSubmissionController.cs
  │   ├── Models/
  │   └── Tests/
  ```
- **Co-located tests**: Test files near the code they test
- **Project Separation**: Separate projects for API, Core, Infrastructure, and Tests
- **Documentation**: Keep documentation in `/documentation/` directory

### Documentation Requirements
- **XML Comments**: Comprehensive XML documentation for all public APIs
- **API Documentation**: Use Swagger/OpenAPI for API documentation
- **Architecture Diagrams**: Maintain C4 model diagrams in `/documentation/`
- **Sprint Updates**: Update system architecture documentation with every sprint

## Azure Blob Storage Integration

- **Container**: `zzld-form`
- **Storage Account**: `aivideoprocessing.blob.core.windows.net`
- **SDK**: Azure.Storage.Blobs NuGet package
- **Authentication**: Use Azure Managed Identity or Azure Key Vault for connection strings
- **Naming Convention**: `{userId}/{formId}/{timestamp}.pdf`
- **Retry Policy**: Use Polly for transient failure handling
- **SAS Tokens**: Use time-limited SAS tokens for secure document access
- **Local Testing**: Use Azurite (Azure Storage Emulator) for local development

### Storage Service Pattern
```csharp
public interface IDocumentStorageService
{
    Task<string> UploadFormAsync(Stream content, string fileName, Dictionary<string, string> metadata);
    Task<Stream> GetFormAsync(string blobName);
    Task<bool> DeleteFormAsync(string blobName);
    Task<IEnumerable<BlobItem>> ListFormsAsync(string userId);
}
```

## Quality Standards

### Pre-commit Checks
- Code formatting with `dotnet format`
- Code analysis with Roslyn analyzers
- Unit test execution
- Secret detection

### Pre-merge Requirements
- All tests pass (unit, integration, E2E)
- Minimum 85% code coverage maintained
- Release build completes successfully without warnings
- No high or critical severity vulnerabilities
- Code review approval required

## E2E Testing Strategy

**See `/documentation/E2E_TESTING_STRATEGY.md` for comprehensive testing approach.**

### Test Structure
```
tests/
├── ZZLD_Form.UnitTests/          # Component & function tests
├── ZZLD_Form.IntegrationTests/   # API & service integration tests  
├── ZZLD_Form.E2ETests/           # End-to-end workflow tests
└── TestUtilities/                # Shared test utilities
```

### Key E2E Test Scenarios
- Complete form submission workflow (upload → storage → retrieval)
- Azure Blob Storage integration (upload, download, list, delete)
- Error handling (storage unavailable, invalid data, oversized files)
- Security (authentication, authorization, file sanitization)
- Concurrency and performance testing

## Project Structure

```
ZZLD_Form/
├── documentation/              # Project documentation
│   ├── GIT_STANDARDS.md       # Git workflow and standards
│   ├── DEVELOPMENT_STANDARDS.md  # C# development guidelines
│   ├── E2E_TESTING_STRATEGY.md   # Testing strategy and patterns
│   └── Samples/               # Original sample documents
├── form/                      # Form templates and documents
│   ├── ZZLD_Form.pdf         # Bulgarian form template
│   └── Декларация по ЗЗЛД.docx
└── requirements/              # Project requirements
    └── zzld_form.txt
```

## ZZLD Form-Specific Requirements

### Form Processing
- **Supported Formats**: PDF and DOCX
- **File Size Limit**: Maximum 10MB per submission
- **Metadata**: Store user ID, submission timestamp, and status
- **Status Workflow**: Submitted → Processing → Completed/Rejected
- **Data Retention**: Active forms in hot storage (90 days), then archive to cool storage

### GDPR Compliance
- Follow Bulgarian Personal Data Protection regulations
- Implement proper data retention and deletion policies
- Audit log all access to personal data
- Secure storage with encryption at rest and in transit

## Required NuGet Packages

```xml
<!-- Core packages -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.*" />
<PackageReference Include="Azure.Identity" Version="1.*" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />
<PackageReference Include="Polly" Version="8.*" />
<PackageReference Include="itext" Version="9.3.0" />
<PackageReference Include="itext7.bouncy-castle-adapter" Version="9.3.0" />

<!-- Testing packages -->
<PackageReference Include="xUnit" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="coverlet.collector" Version="6.*" />
```

## PDF Generation

### iText7 Implementation
- **Library**: iText7 9.3.0 for filling existing PDF templates
- **Approach**: Load existing PDF template and overlay text at precise coordinates
- **Font**: Arial Bold TrueType (`/mnt/c/Windows/Fonts/arialbd.ttf`) with IDENTITY_H encoding
- **Cyrillic Support**: TrueType fonts required for Bulgarian text (standard PDF fonts don't support Cyrillic)
- **Template Location**: `templates/ZZLD_Form.pdf`
- **Field Coordinates**: Defined in `requirements/fields.txt` (Y, X coordinates)

### PersonalData Model Fields
```csharp
public class PersonalData
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string EGN { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Community { get; set; }  // ж.к.
    public string Street { get; set; }      // ул.
    public string Number { get; set; }      // N
    public string Block { get; set; }       // бл.
    public string Entrance { get; set; }    // вх.
    public string Floor { get; set; }       // ет.
    public string Apartment { get; set; }   // ап.
    public string PhoneNumber { get; set; } // тел.
}
```

### Field Positioning
- Each field has fixed Y,X coordinates on the PDF template
- Font size: 10pt, black, bold
- Coordinates reference the bottom-left corner of the text
- See `requirements/fields.txt` for complete coordinate mapping

## Important Notes

- Target .NET 8.0
- Follow C# best practices and .NET conventions
- Use Azurite for local Azure Storage testing
- Maintain 85% test coverage from the start
- Never commit secrets or API keys to the repository
- All development follows TDD methodology
- PDF generation fills existing template rather than creating new documents
