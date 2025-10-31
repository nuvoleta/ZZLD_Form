# ZZLD Form Generator - Quick Start Guide

## Prerequisites
- .NET 8.0 SDK
- Azure Storage Account or Azure Storage Emulator (Azurite)

## Quick Setup

### 1. Configure Azure Storage

Edit `src/ZZLD_Form.API/appsettings.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "YOUR_CONNECTION_STRING_HERE",
    "AccountName": "aivideoprocessing",
    "ContainerName": "zzld-form",
    "UseManagedIdentity": false,
    "SasTokenValidityHours": 24
  }
}
```

Or use environment variable:
```bash
export AzureStorage__ConnectionString="YOUR_CONNECTION_STRING_HERE"
```

### 2. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
cd src/ZZLD_Form.API
dotnet run
```

Access Swagger UI: https://localhost:5001/swagger

## Quick Test with curl

### Generate a Form

```bash
curl -X POST https://localhost:5001/api/form/generate \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Иван",
    "middleName": "Петров",
    "lastName": "Иванов",
    "egn": "1234567890",
    "dateOfBirth": "1990-05-15T00:00:00",
    "address": "ул. Витоша 10",
    "city": "София",
    "postalCode": "1000",
    "phoneNumber": "+359888123456",
    "email": "ivan.ivanov@example.com",
    "documentNumber": "123456789",
    "documentIssueDate": "2020-01-01T00:00:00",
    "documentIssuedBy": "МВР София"
  }'
```

### Retrieve a Form

```bash
curl -X GET https://localhost:5001/api/form/{formId}
```

### Health Check

```bash
curl https://localhost:5001/api/health
```

## Development with Azure Storage Emulator (Azurite)

### Install Azurite

```bash
npm install -g azurite
```

### Run Azurite

```bash
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

### Configure for Azurite

Use `appsettings.Development.json` (already configured):

```json
{
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "AccountName": "devstoreaccount1",
    "ContainerName": "zzld-form"
  }
}
```

## Common Commands

### Build Commands
```bash
# Clean build
dotnet clean && dotnet build

# Release build
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish
```

### Test Commands
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/ZZLD_Form.UnitTests/

# Run specific test class
dotnet test --filter "FullyQualifiedName~PersonalDataValidatorTests"

# Run with detailed output
dotnet test --verbosity detailed
```

### Run Commands
```bash
# Run API in development mode
cd src/ZZLD_Form.API
dotnet run

# Run with specific environment
dotnet run --environment Production

# Run with watch (auto-reload)
dotnet watch run
```

## Project Structure Overview

```
ZZLD_Form/
├── src/
│   ├── ZZLD_Form.API/              # REST API
│   ├── ZZLD_Form.Core/             # Business Logic
│   ├── ZZLD_Form.Infrastructure/   # PDF & Storage
│   └── ZZLD_Form.Shared/           # DTOs & Constants
└── tests/
    ├── ZZLD_Form.UnitTests/        # Unit Tests
    ├── ZZLD_Form.IntegrationTests/ # Integration Tests
    └── ZZLD_Form.E2ETests/         # E2E Tests
```

## Troubleshooting

### Issue: dotnet command not found
**Solution**: Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

### Issue: Azure Storage connection error
**Solution**: 
1. Verify connection string is correct
2. Check container name matches configuration
3. Use Azurite for local development

### Issue: Tests failing
**Solution**:
1. Run `dotnet restore` to ensure packages are installed
2. Run `dotnet clean && dotnet build` to rebuild
3. Check test output with `dotnet test -v detailed`

### Issue: PDF not generating
**Solution**:
1. Verify QuestPDF package is installed
2. Check for font/encoding issues with Cyrillic text
3. Review logs in `logs/` directory

### Issue: SAS token expired
**Solution**: SAS tokens are valid for 24 hours. Retrieve form again to get new token.

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/form/generate | Generate new ZZLD form |
| GET | /api/form/{formId} | Retrieve generated form |
| GET | /api/health | Health check |

## Validation Rules

| Field | Requirements |
|-------|--------------|
| EGN | Exactly 10 digits |
| PostalCode | Exactly 4 digits |
| Email | Valid email format |
| DateOfBirth | Past date, max 150 years ago |
| All names | Required |
| Address | Required |
| City | Required |
| DocumentNumber | Required |
| DocumentIssuedBy | Required |

## Next Steps

1. Configure Azure Storage connection
2. Run the application
3. Test with Swagger UI
4. Review logs in `logs/` directory
5. Deploy to Azure App Service

## Support

For detailed documentation, see:
- README.md - Full documentation
- IMPLEMENTATION_SUMMARY.md - Implementation details
- documentation/ - Additional guides

For issues, review logs in the `logs/` directory.
