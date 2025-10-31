#!/bin/bash

# ZZLD Form API Test Script
# Tests all endpoints with sample data

set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
API_URL="${API_URL:-https://localhost:5001}"
VERBOSE="${VERBOSE:-false}"

echo -e "${YELLOW}================================${NC}"
echo -e "${YELLOW}  ZZLD Form API Test Script${NC}"
echo -e "${YELLOW}================================${NC}"
echo ""
echo "API URL: $API_URL"
echo ""

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
    fi
}

# Function to make API call and check response
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local expected_status=$4
    local test_name=$5
    
    echo -e "\n${YELLOW}Testing:${NC} $test_name"
    echo "Endpoint: $method $endpoint"
    
    if [ "$method" == "POST" ]; then
        response=$(curl -k -s -w "\n%{http_code}" -X POST \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_URL$endpoint" 2>/dev/null)
    else
        response=$(curl -k -s -w "\n%{http_code}" -X GET \
            "$API_URL$endpoint" 2>/dev/null)
    fi
    
    http_code=$(echo "$response" | tail -n 1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$VERBOSE" == "true" ]; then
        echo "Response Code: $http_code"
        echo "Response Body: $body" | jq '.' 2>/dev/null || echo "$body"
    fi
    
    if [ "$http_code" == "$expected_status" ]; then
        print_result 0 "$test_name"
        echo "$body"
        return 0
    else
        print_result 1 "$test_name (Expected: $expected_status, Got: $http_code)"
        echo "$body"
        return 1
    fi
}

# Test 1: Health Check
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test 1: Health Check${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"
test_endpoint "GET" "/api/health" "" "200" "Health check endpoint"

# Test 2: Generate Form with Valid Data
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test 2: Generate Form (Valid Data)${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"

FORM_DATA='{
  "fullName": "Иван Петров Георгиев",
  "egn": "1234567890",
  "address": "ул. Цар Освободител 123",
  "city": "София",
  "postalCode": "1000",
  "phoneNumber": "+359888123456",
  "email": "ivan.petrov@example.com",
  "dateOfBirth": "1990-05-15T00:00:00",
  "idCardNumber": "123456789",
  "idCardIssueDate": "2020-01-15T00:00:00",
  "idCardIssuer": "МВР София"
}'

generate_response=$(test_endpoint "POST" "/api/form/generate" "$FORM_DATA" "200" "Generate ZZLD form")
FORM_ID=$(echo "$generate_response" | jq -r '.formId' 2>/dev/null || echo "")

if [ -n "$FORM_ID" ] && [ "$FORM_ID" != "null" ]; then
    echo -e "${GREEN}Form generated successfully!${NC}"
    echo "Form ID: $FORM_ID"
    DOWNLOAD_URL=$(echo "$generate_response" | jq -r '.downloadUrl' 2>/dev/null || echo "")
    echo "Download URL: $DOWNLOAD_URL"
    
    # Test 3: Retrieve Generated Form
    echo -e "\n${YELLOW}═══════════════════════════════${NC}"
    echo -e "${YELLOW}Test 3: Retrieve Form${NC}"
    echo -e "${YELLOW}═══════════════════════════════${NC}"
    test_endpoint "GET" "/api/form/$FORM_ID" "" "200" "Retrieve generated form by ID"
else
    echo -e "${RED}Failed to extract Form ID from response${NC}"
fi

# Test 4: Generate Form with Invalid Data (missing required field)
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test 4: Validation Test (Missing EGN)${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"

INVALID_DATA='{
  "fullName": "Иван Петров",
  "address": "ул. Цар Освободител 123",
  "city": "София",
  "postalCode": "1000",
  "phoneNumber": "+359888123456",
  "email": "ivan@example.com",
  "dateOfBirth": "1990-05-15T00:00:00"
}'

test_endpoint "POST" "/api/form/generate" "$INVALID_DATA" "400" "Validation error for missing EGN"

# Test 5: Generate Form with Invalid EGN (wrong length)
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test 5: Validation Test (Invalid EGN Length)${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"

INVALID_EGN='{
  "fullName": "Иван Петров Георгиев",
  "egn": "123",
  "address": "ул. Цар Освободител 123",
  "city": "София",
  "postalCode": "1000",
  "phoneNumber": "+359888123456",
  "email": "ivan.petrov@example.com",
  "dateOfBirth": "1990-05-15T00:00:00"
}'

test_endpoint "POST" "/api/form/generate" "$INVALID_EGN" "400" "Validation error for invalid EGN length"

# Test 6: Retrieve Non-Existent Form
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test 6: Not Found Test${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"
test_endpoint "GET" "/api/form/non-existent-id" "" "404" "Not found for non-existent form"

# Summary
echo -e "\n${YELLOW}═══════════════════════════════${NC}"
echo -e "${YELLOW}Test Summary${NC}"
echo -e "${YELLOW}═══════════════════════════════${NC}"
echo -e "API URL: $API_URL"
echo -e "All critical endpoints tested"
echo -e "\n${GREEN}Testing complete!${NC}"
echo ""
echo "To test with verbose output: VERBOSE=true ./test-api.sh"
echo "To test against different URL: API_URL=https://your-api.com ./test-api.sh"
