# ZZLD Form Generator

A .NET 8 Web API for generating ZZLD (Personal Data Protection Law) declaration forms for Bulgarian citizens. The application generates PDF forms with personal data, stores them in Azure Blob Storage, and provides download URLs with SAS tokens.

## Features

- Fill existing PDF templates with Bulgarian Cyrillic support using iText7
- FluentValidation for input validation (EGN 10 digits, postal code 4 digits)
- Azure Blob Storage integration with SAS token generation
- Retry logic using Polly for resilient storage operations
- Comprehensive logging with Serilog
- Full test coverage (Unit, Integration, E2E)
- OpenAPI/Swagger documentation
- Precise coordinate-based field positioning for 14 form fields

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
- Arial Bold font (`/mnt/c/Windows/Fonts/arialbd.ttf` for Cyrillic support)
- PDF template file (`templates/ZZLD_Form.pdf`)

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
  "city": "София",
  "postalCode": "1000",
  "community": "Лозенец",
  "street": "Витоша",
  "number": "10",
  "block": "5",
  "entrance": "A",
  "floor": "3",
  "apartment": "12",
  "phoneNumber": "+359888123456"
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
- Middle Name: Optional
- Last Name: Required
- EGN: Required, exactly 10 digits
- City: Required
- Postal Code: Required, exactly 4 digits
- Community: Optional
- Street: Optional
- Number: Optional
- Block: Optional
- Entrance: Optional
- Floor: Optional
- Apartment: Optional
- Phone Number: Optional

## PDF Format

PDF generation uses iText7 to fill an existing template:
- **Library**: iText7 9.3.0 with bouncy-castle-adapter
- **Approach**: Overlay text on existing PDF template at precise coordinates
- **Font**: Arial Bold TrueType with IDENTITY_H encoding for Cyrillic support
- **Template**: `templates/ZZLD_Form.pdf` (original Bulgarian form)
- **Fields**: 14 personal data fields positioned per `requirements/fields.txt`
- **Output**: PDF with filled data matching original form layout

## Troubleshooting

### Font Not Found Error
If you encounter "font file not found" errors, ensure Arial Bold font is available at `/mnt/c/Windows/Fonts/arialbd.ttf` (WSL path) or update the font path in `PdfProcessor.cs` to match your system.

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
