#!/bin/bash

# New-Order API Testing Script
echo "🚀 Takhlees Tech Backend - New-Order API Testing"
echo "================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
AUTH_SERVICE_URL="http://localhost:9100"
USER_SERVICE_URL="http://localhost:5003"
NGINX_URL="http://localhost:8090"

# Test user credentials
TEST_EMAIL="testorder@example.com"
TEST_PASSWORD="OrderTest123!"
TEST_FULLNAME="Order Test User"
TEST_IDENTITY="1234567890"
TEST_PHONE="+1234567890"

# Function to print status
print_status() {
    local message=$1
    local status=$2
    printf "%-50s" "$message:"
    if [ "$status" = "SUCCESS" ]; then
        echo -e "${GREEN}✅ SUCCESS${NC}"
    elif [ "$status" = "FAIL" ]; then
        echo -e "${RED}❌ FAIL${NC}"
    elif [ "$status" = "WARNING" ]; then
        echo -e "${YELLOW}⚠️  WARNING${NC}"
    else
        echo -e "${BLUE}ℹ️  INFO${NC}"
    fi
}

# Function to create test files
create_test_files() {
    echo "📁 Creating test files for upload..."
    
    # Create a test PDF file
    echo "This is a test PDF content" > test_document.pdf
    
    # Create a test image file (mock)
    echo "This is a test image content" > test_image.jpg
    
    print_status "Test files created" "SUCCESS"
}

# Function to cleanup test files
cleanup_test_files() {
    echo "🧹 Cleaning up test files..."
    rm -f test_document.pdf test_image.jpg
    print_status "Test files cleaned up" "SUCCESS"
}

# Function to register test user
register_test_user() {
    echo ""
    echo "👤 Registering test user..."
    
    REGISTER_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$AUTH_SERVICE_URL/api/Register-user" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\":\"$TEST_EMAIL\",
            \"password\":\"$TEST_PASSWORD\",
            \"fullName\":\"$TEST_FULLNAME\",
            \"confirm\":\"$TEST_PASSWORD\",
            \"identity\":\"$TEST_IDENTITY\",
            \"phoneNumber\":\"$TEST_PHONE\"
        }")
    
    REGISTER_STATUS=$(echo $REGISTER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    REGISTER_BODY=$(echo $REGISTER_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $REGISTER_STATUS -eq 200 ]; then
        print_status "User registration" "SUCCESS"
        return 0
    elif [ $REGISTER_STATUS -eq 400 ]; then
        print_status "User registration (user may exist)" "WARNING"
        echo "   Response: $(echo $REGISTER_BODY | cut -c1-100)..."
        return 0
    else
        print_status "User registration" "FAIL"
        echo "   HTTP Status: $REGISTER_STATUS"
        echo "   Response: $(echo $REGISTER_BODY | cut -c1-100)..."
        return 1
    fi
}

# Function to login and get token
login_and_get_token() {
    echo ""
    echo "🔐 Logging in to get authentication token..."
    
    LOGIN_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$AUTH_SERVICE_URL/api/Login" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\":\"$TEST_EMAIL\",
            \"password\":\"$TEST_PASSWORD\"
        }")
    
    LOGIN_STATUS=$(echo $LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    LOGIN_BODY=$(echo $LOGIN_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $LOGIN_STATUS -eq 200 ]; then
        # Extract token from response (assuming it's in JSON format)
        TOKEN=$(echo $LOGIN_BODY | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
        if [ -n "$TOKEN" ]; then
            print_status "User login" "SUCCESS"
            echo "   Token: ${TOKEN:0:50}..."
            return 0
        else
            print_status "Token extraction" "FAIL"
            echo "   Response: $(echo $LOGIN_BODY | cut -c1-100)..."
            return 1
        fi
    else
        print_status "User login" "FAIL"
        echo "   HTTP Status: $LOGIN_STATUS"
        echo "   Response: $(echo $LOGIN_BODY | cut -c1-100)..."
        return 1
    fi
}

# Function to test New-Order API
test_new_order_api() {
    echo ""
    echo "📦 Testing New-Order API..."
    
    if [ -z "$TOKEN" ]; then
        print_status "New-Order API test" "FAIL"
        echo "   Error: No authentication token available"
        return 1
    fi
    
    # Test 1: Valid order with all required fields
    echo ""
    echo "🧪 Test 1: Valid order submission"
    
    ORDER_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$USER_SERVICE_URL/api/New-Order" \
        -H "Authorization: Bearer $TOKEN" \
        -F "Location=Riyadh, Saudi Arabia" \
        -F "numberOfLicense=123456789" \
        -F "Notes=This is a test order for API testing purposes" \
        -F "City=Riyadh" \
        -F "Town=Al-Malaz" \
        -F "zipCode=12345" \
        -F "numberOfTypeOrders[0].typeOrder=Electronics Import" \
        -F "numberOfTypeOrders[0].Number=5" \
        -F "numberOfTypeOrders[0].Weight=25.5" \
        -F "numberOfTypeOrders[0].Size=Medium" \
        -F "uploadFile=@test_document.pdf" \
        -F "uploadFile=@test_image.jpg")
    
    ORDER_STATUS=$(echo $ORDER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    ORDER_BODY=$(echo $ORDER_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $ORDER_STATUS -eq 200 ]; then
        print_status "Valid order submission" "SUCCESS"
        echo "   Response: $(echo $ORDER_BODY | cut -c1-100)..."
    else
        print_status "Valid order submission" "FAIL"
        echo "   HTTP Status: $ORDER_STATUS"
        echo "   Response: $(echo $ORDER_BODY | cut -c1-100)..."
    fi
    
    # Test 2: Order without required fields
    echo ""
    echo "🧪 Test 2: Order without required fields (should fail)"
    
    INVALID_ORDER_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$USER_SERVICE_URL/api/New-Order" \
        -H "Authorization: Bearer $TOKEN" \
        -F "Notes=This order is missing required fields")
    
    INVALID_ORDER_STATUS=$(echo $INVALID_ORDER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    INVALID_ORDER_BODY=$(echo $INVALID_ORDER_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $INVALID_ORDER_STATUS -eq 400 ]; then
        print_status "Invalid order rejection" "SUCCESS"
        echo "   Response: $(echo $INVALID_ORDER_BODY | cut -c1-100)..."
    else
        print_status "Invalid order rejection" "FAIL"
        echo "   HTTP Status: $INVALID_ORDER_STATUS (expected 400)"
        echo "   Response: $(echo $INVALID_ORDER_BODY | cut -c1-100)..."
    fi
    
    # Test 3: Order without authentication token
    echo ""
    echo "🧪 Test 3: Order without authentication (should fail)"
    
    UNAUTH_ORDER_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$USER_SERVICE_URL/api/New-Order" \
        -F "Location=Test Location" \
        -F "numberOfLicense=123456789")
    
    UNAUTH_ORDER_STATUS=$(echo $UNAUTH_ORDER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    UNAUTH_ORDER_BODY=$(echo $UNAUTH_ORDER_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $UNAUTH_ORDER_STATUS -eq 401 ]; then
        print_status "Unauthorized order rejection" "SUCCESS"
        echo "   Response: $(echo $UNAUTH_ORDER_BODY | cut -c1-100)..."
    else
        print_status "Unauthorized order rejection" "FAIL"
        echo "   HTTP Status: $UNAUTH_ORDER_STATUS (expected 401)"
        echo "   Response: $(echo $UNAUTH_ORDER_BODY | cut -c1-100)..."
    fi
}

# Function to test through Nginx proxy
test_new_order_via_nginx() {
    echo ""
    echo "🔄 Testing New-Order API through Nginx proxy..."
    
    if [ -z "$TOKEN" ]; then
        print_status "New-Order API test via Nginx" "FAIL"
        echo "   Error: No authentication token available"
        return 1
    fi
    
    NGINX_ORDER_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "$NGINX_URL/api/New-Order" \
        -H "Authorization: Bearer $TOKEN" \
        -F "Location=Jeddah, Saudi Arabia" \
        -F "numberOfLicense=987654321" \
        -F "Notes=This is a test order via Nginx proxy" \
        -F "City=Jeddah" \
        -F "Town=Al-Hamra" \
        -F "zipCode=54321" \
        -F "numberOfTypeOrders[0].typeOrder=Machinery Import" \
        -F "numberOfTypeOrders[0].Number=2" \
        -F "numberOfTypeOrders[0].Weight=150.0" \
        -F "numberOfTypeOrders[0].Size=Large" \
        -F "uploadFile=@test_document.pdf")
    
    NGINX_ORDER_STATUS=$(echo $NGINX_ORDER_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    NGINX_ORDER_BODY=$(echo $NGINX_ORDER_RESPONSE | sed -e 's/HTTPSTATUS:.*//g')
    
    if [ $NGINX_ORDER_STATUS -eq 200 ]; then
        print_status "Order via Nginx proxy" "SUCCESS"
        echo "   Response: $(echo $NGINX_ORDER_BODY | cut -c1-100)..."
    else
        print_status "Order via Nginx proxy" "FAIL"
        echo "   HTTP Status: $NGINX_ORDER_STATUS"
        echo "   Response: $(echo $NGINX_ORDER_BODY | cut -c1-100)..."
    fi
}

# Function to display API documentation
show_api_documentation() {
    echo ""
    echo "📖 NEW-ORDER API DOCUMENTATION"
    echo "==============================="
    echo ""
    echo "Endpoint: POST /api/New-Order"
    echo "Service: User Service (Port 5003)"
    echo "Content-Type: multipart/form-data"
    echo "Authentication: Bearer Token required"
    echo ""
    echo "Required Fields:"
    echo "  - Location (string): Location of the order"
    echo "  - numberOfLicense (string): License number (numeric)"
    echo "  - numberOfTypeOrders (array): Array of order types"
    echo ""
    echo "Optional Fields:"
    echo "  - Notes (string): Additional notes"
    echo "  - City (string): City name"
    echo "  - Town (string): Town/district name"
    echo "  - zipCode (string): Postal code"
    echo "  - uploadFile (files): Multiple files (PDF, JPG, PNG allowed)"
    echo ""
    echo "numberOfTypeOrders Structure:"
    echo "  - typeOrder (string): Type of order"
    echo "  - Number (int): Quantity"
    echo "  - Weight (float): Weight in kg"
    echo "  - Size (string): Size description"
    echo ""
    echo "Example curl command:"
    echo "curl -X POST http://localhost:5003/api/New-Order \\"
    echo "  -H \"Authorization: Bearer YOUR_TOKEN\" \\"
    echo "  -F \"Location=Riyadh, Saudi Arabia\" \\"
    echo "  -F \"numberOfLicense=123456789\" \\"
    echo "  -F \"numberOfTypeOrders[0].typeOrder=Electronics Import\" \\"
    echo "  -F \"numberOfTypeOrders[0].Number=5\" \\"
    echo "  -F \"numberOfTypeOrders[0].Weight=25.5\" \\"
    echo "  -F \"numberOfTypeOrders[0].Size=Medium\" \\"
    echo "  -F \"uploadFile=@document.pdf\""
    echo ""
}

# Main execution
main() {
    echo "Starting New-Order API tests..."
    
    # Show API documentation
    show_api_documentation
    
    # Create test files
    create_test_files
    
    # Register test user
    if ! register_test_user; then
        echo "Failed to register test user. Exiting."
        cleanup_test_files
        exit 1
    fi
    
    # Login and get token
    if ! login_and_get_token; then
        echo "Failed to login and get token. Exiting."
        cleanup_test_files
        exit 1
    fi
    
    # Test New-Order API
    test_new_order_api
    
    # Test through Nginx
    test_new_order_via_nginx
    
    # Cleanup
    cleanup_test_files
    
    echo ""
    echo "🎉 New-Order API testing completed!"
    echo "=================================="
}

# Check if running directly or being sourced
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi 