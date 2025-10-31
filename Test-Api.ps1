# ZZLD Form API Test Script (PowerShell)
# Tests all endpoints with sample data

param(
    [string]$ApiUrl = "https://localhost:5001",
    [switch]$Verbose
)

# Configuration
$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

Write-Host "================================" -ForegroundColor Yellow
Write-Host "  ZZLD Form API Test Script" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "API URL: $ApiUrl"
Write-Host ""

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Data,
        [int]$ExpectedStatus,
        [string]$TestName
    )
    
    Write-Host "`n" -NoNewline
    Write-Host "Testing: " -ForegroundColor Yellow -NoNewline
    Write-Host $TestName
    Write-Host "Endpoint: $Method $Endpoint"
    
    try {
        $uri = "$ApiUrl$Endpoint"
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        if ($Method -eq "POST") {
            $body = $Data | ConvertTo-Json -Depth 10
            $response = Invoke-WebRequest -Uri $uri -Method $Method -Headers $headers -Body $body -UseBasicParsing
        } else {
            $response = Invoke-WebRequest -Uri $uri -Method $Method -Headers $headers -UseBasicParsing
        }
        
        $statusCode = $response.StatusCode
        $content = $response.Content
        
        if ($Verbose) {
            Write-Host "Response Code: $statusCode"
            Write-Host "Response Body: $content"
        }
        
        if ($statusCode -eq $ExpectedStatus) {
            Write-Host "✓ " -ForegroundColor Green -NoNewline
            Write-Host $TestName
            return $content | ConvertFrom-Json
        } else {
            Write-Host "✗ " -ForegroundColor Red -NoNewline
            Write-Host "$TestName (Expected: $ExpectedStatus, Got: $statusCode)"
            return $null
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($Verbose) {
            Write-Host "Response Code: $statusCode"
            Write-Host "Error: $($_.Exception.Message)"
        }
        
        if ($statusCode -eq $ExpectedStatus) {
            Write-Host "✓ " -ForegroundColor Green -NoNewline
            Write-Host $TestName
            
            # Try to extract response body
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                return $responseBody | ConvertFrom-Json
            }
        } else {
            Write-Host "✗ " -ForegroundColor Red -NoNewline
            Write-Host "$TestName (Expected: $ExpectedStatus, Got: $statusCode)"
        }
        return $null
    }
}

# Test 1: Health Check
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test 1: Health Check" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow
Test-Endpoint -Method "GET" -Endpoint "/api/health" -ExpectedStatus 200 -TestName "Health check endpoint"

# Test 2: Generate Form with Valid Data
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test 2: Generate Form (Valid Data)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow

$formData = @{
    fullName = "Иван Петров Георгиев"
    egn = "1234567890"
    address = "ул. Цар Освободител 123"
    city = "София"
    postalCode = "1000"
    phoneNumber = "+359888123456"
    email = "ivan.petrov@example.com"
    dateOfBirth = "1990-05-15T00:00:00"
    idCardNumber = "123456789"
    idCardIssueDate = "2020-01-15T00:00:00"
    idCardIssuer = "МВР София"
}

$generateResponse = Test-Endpoint -Method "POST" -Endpoint "/api/form/generate" -Data $formData -ExpectedStatus 200 -TestName "Generate ZZLD form"

if ($generateResponse -and $generateResponse.formId) {
    Write-Host "Form generated successfully!" -ForegroundColor Green
    Write-Host "Form ID: $($generateResponse.formId)"
    Write-Host "Download URL: $($generateResponse.downloadUrl)"
    
    $formId = $generateResponse.formId
    
    # Test 3: Retrieve Generated Form
    Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
    Write-Host "Test 3: Retrieve Form" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════" -ForegroundColor Yellow
    Test-Endpoint -Method "GET" -Endpoint "/api/form/$formId" -ExpectedStatus 200 -TestName "Retrieve generated form by ID"
}
else {
    Write-Host "Failed to extract Form ID from response" -ForegroundColor Red
}

# Test 4: Generate Form with Invalid Data (missing required field)
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test 4: Validation Test (Missing EGN)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow

$invalidData = @{
    fullName = "Иван Петров"
    address = "ул. Цар Освободител 123"
    city = "София"
    postalCode = "1000"
    phoneNumber = "+359888123456"
    email = "ivan@example.com"
    dateOfBirth = "1990-05-15T00:00:00"
}

Test-Endpoint -Method "POST" -Endpoint "/api/form/generate" -Data $invalidData -ExpectedStatus 400 -TestName "Validation error for missing EGN"

# Test 5: Generate Form with Invalid EGN (wrong length)
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test 5: Validation Test (Invalid EGN Length)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow

$invalidEgn = @{
    fullName = "Иван Петров Георгиев"
    egn = "123"
    address = "ул. Цар Освободител 123"
    city = "София"
    postalCode = "1000"
    phoneNumber = "+359888123456"
    email = "ivan.petrov@example.com"
    dateOfBirth = "1990-05-15T00:00:00"
}

Test-Endpoint -Method "POST" -Endpoint "/api/form/generate" -Data $invalidEgn -ExpectedStatus 400 -TestName "Validation error for invalid EGN length"

# Test 6: Retrieve Non-Existent Form
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test 6: Not Found Test" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow
Test-Endpoint -Method "GET" -Endpoint "/api/form/non-existent-id" -ExpectedStatus 404 -TestName "Not found for non-existent form"

# Summary
Write-Host "`n═══════════════════════════════" -ForegroundColor Yellow
Write-Host "Test Summary" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Yellow
Write-Host "API URL: $ApiUrl"
Write-Host "All critical endpoints tested"
Write-Host "`nTesting complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To test with verbose output: .\Test-Api.ps1 -Verbose"
Write-Host "To test against different URL: .\Test-Api.ps1 -ApiUrl https://your-api.com"
