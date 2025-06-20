#!/bin/bash

# Health Check Script for Takhlees Tech Backend
echo "🏥 Takhlees Tech Backend - Health Check"
echo "======================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check HTTP endpoint
check_endpoint() {
    local name=$1
    local url=$2
    local expected_code=${3:-200}
    
    printf "%-25s" "$name:"
    
    if command -v curl &> /dev/null; then
        response=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 --max-time 10 "$url" 2>/dev/null)
        if [ "$response" = "$expected_code" ]; then
            echo -e "${GREEN}✅ UP (HTTP $response)${NC}"
            return 0
        else
            echo -e "${RED}❌ DOWN (HTTP $response)${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠️  curl not available${NC}"
        return 2
    fi
}

# Function to check Docker container
check_container() {
    local container_name=$1
    printf "%-25s" "$container_name:"
    
    if command -v docker &> /dev/null; then
        if docker ps --filter "name=$container_name" --filter "status=running" --format "{{.Names}}" | grep -q "$container_name"; then
            echo -e "${GREEN}✅ RUNNING${NC}"
            return 0
        else
            echo -e "${RED}❌ NOT RUNNING${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠️  Docker not available${NC}"
        return 2
    fi
}

# Function to check database connection
check_database() {
    local db_name=$1
    local port=$2
    printf "%-25s" "$db_name:"
    
    if command -v nc &> /dev/null; then
        if nc -z localhost "$port" 2>/dev/null; then
            echo -e "${GREEN}✅ ACCESSIBLE (Port $port)${NC}"
            return 0
        else
            echo -e "${RED}❌ NOT ACCESSIBLE (Port $port)${NC}"
            return 1
        fi
    elif command -v telnet &> /dev/null; then
        if timeout 3 telnet localhost "$port" &>/dev/null; then
            echo -e "${GREEN}✅ ACCESSIBLE (Port $port)${NC}"
            return 0
        else
            echo -e "${RED}❌ NOT ACCESSIBLE (Port $port)${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠️  netcat/telnet not available${NC}"
        return 2
    fi
}

echo ""
echo "🐳 Docker Containers Status"
echo "----------------------------"

# Check Docker containers
check_container "firstproject-service"
check_container "admin-service"
check_container "customerservice-service"
check_container "user-service"
check_container "admin-db"
check_container "customerservice-db"
check_container "firstproject-db"
check_container "user-db"
check_container "hangfire-db"
check_container "phpmyadmin"
check_container "nginx-proxy"

echo ""
echo "🗄️ Database Connectivity"
echo "-------------------------"

# Check database ports
check_database "admin-db" 3307
check_database "customerservice-db" 3308
check_database "firstproject-db" 3309
check_database "user-db" 3310
check_database "hangfire-db" 3311

echo ""
echo "🌐 HTTP Endpoints"
echo "-----------------"

# Check HTTP endpoints
check_endpoint "FirstProject Service" "http://localhost:9100/health"
check_endpoint "Admin Service" "http://localhost:5001/health"
check_endpoint "Customer Service" "http://localhost:5002/health"
check_endpoint "User Service" "http://localhost:5003/health"
check_endpoint "phpMyAdmin" "http://localhost:8080/"
check_endpoint "Nginx Proxy" "http://localhost:8090/health"

echo ""
echo "🔍 Service-Specific Checks"
echo "---------------------------"

# Check Swagger endpoints
check_endpoint "FirstProject Swagger" "http://localhost:9100/swagger" 200
check_endpoint "Admin Swagger" "http://localhost:5001/swagger" 200
check_endpoint "Customer Swagger" "http://localhost:5002/swagger" 200
check_endpoint "User Swagger" "http://localhost:5003/swagger" 200

echo ""
echo "📊 Quick System Info"
echo "---------------------"

# System information
if command -v docker &> /dev/null; then
    echo "Docker Version: $(docker --version 2>/dev/null || echo 'Not available')"
    echo "Docker Compose: $(docker-compose --version 2>/dev/null || echo 'Not available')"
    
    # Count running containers
    running_containers=$(docker ps -q | wc -l)
    total_containers=$(docker ps -a -q | wc -l)
    echo "Containers: $running_containers running / $total_containers total"
    
    # Docker system resources
    echo "Docker Resources:"
    docker system df 2>/dev/null || echo "  Unable to get resource info"
fi

echo ""
echo "💡 Troubleshooting Tips"
echo "-----------------------"
echo "• If services are down: ./scripts/start-all.sh"
echo "• To view logs: ./scripts/logs.sh [service-name]"
echo "• To restart specific service: docker-compose restart [service-name]"
echo "• To rebuild service: docker-compose up -d --build [service-name]"

echo ""
echo "Health check completed!"
echo "=======================" 