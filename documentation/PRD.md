# Product Requirements Document (PRD)
## ZZLD Form Generator - MVP

**Version:** 1.0  
**Date:** October 31, 2025  
**Status:** Draft  
**Owner:** Development Team

---

## 1. Executive Summary

### 1.1 Purpose
Build a REST API application that generates personalized ZZLD (Bulgarian Personal Data Protection Declaration) forms by filling a PDF template with user-provided personal information and storing the generated documents in Azure Blob Storage.

### 1.2 Scope
This is an MVP (Minimum Viable Product) that will serve as a component of a larger project. The MVP focuses on core PDF generation functionality without authentication or complex retention policies.

### 1.3 Success Criteria
- Successfully generate personalized PDF documents from template
- Store generated PDFs in Azure Blob Storage with unique filenames
- Provide downloadable URLs for generated documents
- Support Bulgarian language and date formatting
- Achieve 85% test coverage
- Complete form generation within 2 seconds (95th percentile)

---

## 2. Product Overview

### 2.1 Problem Statement
Organizations need to generate personalized ZZLD forms (Bulgarian Personal Data Protection Declarations) for individuals. Manual form filling is time-consuming and error-prone. An automated solution is needed to streamline this process.

### 2.2 Solution
A REST API service that:
1. Accepts personal data via API endpoint
2. Retrieves PDF template from Azure Blob Storage
3. Fills template fields (marked with "....") with provided data
4. Generates unique filename and stores PDF in blob storage
5. Returns secure access URL to generated document

### 2.3 Target Users
- **Primary**: Backend systems and applications that need to generate ZZLD forms
- **Secondary**: Developers who will integrate this API into larger applications

---

## 3. Functional Requirements

### 3.1 Core Features

#### 3.1.1 PDF Form Generation (P0 - Critical)
**Description**: Generate personalized PDF forms from template

**Acceptance Criteria**:
- API accepts JSON payload with personal data
- System retrieves ZZLD_Form.pdf template from Azure Blob Storage
- All fields marked with "...." in template are replaced with provided data
- Generated PDF maintains original template formatting
- Bulgarian characters (Cyrillic) are properly encoded and displayed
- Date fields use Bulgarian date format (dd.MM.yyyy)

**API Endpoint**:
```
POST /api/form/generate
Content-Type: application/json

Request Body:
{
  "fullName": "string",
  "egn": "string",           // Bulgarian Personal ID Number
  "address": "string",
  "city": "string",
  "postalCode": "string",
  "phoneNumber": "string",
  "email": "string",
  "dateOfBirth": "string",   // ISO 8601 format
  "idCardNumber": "string",
  "idCardIssueDate": "string",
  "idCardIssuer": "string",
  // Additional fields based on PDF template analysis
}

Response:
{
  "formId": "string",
  "fileName": "string",
  "downloadUrl": "string",      // SAS URL with expiration
  "generatedAt": "string",      // ISO 8601 timestamp
  "expiresAt": "string"         // SAS token expiration
}
```

#### 3.1.2 Form Retrieval (P0 - Critical)
**Description**: Retrieve previously generated forms

**Acceptance Criteria**:
- API accepts formId as parameter
- Returns SAS URL for secure download
- Returns 404 if form not found
- SAS token valid for 1 hour

**API Endpoint**:
```
GET /api/form/{formId}

Response:
{
  "formId": "string",
  "fileName": "string",
  "downloadUrl": "string",
  "generatedAt": "string",
  "metadata": {
    "fullName": "string",
    "generatedAt": "string"
  }
}
```

#### 3.1.3 Health Check (P0 - Critical)
**Description**: Health check endpoint for monitoring

**API Endpoint**:
```
GET /api/health

Response:
{
  "status": "Healthy|Degraded|Unhealthy",
  "checks": {
    "blobStorage": "Healthy|Unhealthy",
    "pdfProcessor": "Healthy|Unhealthy"
  },
  "timestamp": "string"
}
```

### 3.2 Input Validation (P0 - Critical)

**Requirements**:
- All required fields must be present
- EGN (Bulgarian Personal ID) must be 10 digits
- Email must be valid format
- Phone number must be valid Bulgarian format (+359...)
- Postal code must be 4 digits
- Date fields must be valid dates
- Text fields must contain Bulgarian (Cyrillic) or Latin characters
- Maximum field lengths enforced

**Error Response Format**:
```json
{
  "error": "ValidationError",
  "message": "Input validation failed",
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  }
}
```

### 3.3 File Storage (P0 - Critical)

**Requirements**:
- Generated PDFs stored in Azure Blob Storage container: `zzld-form`
- Unique filename format: `generated/{timestamp}_{guid}.pdf`
  - Example: `generated/20251031_143022_a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf`
- Blob metadata includes:
  - `fullName`: Person's full name
  - `generatedAt`: Generation timestamp (UTC)
  - `formId`: Unique form identifier
- SAS token generated with 1-hour expiration for downloads
- Content-Type: `application/pdf`

### 3.4 Template Management (P1 - High)

**Requirements**:
- PDF template (`ZZLD_Form.pdf`) stored in blob storage at: `templates/ZZLD_Form.pdf`
- Template cached in memory after first retrieval
- Cache invalidation if template updated (manual restart for MVP)
- Fallback error handling if template not found

---

## 4. Non-Functional Requirements

### 4.1 Performance (P0 - Critical)
- API response time: < 2 seconds for 95th percentile
- Support concurrent requests: minimum 10 concurrent PDF generations
- Template retrieval cached to minimize blob storage calls
- PDF generation processing time: < 1 second

### 4.2 Reliability (P0 - Critical)
- 99% uptime SLA for MVP
- Graceful error handling with appropriate HTTP status codes
- Retry logic for transient Azure Blob Storage failures (Polly library)
- Comprehensive logging for debugging

### 4.3 Security (P1 - High)
- No authentication required for MVP (future enhancement)
- Input sanitization to prevent injection attacks
- SAS tokens for secure document access (time-limited)
- HTTPS enforced for all API endpoints
- No sensitive data logged (GDPR compliance)

### 4.4 Scalability (P1 - High)
- Stateless API design for horizontal scaling
- Azure App Service deployment with auto-scaling capability
- Blob storage can handle growth in document volume

### 4.5 Maintainability (P0 - Critical)
- Clean architecture with separation of concerns
- 85% minimum test coverage
- Comprehensive XML documentation for public APIs
- Structured logging with correlation IDs

### 4.6 Localization (P0 - Critical)
- All generated PDFs use Bulgarian (Cyrillic) character encoding
- Date format: dd.MM.yyyy (Bulgarian standard)
- Error messages in English (API consumers handle user-facing messages)

---

## 5. Technical Requirements

### 5.1 Technology Stack

**Platform**:
- .NET 6.0 or .NET 8.0
- ASP.NET Core Web API

**PDF Processing**:
- QuestPDF (recommended for simplicity) OR
- iText7 (if advanced PDF manipulation needed)

**Cloud Services**:
- Azure Blob Storage (existing container: `zzld-form`)
- Azure App Service (deployment target)

**Key Libraries**:
```xml
<PackageReference Include="Azure.Storage.Blobs" Version="12.*" />
<PackageReference Include="Azure.Identity" Version="1.*" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />
<PackageReference Include="Polly" Version="8.*" />
<PackageReference Include="QuestPDF" Version="2024.*" />
<PackageReference Include="Serilog.AspNetCore" Version="7.*" />
```

**Testing**:
```xml
<PackageReference Include="xUnit" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="coverlet.collector" Version="6.*" />
```

### 5.2 Project Structure

```
src/
├── ZZLD_Form.API/              # Web API project
├── ZZLD_Form.Core/             # Business logic
├── ZZLD_Form.Infrastructure/   # External dependencies (Blob, PDF)
└── ZZLD_Form.Shared/           # Shared DTOs/constants

tests/
├── ZZLD_Form.UnitTests/
├── ZZLD_Form.IntegrationTests/
└── ZZLD_Form.E2ETests/
```

### 5.3 Configuration

**appsettings.json**:
```json
{
  "AzureStorage": {
    "ConnectionString": "",  // From Azure Key Vault or user secrets
    "ContainerName": "zzld-form",
    "TemplatePath": "templates/ZZLD_Form.pdf",
    "GeneratedFolder": "generated/",
    "SasTokenExpirationHours": 1
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5.4 Deployment

**Target**: Azure App Service (Linux)
- **Reason**: Simplest deployment for MVP, supports .NET, built-in HTTPS, easy scaling

**Alternative**: Azure Container Apps (if containerization preferred)

**Configuration**:
- Runtime: .NET 6.0 or .NET 8.0
- Operating System: Linux
- Pricing Tier: B1 (Basic) for MVP, scalable to higher tiers
- Always On: Enabled
- HTTPS Only: Enabled

---

## 6. API Specifications

### 6.1 Endpoints Summary

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/form/generate` | Generate personalized PDF | No (MVP) |
| GET | `/api/form/{formId}` | Retrieve form by ID | No (MVP) |
| GET | `/api/health` | Health check | No |

### 6.2 Error Codes

| HTTP Status | Error Code | Description |
|-------------|------------|-------------|
| 200 | - | Success |
| 400 | `ValidationError` | Input validation failed |
| 404 | `NotFound` | Form not found |
| 500 | `InternalServerError` | Server error |
| 503 | `ServiceUnavailable` | Blob storage unavailable |

### 6.3 Rate Limiting (Future Enhancement)
Not implemented in MVP. Consider for production version.

---

## 7. Data Models

### 7.1 PersonalData (Input)

```csharp
public class PersonalData
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{10}$")]
    public string EGN { get; set; }  // Bulgarian Personal ID Number
    
    [Required]
    [StringLength(500)]
    public string Address { get; set; }
    
    [Required]
    [StringLength(100)]
    public string City { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{4}$")]
    public string PostalCode { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [StringLength(50)]
    public string IdCardNumber { get; set; }
    
    public DateTime? IdCardIssueDate { get; set; }
    
    [StringLength(200)]
    public string IdCardIssuer { get; set; }
}
```

### 7.2 FormGenerationResult (Output)

```csharp
public class FormGenerationResult
{
    public string FormId { get; set; }
    public string FileName { get; set; }
    public string DownloadUrl { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

---

## 8. Testing Requirements

### 8.1 Test Coverage
- **Minimum**: 85% code coverage
- **Unit Tests**: All services, validators, and business logic
- **Integration Tests**: Blob storage operations, PDF generation
- **E2E Tests**: Complete workflow (API → PDF generation → storage)

### 8.2 Test Scenarios

**Unit Tests**:
- Input validation (valid/invalid data)
- Unique filename generation
- Bulgarian date formatting
- Cyrillic character handling

**Integration Tests**:
- Blob storage upload/download
- PDF template retrieval
- SAS token generation
- PDF form field population

**E2E Tests**:
- Complete form generation workflow
- Form retrieval by ID
- Error handling (template not found, storage unavailable)
- Concurrent form generation

### 8.3 Test Data
- Sample personal data with Bulgarian names and addresses
- Test PDF template in Azurite (local testing)
- Edge cases: special characters, long text, missing optional fields

---

## 9. Testing Scripts

### 9.1 Bash Script Requirements
Provide `test-api.sh` script that:
- Tests form generation endpoint with sample data
- Tests form retrieval endpoint
- Tests health check endpoint
- Displays results in readable format
- Uses `curl` and `jq` for JSON parsing

### 9.2 PowerShell Script Requirements
Provide `Test-Api.ps1` script with same functionality as bash script:
- Uses `Invoke-RestMethod` for API calls
- Formats output for readability
- Supports Windows environments

---

## 10. Development Workflow

### 10.1 Phase 1: Setup (Week 1)
- [ ] Create solution and project structure
- [ ] Set up Azure Blob Storage connection
- [ ] Configure Azurite for local development
- [ ] Create initial API skeleton
- [ ] Set up CI/CD pipeline (GitHub Actions)

### 10.2 Phase 2: Core Development (Week 2-3)
- [ ] Analyze PDF template and identify all fields
- [ ] Create `PersonalData` model with validation
- [ ] Implement `BlobStorageService`
- [ ] Implement `PdfProcessor` with chosen library
- [ ] Implement `FormService` business logic
- [ ] Create API endpoints

### 10.3 Phase 3: Testing (Week 3-4)
- [ ] Write unit tests (85% coverage target)
- [ ] Write integration tests for blob storage
- [ ] Write E2E tests for complete workflow
- [ ] Create test scripts (Bash + PowerShell)
- [ ] Performance testing

### 10.4 Phase 4: Deployment (Week 4)
- [ ] Deploy to Azure App Service (staging)
- [ ] Smoke testing in staging
- [ ] Production deployment
- [ ] Documentation finalization

---

## 11. Risks and Mitigation

### 11.1 Technical Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| PDF library complexity | High | Medium | Choose QuestPDF for simplicity, fallback to iText7 if needed |
| Bulgarian character encoding issues | High | Medium | Thorough testing with Cyrillic characters, UTF-8 encoding |
| Azure Blob Storage throttling | Medium | Low | Implement retry logic with Polly, caching for template |
| PDF template field identification | High | Medium | Manual analysis, coordinate with form provider |
| Performance degradation with concurrent requests | Medium | Low | Load testing, Azure App Service scaling |

### 11.2 Schedule Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| PDF template analysis takes longer than expected | Medium | Medium | Allocate buffer time, start early |
| Testing takes longer than planned | Medium | Low | Automate tests, parallel development |

---

## 12. Dependencies

### 12.1 External Dependencies
- Azure Blob Storage account (existing)
- PDF template file (`ZZLD_Form.pdf`) in blob storage
- Azure subscription for App Service deployment

### 12.2 Internal Dependencies
- None (standalone MVP component)

---

## 13. Success Metrics

### 13.1 MVP Launch Criteria
- [x] All P0 features implemented
- [ ] 85% test coverage achieved
- [ ] API successfully generates PDFs with Bulgarian characters
- [ ] Deployed to Azure App Service
- [ ] Test scripts (Bash + PowerShell) provided
- [ ] Documentation complete

### 13.2 Performance Metrics
- API response time < 2 seconds (95th percentile)
- Successful PDF generation rate > 99%
- Zero data loss (all generated PDFs stored successfully)

### 13.3 Quality Metrics
- Zero critical bugs in production
- Test coverage ≥ 85%
- All API endpoints return appropriate status codes

---

## 14. Future Enhancements (Post-MVP)

### 14.1 Authentication & Authorization (P1)
- Azure AD integration
- API key authentication
- Role-based access control

### 14.2 Advanced Features (P2)
- Batch PDF generation
- Form templates versioning
- Custom template upload
- Email delivery of generated forms
- Audit logging and compliance reporting

### 14.3 Retention Policies (P2)
- Automated document archival after 90 days
- Automatic deletion after retention period
- Lifecycle management policies

### 14.4 Monitoring & Observability (P1)
- Application Insights integration
- Performance metrics dashboard
- Alert configuration for failures

---

## 15. Appendices

### 15.1 Glossary
- **ZZLD**: Заявление за Защита на Лични Данни (Bulgarian Personal Data Protection Declaration)
- **EGN**: Единен граждански номер (Unified Civil Number - Bulgarian Personal ID)
- **SAS**: Shared Access Signature (Azure Blob Storage secure access)
- **MVP**: Minimum Viable Product

### 15.2 References
- Azure Blob Storage Documentation: https://docs.microsoft.com/azure/storage/blobs/
- QuestPDF Documentation: https://www.questpdf.com/
- iText7 Documentation: https://itextpdf.com/itext-7
- Bulgarian Date Format Standard: dd.MM.yyyy

### 15.3 Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-31 | Development Team | Initial PRD creation |

---

**Document Status**: Ready for Review  
**Next Review Date**: Upon MVP completion  
**Approval Required From**: Product Owner, Technical Lead
