# Development Standards and Guidelines - ZZLD Form Project

## Software Engineering Standards

### Test-Driven Development (TDD)
- **Methodology**: Follow TDD methodology for all new features
- **Coverage Requirement**: Maintain minimum 85% test coverage
- **Test Types**: Unit tests, integration tests, and E2E tests required
- **Component Testing**: All C# classes and services must have corresponding unit tests
- **API Testing**: Integration tests required for all API endpoints
- **Critical Journeys**: E2E tests for all critical user workflows (form submission, document retrieval, status checking)

### Code Quality Standards

#### C# and .NET Standards
- **Target Framework**: .NET 6.0 or later
- **Nullable Reference Types**: Enable nullable reference types for null safety
- **Code Analysis**: Enable all recommended analyzers and treat warnings as errors
- **Async/Await**: Use async/await patterns for all I/O operations
- **Dependency Injection**: Use built-in DI container for service management

#### Code Organization
- **Naming Conventions**: 
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - Prefix interfaces with 'I' (e.g., `IFormService`)
- **File Structure**: Feature-based organization with co-located tests
  ```
  Features/
  ├── FormSubmission/
  │   ├── FormSubmissionService.cs
  │   ├── FormSubmissionController.cs
  │   ├── Models/
  │   └── Tests/
  │       ├── FormSubmissionServiceTests.cs
  │       └── FormSubmissionControllerTests.cs
  ```
- **Project Organization**: Separate projects for API, Core, Infrastructure, and Tests

#### Error Handling
- **Exception Handling**: Use try-catch blocks with specific exception types
- **Logging**: Use ILogger for structured logging with appropriate log levels
- **Custom Exceptions**: Create domain-specific exceptions for business logic errors
- **API Responses**: Consistent error response format with appropriate HTTP status codes
- **Fallback Strategies**: Graceful degradation for Azure Blob Storage failures

### Testing Commands
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura

# Run specific test project
dotnet test tests/ZZLD_Form.UnitTests

# Run integration tests
dotnet test tests/ZZLD_Form.IntegrationTests

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=html
```

## Azure Blob Storage Integration

### Storage Configuration
- **Container Name**: `zzld-form`
- **Storage Account**: `aivideoprocessing.blob.core.windows.net`
- **Authentication**: Use Azure Managed Identity or Azure Key Vault for connection strings
- **SDK**: Azure.Storage.Blobs NuGet package

### Best Practices
- **Naming Conventions**: Use consistent blob naming (e.g., `{userId}/{formId}/{timestamp}.pdf`)
- **Error Handling**: Handle transient failures with retry policies (use Polly)
- **Metadata**: Store form metadata as blob metadata for easier querying
- **SAS Tokens**: Use time-limited SAS tokens for secure document access
- **Lifecycle Management**: Implement blob lifecycle policies for document retention

### Example Service Pattern
```csharp
public interface IDocumentStorageService
{
    Task<string> UploadFormAsync(Stream content, string fileName, Dictionary<string, string> metadata);
    Task<Stream> GetFormAsync(string blobName);
    Task<bool> DeleteFormAsync(string blobName);
    Task<IEnumerable<BlobItem>> ListFormsAsync(string userId);
}
```

## Authentication and Security

### Security Best Practices
- **Input Validation**: Use FluentValidation for all input models
- **SQL Injection Prevention**: Use EF Core with parameterized queries
- **XSS Prevention**: Sanitize all user inputs before storage
- **CORS Configuration**: Restrict CORS to specific allowed origins
- **Secrets Management**: Azure Key Vault for all sensitive credentials
- **HTTPS Only**: Enforce HTTPS for all endpoints
- **Rate Limiting**: Implement rate limiting on API endpoints

### Data Protection
- **Personal Data**: Follow GDPR compliance for ZZLD form data
- **Encryption**: 
  - Data at rest: Azure Blob Storage encryption
  - Data in transit: TLS 1.2 or higher
- **Access Control**: Implement proper authorization checks
- **Audit Logging**: Log all access to personal data

## Deployment Standards

### Build Configuration
```bash
# Build for development
dotnet build --configuration Debug

# Build for production
dotnet build --configuration Release

# Publish application
dotnet publish --configuration Release --output ./publish
```

### Container Standards (if using Docker)
- **Base Image**: Use official Microsoft .NET runtime images
- **Multi-stage Builds**: Separate build and runtime stages
- **Security**: Run as non-root user
- **Health Checks**: Implement health check endpoints

### Deployment Environments
- **Development**: Local development environment
- **Staging**: Azure App Service or Container Apps (staging slot)
- **Production**: Azure App Service or Container Apps (production slot)
- **Testing Requirements**: All tests must pass before deployment
- **Approval Process**: Manual approval required for production deployments

## Performance Standards

### API Performance
- **Response Time**: API endpoints should respond within 500ms for 95th percentile
- **Database Queries**: Use EF Core query optimization and indexing
- **Caching**: Implement caching for frequently accessed data (use IMemoryCache or Redis)
- **Blob Storage**: Cache blob URLs and use CDN for frequently accessed documents

### Monitoring and Diagnostics
- **Application Insights**: Enable Application Insights for telemetry
- **Health Checks**: Implement `/health` endpoint
- **Metrics**: Track key metrics (request count, response time, error rate)
- **Alerts**: Configure alerts for critical errors and performance degradation

## Documentation Standards

### Code Documentation
- **XML Comments**: Comprehensive XML documentation comments for all public APIs
  ```csharp
  /// <summary>
  /// Uploads a ZZLD form document to Azure Blob Storage
  /// </summary>
  /// <param name="formData">The form data to upload</param>
  /// <returns>The blob URI of the uploaded document</returns>
  public async Task<Uri> UploadFormAsync(FormData formData)
  ```
- **README Files**: Each project should have a README explaining its purpose
- **API Documentation**: Use Swagger/OpenAPI for API documentation
- **Architecture Diagrams**: Maintain C4 model diagrams in `/documentation/`

### Project Documentation
- **Feature Documentation**: Document all features in `/documentation/features/`
- **Migration Guides**: Document breaking changes and upgrade paths
- **Testing Documentation**: Document test strategies and test data setup
- **Deployment Documentation**: Document deployment procedures and configurations

## NuGet Package Standards

### Required Packages
```xml
<!-- Core packages -->
<PackageReference Include="Microsoft.AspNetCore.App" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.*" />
<PackageReference Include="Azure.Identity" Version="1.*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />

<!-- Logging and Monitoring -->
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.*" />
<PackageReference Include="Serilog.AspNetCore" Version="7.*" />

<!-- Testing -->
<PackageReference Include="xUnit" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />

<!-- Resilience -->
<PackageReference Include="Polly" Version="8.*" />
```

### Package Management
- **Version Pinning**: Use specific version ranges to avoid breaking changes
- **Security Updates**: Regularly update packages for security patches
- **Dependency Review**: Review all dependencies before adding to project

## Quality Gates

### Pre-commit Checks
- **Code Formatting**: Use `.editorconfig` and `dotnet format`
- **Code Analysis**: Run Roslyn analyzers
- **Unit Tests**: All unit tests must pass
- **Build**: Project must build without errors or warnings

### Pre-merge Checks
- **All Tests Pass**: Unit, integration, and E2E tests
- **Code Coverage**: Minimum 85% coverage maintained
- **Build Success**: Release build completes successfully
- **Security Scan**: No high or critical severity vulnerabilities
- **Code Review**: At least one approval required

### Automated Workflows
- **CI/CD Pipeline**: GitHub Actions for build, test, and deployment
- **Dependency Scanning**: Dependabot for automated security updates
- **Code Quality**: SonarCloud or similar for static analysis

## Development Workflow

### Local Development Setup
```bash
# Clone repository
git clone https://github.com/nuvoleta/ZZLD_Form.git

# Restore dependencies
dotnet restore

# Set up user secrets (for local development)
dotnet user-secrets init
dotnet user-secrets set "AzureStorage:ConnectionString" "your-connection-string"

# Run application
dotnet run --project src/ZZLD_Form.API

# Run tests
dotnet test
```

### Configuration Management
- **appsettings.json**: Default configuration
- **appsettings.Development.json**: Development-specific settings
- **appsettings.Production.json**: Production-specific settings
- **User Secrets**: For local development secrets
- **Azure Key Vault**: For production secrets

## Form-Specific Standards

### ZZLD Form Processing
- **Validation**: Validate all form fields before storage
- **File Formats**: Support PDF and DOCX formats
- **File Size Limits**: Maximum 10MB per form submission
- **Metadata Storage**: Store submission metadata (user ID, timestamp, status)
- **Status Tracking**: Implement status workflow (Submitted → Processing → Completed/Rejected)

### Data Retention
- **Active Forms**: Keep in hot storage for 90 days
- **Archived Forms**: Move to cool/archive storage after 90 days
- **Deletion Policy**: Comply with data retention regulations

---

*This document should be updated as development standards evolve*
*See `/documentation/GIT_STANDARDS.md` for version control guidelines*
*See `/documentation/E2E_TESTING_STRATEGY.md` for comprehensive testing approach*
