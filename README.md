# ZZLD Form Generator

A .NET 8 Web API for generating ZZLD (Personal Data Protection Law) declaration forms for Bulgarian citizens. The application generates PDF forms with personal data, stores them in Azure Blob Storage, and provides download URLs with SAS tokens.

## Features

- Generate PDF forms with Bulgarian Cyrillic support
- FluentValidation for input validation (EGN 10 digits, postal code 4 digits)
- Azure Blob Storage integration with SAS token generation
- Retry logic using Polly for resilient storage operations
- Comprehensive logging with Serilog
- Full test coverage (Unit, Integration, E2E)
- OpenAPI/Swagger documentation

## Project Structure

```
ZZLD_Form/
├── src/
│   ├── ZZLD_Form.API/          # Web API layer
│   ├── ZZLD_Form.Core/         # Business logic and domain models
│   ├── ZZLD_Form.Infrastructure/ # External services (PDF, Blob Storage)
│   └── ZZLD_Form.Shared/       # Shared DTOs and constants
└── tests/
    ├── ZZLD_Form.UnitTests/         # Unit tests
    ├── ZZLD_Form.IntegrationTests/  # Integration tests
    └── ZZLD_Form.E2ETests/          # End-to-end tests
```

## Prerequisites

- .NET 8.0 SDK or later
- Azure Storage Account (or Azure Storage Emulator for development)

## Configuration

### appsettings.json

```json
{
  "AzureStorage": {
    "ConnectionString": "<your-connection-string>",
    "AccountName": "aivideoprocessing",
    "ContainerName": "zzld-form",
    "UseManagedIdentity": false,
    "SasTokenValidityHours": 24
  }
}
```

### Environment Variables

Alternatively, you can configure using environment variables:

- `AzureStorage__ConnectionString`
- `AzureStorage__AccountName`
- `AzureStorage__ContainerName`

## Building the Project

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build in Release mode
dotnet build -c Release
```

## Running the Application

```bash
# Run the API
cd src/ZZLD_Form.API
dotnet run

# The API will be available at:
# - https://localhost:5001
# - http://localhost:5000
# - Swagger UI: https://localhost:5001/swagger
```

## Running Tests

### Run All Tests

```bash
# Run all tests in the solution
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Projects

```bash
# Unit tests only
dotnet test tests/ZZLD_Form.UnitTests/

# Integration tests only
dotnet test tests/ZZLD_Form.IntegrationTests/

# E2E tests only
dotnet test tests/ZZLD_Form.E2ETests/
```

### Run Specific Test Classes

```bash
# Run a specific test class
dotnet test --filter "FullyQualifiedName~PersonalDataValidatorTests"

# Run a specific test method
dotnet test --filter "FullyQualifiedName~PersonalDataValidatorTests.Validate_WithValidData_ShouldNotHaveErrors"
```

## API Endpoints

### Health Check
```
GET /api/health
```

### Generate Form
```
POST /api/form/generate
Content-Type: application/json

{
  "firstName": "Иван",
  "middleName": "Петров",
  "lastName": "Иванов",
  "egn": "1234567890",
  "dateOfBirth": "1990-05-15",
  "address": "ул. Витоша 10",
  "city": "София",
  "postalCode": "1000",
  "phoneNumber": "+359888123456",
  "email": "ivan.ivanov@example.com",
  "documentNumber": "123456789",
  "documentIssueDate": "2020-01-01",
  "documentIssuedBy": "МВР София"
}
```

### Get Form
```
GET /api/form/{formId}
```

## Development

### Using Azure Storage Emulator (Azurite)

For local development, you can use Azurite:

```bash
# Install Azurite
npm install -g azurite

# Run Azurite
azurite --silent --location c:\azurite --debug c:\azurite\debug.log

# Or using Docker
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

### Code Style

The project follows standard C# naming conventions:
- PascalCase for public members
- camelCase for private fields with underscore prefix
- Async methods end with "Async"
- Interfaces start with "I"

## Deployment

### Azure App Service

1. Create an Azure App Service (Linux, .NET 8)
2. Configure Application Settings:
   - `AzureStorage__ConnectionString` or use Managed Identity
   - `AzureStorage__AccountName`
   - `AzureStorage__ContainerName`
3. Deploy using:

```bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group <resource-group> \
  --name <app-name> \
  --src ./publish.zip
```

### Using Managed Identity

For production, it's recommended to use Managed Identity:

1. Enable Managed Identity on your App Service
2. Grant "Storage Blob Data Contributor" role to the identity
3. Set `AzureStorage__UseManagedIdentity` to `true`
4. Remove `AzureStorage__ConnectionString`

## Validation Rules

### Personal Data
- First Name: Required
- Last Name: Required
- EGN: Required, exactly 10 digits
- Date of Birth: Must be in the past, not more than 150 years ago
- Address: Required
- City: Required
- Postal Code: Required, exactly 4 digits
- Email: Required, valid email format
- Document Number: Required
- Document Issue Date: Cannot be in the future
- Document Issued By: Required

## PDF Format

Generated PDFs include:
- Bulgarian Cyrillic support
- Date format: dd.MM.yyyy (Bulgarian standard)
- Personal information section
- Address and contact details
- Document information
- Declaration text (ZZLD compliance)
- Signature section with generation timestamp

## Troubleshooting

### QuestPDF License Error
If you encounter a QuestPDF license error, the Community license is configured for non-commercial use. For commercial use, obtain a license from QuestPDF.

### Azure Storage Connection Issues
- Verify connection string is correct
- Check that the container exists or the app has permission to create it
- For Managed Identity, ensure proper RBAC roles are assigned

### Tests Failing
- Ensure all NuGet packages are restored: `dotnet restore`
- Check that .NET 8 SDK is installed: `dotnet --version`
- Run tests with verbose output to see detailed errors: `dotnet test -v detailed`

## License

This project is for internal use by Amexis.

## Support

For issues or questions, contact the development team.
