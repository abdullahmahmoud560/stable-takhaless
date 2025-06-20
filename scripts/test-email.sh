#!/bin/bash

# =======================================================
# Email SMTP Test Script
# =======================================================

echo "🔍 Testing SMTP Connection..."
echo "==============================="

# Load environment variables
source ../firstProject/environment.env 2>/dev/null || {
    echo "❌ Could not load environment variables"
    exit 1
}

SMTP_SERVER="${EmailConfiguration__SmtpServer}"
SMTP_PORT="${EmailConfiguration__Port}"
SMTP_USER="${EmailConfiguration__Username}"

echo "📧 SMTP Server: $SMTP_SERVER"
echo "🔌 Port: $SMTP_PORT"
echo "👤 Username: $SMTP_USER"
echo ""

# Test 1: Check if SMTP server is reachable
echo "🔍 Test 1: Checking SMTP server connectivity..."
if timeout 10 bash -c "</dev/tcp/$SMTP_SERVER/$SMTP_PORT"; then
    echo "✅ SMTP server is reachable on port $SMTP_PORT"
else
    echo "❌ Cannot connect to SMTP server on port $SMTP_PORT"
    echo "💡 Trying alternative ports..."
    
    # Test common SMTP ports
    for port in 587 25 2525; do
        echo "   Testing port $port..."
        if timeout 5 bash -c "</dev/tcp/$SMTP_SERVER/$port" 2>/dev/null; then
            echo "   ✅ Port $port is available"
        else
            echo "   ❌ Port $port is not available"
        fi
    done
fi

echo ""

# Test 2: Check SSL/TLS connectivity
echo "🔍 Test 2: Checking SSL/TLS connectivity..."
if command -v openssl >/dev/null 2>&1; then
    echo "Testing SSL connection to $SMTP_SERVER:$SMTP_PORT..."
    timeout 10 openssl s_client -connect "$SMTP_SERVER:$SMTP_PORT" -quiet -verify_return_error < /dev/null
    if [ $? -eq 0 ]; then
        echo "✅ SSL/TLS connection successful"
    else
        echo "❌ SSL/TLS connection failed"
        echo "💡 Try using port 587 with STARTTLS instead of port 465 with SSL"
    fi
else
    echo "⚠️ openssl not available, skipping SSL test"
fi

echo ""

# Test 3: DNS Resolution
echo "🔍 Test 3: Checking DNS resolution..."
if nslookup "$SMTP_SERVER" >/dev/null 2>&1; then
    echo "✅ DNS resolution successful"
    nslookup "$SMTP_SERVER" | grep -A 2 "Name:"
else
    echo "❌ DNS resolution failed"
fi

echo ""
echo "🔧 Troubleshooting Tips:"
echo "======================="
echo "1. If port 465 fails, try port 587 with STARTTLS"
echo "2. Check if your hosting provider blocks SMTP ports"
echo "3. Verify your email credentials are correct"
echo "4. Check if your IP is whitelisted with Hostinger"
echo "5. Consider using Gmail SMTP as an alternative"
echo ""
echo "📝 To switch to port 587:"
echo "   EmailConfiguration__Port=587"
echo "   EmailConfiguration__UseSSL=false"
echo ""
echo "📝 To switch to Gmail:"
echo "   EmailConfiguration__SmtpServer=smtp.gmail.com"
echo "   EmailConfiguration__Port=587"
echo "   EmailConfiguration__UseSSL=false" 