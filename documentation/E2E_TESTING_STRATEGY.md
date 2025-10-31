# E2E Testing Strategy for ZZLD Form Application

## Executive Summary

This document outlines a comprehensive End-to-End (E2E) testing strategy for the ZZLD Form application, a C# .NET application for managing ZZLD (Bulgarian Personal Data Protection Declaration) form submissions with Azure Blob Storage integration.

## Testing Infrastructure Overview

### Test Structure
```
tests/
├── ZZLD_Form.UnitTests/          # Component & function tests (85% coverage target)
├── ZZLD_Form.IntegrationTests/   # API & service integration tests  
├── ZZLD_Form.E2ETests/           # End-to-end workflow tests
└── TestUtilities/                # Shared test utilities and helpers
```

### Testing Technologies
- **Unit Tests**: xUnit + Moq + FluentAssertions
- **Integration Tests**: xUnit + WebApplicationFactory + TestContainers (if using database)
- **E2E Tests**: xUnit + Selenium/Playwright for UI, or REST client for API-only
- **Azure Storage Tests**: Azurite (Azure Storage Emulator) for local testing
- **Coverage**: 85% threshold enforced with coverlet

### Test Execution Commands
```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=E2E

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura /p:Threshold=85

# Run tests in parallel
dotnet test --parallel
```

## Enhanced E2E Testing Strategy

### 1. Test Environment Strategy

#### 1.1 Multi-Environment Testing
```csharp
// Test configuration for different environments
public class TestEnvironmentConfiguration
{
    public static class Environments
    {
        public const string Development = "Development";
        public const string Staging = "Staging";
        public const string Production = "Production";
    }
    
    public string BaseUrl { get; set; }
    public string StorageConnectionString { get; set; }
    public string StorageContainer { get; set; } = "zzld-form";
    
    public static TestEnvironmentConfiguration GetConfiguration(string environment)
    {
        return environment switch
        {
            Environments.Development => new()
            {
                BaseUrl = "https://localhost:5001",
                StorageConnectionString = "UseDevelopmentStorage=true" // Azurite
            },
            Environments.Staging => new()
            {
                BaseUrl = Environment.GetEnvironmentVariable("STAGING_URL"),
                StorageConnectionString = Environment.GetEnvironmentVariable("STAGING_STORAGE")
            },
            Environments.Production => new()
            {
                BaseUrl = Environment.GetEnvironmentVariable("PRODUCTION_URL"),
                StorageConnectionString = Environment.GetEnvironmentVariable("PRODUCTION_STORAGE")
            },
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
    }
}
```

#### 1.2 Test Data Management
```csharp
public class TestDataManager : IAsyncLifetime
{
    private readonly BlobContainerClient _containerClient;
    private readonly List<string> _createdBlobs = new();
    
    public async Task InitializeAsync()
    {
        await _containerClient.CreateIfNotExistsAsync();
        await SeedTestData();
    }
    
    private async Task SeedTestData()
    {
        // Upload sample ZZLD forms for testing
        var testForms = new[]
        {
            "test-form-valid.pdf",
            "test-form-large.pdf",
            "test-form-invalid.pdf"
        };
        
        foreach (var form in testForms)
        {
            var blobClient = _containerClient.GetBlobClient($"test-data/{form}");
            await using var stream = File.OpenRead($"TestData/{form}");
            await blobClient.UploadAsync(stream, overwrite: true);
            _createdBlobs.Add(blobClient.Name);
        }
    }
    
    public async Task DisposeAsync()
    {
        // Clean up test data
        foreach (var blobName in _createdBlobs)
        {
            await _containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
        }
    }
}
```

### 2. Comprehensive Test Scenarios

#### 2.1 Core User Journeys
```csharp
[Trait("Category", "E2E")]
public class CompleteFormWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestDataManager _testData;
    
    [Fact]
    public async Task CompleteFormSubmissionWorkflow_Success()
    {
        // 1. Submit ZZLD form with valid data
        var formData = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead("TestData/valid-form.pdf");
        formData.Add(new StreamContent(fileStream), "file", "zzld-form.pdf");
        formData.Add(new StringContent("John Doe"), "fullName");
        formData.Add(new StringContent("john.doe@example.com"), "email");
        
        var submitResponse = await _client.PostAsync("/api/forms/submit", formData);
        submitResponse.EnsureSuccessStatusCode();
        
        var submission = await submitResponse.Content.ReadFromJsonAsync<FormSubmissionResponse>();
        submission.Should().NotBeNull();
        submission.SubmissionId.Should().NotBeEmpty();
        submission.Status.Should().Be("Submitted");
        
        // 2. Verify form is stored in Azure Blob Storage
        var blobExists = await VerifyBlobExists(submission.BlobName);
        blobExists.Should().BeTrue();
        
        // 3. Retrieve form status
        var statusResponse = await _client.GetAsync($"/api/forms/{submission.SubmissionId}/status");
        statusResponse.EnsureSuccessStatusCode();
        
        var status = await statusResponse.Content.ReadFromJsonAsync<FormStatusResponse>();
        status.Status.Should().Be("Processing");
        
        // 4. Download submitted form
        var downloadResponse = await _client.GetAsync($"/api/forms/{submission.SubmissionId}/download");
        downloadResponse.EnsureSuccessStatusCode();
        downloadResponse.Content.Headers.ContentType.MediaType.Should().Be("application/pdf");
        
        // 5. List user's forms
        var listResponse = await _client.GetAsync("/api/forms/my-forms");
        listResponse.EnsureSuccessStatusCode();
        
        var forms = await listResponse.Content.ReadFromJsonAsync<List<FormSummary>>();
        forms.Should().Contain(f => f.SubmissionId == submission.SubmissionId);
    }
    
    [Fact]
    public async Task FormSubmission_WithInvalidData_ReturnsValidationErrors()
    {
        // Submit form without required fields
        var formData = new MultipartFormDataContent();
        // Missing file and required fields
        
        var response = await _client.PostAsync("/api/forms/submit", formData);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        error.Errors.Should().ContainKey("file");
        error.Errors.Should().ContainKey("fullName");
    }
}
```

#### 2.2 Critical Error Scenarios
```csharp
[Trait("Category", "E2E")]
public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task FormSubmission_WhenStorageUnavailable_ReturnsServiceUnavailable()
    {
        // Use test server with unavailable storage connection
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace storage service with one pointing to invalid endpoint
                    services.AddSingleton<IDocumentStorageService>(
                        new DocumentStorageService("InvalidConnectionString"));
                });
            });
        
        var client = factory.CreateClient();
        
        var formData = CreateValidFormData();
        var response = await client.PostAsync("/api/forms/submit", formData);
        
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
    
    [Fact]
    public async Task FormDownload_WithNonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/forms/non-existent-id/download");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task FormSubmission_WithOversizedFile_ReturnsBadRequest()
    {
        // Create file larger than 10MB limit
        var largeFileData = new byte[11 * 1024 * 1024]; // 11MB
        
        var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(largeFileData), "file", "large-form.pdf");
        formData.Add(new StringContent("John Doe"), "fullName");
        
        var response = await _client.PostAsync("/api/forms/submit", formData);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        error.Errors["file"].Should().Contain("exceeds maximum size");
    }
}
```

#### 2.3 Azure Blob Storage Integration Tests
```csharp
[Trait("Category", "E2E")]
public class BlobStorageIntegrationTests : IAsyncLifetime
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    
    [Fact]
    public async Task UploadForm_StoresWithCorrectMetadata()
    {
        var documentService = new DocumentStorageService(_containerClient);
        
        var metadata = new Dictionary<string, string>
        {
            ["userId"] = "user123",
            ["submissionDate"] = DateTime.UtcNow.ToString("O"),
            ["formType"] = "ZZLD"
        };
        
        await using var stream = File.OpenRead("TestData/valid-form.pdf");
        var blobUri = await documentService.UploadFormAsync(stream, "test-form.pdf", metadata);
        
        blobUri.Should().NotBeNull();
        
        // Verify metadata
        var blobClient = _containerClient.GetBlobClient(blobUri.Segments.Last());
        var properties = await blobClient.GetPropertiesAsync();
        
        properties.Value.Metadata.Should().ContainKey("userId");
        properties.Value.Metadata["userId"].Should().Be("user123");
    }
    
    [Fact]
    public async Task ListForms_ReturnsOnlyUserForms()
    {
        // Upload forms for multiple users
        await UploadFormForUser("user1", "form1.pdf");
        await UploadFormForUser("user1", "form2.pdf");
        await UploadFormForUser("user2", "form3.pdf");
        
        var documentService = new DocumentStorageService(_containerClient);
        var user1Forms = await documentService.ListFormsAsync("user1");
        
        user1Forms.Should().HaveCount(2);
        user1Forms.Should().AllSatisfy(f => 
            f.Metadata["userId"].Should().Be("user1"));
    }
    
    [Fact]
    public async Task DownloadForm_WithSasToken_ReturnsValidUrl()
    {
        var documentService = new DocumentStorageService(_containerClient);
        
        await using var stream = File.OpenRead("TestData/valid-form.pdf");
        var blobUri = await documentService.UploadFormAsync(stream, "test.pdf", new());
        
        var sasUrl = await documentService.GenerateSasUrlAsync(blobUri.Segments.Last(), TimeSpan.FromHours(1));
        
        sasUrl.Should().NotBeNullOrEmpty();
        sasUrl.Should().Contain("sig="); // SAS signature
        sasUrl.Should().Contain("se=");  // Expiry time
        
        // Verify URL is accessible
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(sasUrl);
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

#### 2.4 Concurrency and Performance Tests
```csharp
[Trait("Category", "E2E")]
public class ConcurrencyTests
{
    [Fact]
    public async Task ConcurrentFormSubmissions_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 10)
            .Select(i => SubmitFormAsync($"user{i}", $"form{i}.pdf"))
            .ToList();
        
        var results = await Task.WhenAll(tasks);
        
        results.Should().AllSatisfy(r =>
        {
            r.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }
    
    [Fact]
    public async Task FormSubmission_CompletesWithinSLA()
    {
        var stopwatch = Stopwatch.StartNew();
        
        var formData = CreateValidFormData();
        var response = await _client.PostAsync("/api/forms/submit", formData);
        
        stopwatch.Stop();
        
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // 2 second SLA
    }
}
```

### 3. Security Testing

#### 3.1 Authentication and Authorization Tests
```csharp
[Trait("Category", "E2E")]
public class SecurityTests
{
    [Fact]
    public async Task ProtectedEndpoints_WithoutAuthentication_ReturnUnauthorized()
    {
        var client = _factory.CreateClient(); // No auth token
        
        var response = await client.GetAsync("/api/forms/my-forms");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task DownloadForm_ForDifferentUser_ReturnsForbidden()
    {
        // User1 submits form
        var user1Client = CreateAuthenticatedClient("user1");
        var submission = await SubmitFormAsync(user1Client);
        
        // User2 tries to download user1's form
        var user2Client = CreateAuthenticatedClient("user2");
        var response = await user2Client.GetAsync($"/api/forms/{submission.SubmissionId}/download");
        
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task FormSubmission_WithMaliciousFileName_IsSanitized()
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(new byte[100]), "file", "../../../etc/passwd");
        formData.Add(new StringContent("John Doe"), "fullName");
        
        var response = await _client.PostAsync("/api/forms/submit", formData);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FormSubmissionResponse>();
            result.BlobName.Should().NotContain("..");
            result.BlobName.Should().NotContain("/etc/");
        }
    }
}
```

### 4. Test Data and Environment Management

#### 4.1 Test Data Seeding
```csharp
public class E2ETestDataSeeder
{
    public static async Task SeedDevelopmentEnvironment(BlobContainerClient container)
    {
        await container.CreateIfNotExistsAsync();
        
        // Upload sample forms
        var sampleForms = new[]
        {
            ("valid-form-1.pdf", "user1", "Submitted"),
            ("valid-form-2.pdf", "user1", "Processing"),
            ("completed-form.pdf", "user2", "Completed")
        };
        
        foreach (var (fileName, userId, status) in sampleForms)
        {
            var blobClient = container.GetBlobClient($"{userId}/{Guid.NewGuid()}/{fileName}");
            
            await using var stream = File.OpenRead($"SeedData/{fileName}");
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId,
                    ["status"] = status,
                    ["submissionDate"] = DateTime.UtcNow.ToString("O")
                }
            });
        }
    }
}
```

#### 4.2 Test Isolation with Azurite
```json
// docker-compose.test.yml
{
  "version": "3.8",
  "services": {
    "azurite": {
      "image": "mcr.microsoft.com/azure-storage/azurite",
      "ports": ["10000:10000", "10001:10001", "10002:10002"],
      "command": "azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0"
    }
  }
}
```

```bash
# Start Azurite for testing
docker-compose -f docker-compose.test.yml up -d

# Run E2E tests
dotnet test --filter Category=E2E

# Stop Azurite
docker-compose -f docker-compose.test.yml down
```

### 5. CI/CD Integration

#### 5.1 GitHub Actions Workflow
```yaml
# .github/workflows/e2e-tests.yml
name: E2E Tests

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    
    services:
      azurite:
        image: mcr.microsoft.com/azure-storage/azurite
        ports:
          - 10000:10000
          - 10001:10001
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '6.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run Unit Tests
        run: dotnet test --filter Category=Unit --no-build --configuration Release
      
      - name: Run Integration Tests
        run: dotnet test --filter Category=Integration --no-build --configuration Release
        env:
          AZURE_STORAGE_CONNECTION_STRING: "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://localhost"
      
      - name: Run E2E Tests
        run: dotnet test --filter Category=E2E --no-build --configuration Release /p:CollectCoverage=true
        env:
          AZURE_STORAGE_CONNECTION_STRING: "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://localhost"
      
      - name: Upload Coverage Report
        uses: codecov/codecov-action@v3
        with:
          file: coverage.cobertura.xml
      
      - name: Upload Test Results
        uses: actions/upload-artifact@v4
        if: failure()
        with:
          name: test-results
          path: TestResults/
```

### 6. Test Reporting

#### 6.1 Custom Test Report Generation
```csharp
[assembly: TestFramework("ZZLD_Form.E2ETests.CustomTestFramework", "ZZLD_Form.E2ETests")]

public class TestRunReporter : IDisposable
{
    private readonly List<TestResult> _results = new();
    
    public void RecordTestResult(string testName, bool passed, TimeSpan duration, string error = null)
    {
        _results.Add(new TestResult
        {
            TestName = testName,
            Passed = passed,
            Duration = duration,
            Error = error,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public void Dispose()
    {
        GenerateHtmlReport();
        GenerateMarkdownReport();
    }
    
    private void GenerateMarkdownReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# E2E Test Results");
        sb.AppendLine($"**Date:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Total Tests:** {_results.Count}");
        sb.AppendLine($"**Passed:** {_results.Count(r => r.Passed)}");
        sb.AppendLine($"**Failed:** {_results.Count(r => !r.Passed)}");
        sb.AppendLine();
        
        sb.AppendLine("## Failed Tests");
        foreach (var failure in _results.Where(r => !r.Passed))
        {
            sb.AppendLine($"- **{failure.TestName}**");
            sb.AppendLine($"  - Error: {failure.Error}");
        }
        
        File.WriteAllText("TestResults/E2E-Report.md", sb.ToString());
    }
}
```

## Implementation Roadmap

### Phase 1: Foundation (Week 1)
- [x] Set up xUnit test projects
- [x] Configure Azurite for local Azure Storage testing
- [x] Create test data management utilities
- [x] Implement base test fixtures

### Phase 2: Core E2E Tests (Week 2)
- [ ] Implement form submission workflow tests
- [ ] Add blob storage integration tests
- [ ] Create error handling scenarios
- [ ] Implement validation tests

### Phase 3: Advanced Scenarios (Week 3)
- [ ] Security and authorization tests
- [ ] Concurrency and performance tests
- [ ] File format validation tests
- [ ] Data retention and cleanup tests

### Phase 4: CI/CD Integration (Week 4)
- [ ] Set up GitHub Actions workflows
- [ ] Configure automated test reporting
- [ ] Implement coverage tracking
- [ ] Set up failure notifications

## Success Metrics

### Coverage Metrics
- **E2E Test Coverage**: 100% of critical user workflows
- **Code Coverage**: Minimum 85% overall
- **Integration Test Coverage**: 100% of API endpoints
- **Storage Operations Coverage**: 100% of blob operations

### Quality Metrics
- **Test Reliability**: <2% flaky test rate
- **Execution Time**: <5 minutes for full E2E suite
- **Bug Detection**: 90% of production issues caught before deployment
- **Performance**: All endpoints complete within SLA

### Operational Metrics
- **Test Automation**: 100% of manual regression tests automated
- **Deployment Confidence**: Zero rollbacks due to missed test scenarios
- **Feedback Loop**: <10 minutes from commit to test results

## Maintenance & Evolution

### Regular Maintenance
- Weekly review of test reliability metrics
- Monthly update of test data and scenarios
- Quarterly review of coverage and gaps
- Annual comprehensive strategy review

### Continuous Improvement
- Add tests for new features immediately
- Update tests when bugs are found in production
- Refactor tests to reduce duplication
- Monitor and optimize test execution time

---

*Last Updated: October 2025*  
*Version: 1.0*  
*Next Review: January 2026*
