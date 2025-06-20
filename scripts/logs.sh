#!/bin/bash

# View Logs Script
echo "📋 Takhlees Tech Backend - Log Viewer"
echo ""

if [ $# -eq 0 ]; then
    echo "Usage: ./scripts/logs.sh [service-name]"
    echo ""
    echo "Available services:"
    echo "  - admin-service"
    echo "  - customerservice-service"
    echo "  - firstproject-service"
    echo "  - user-service"
    echo "  - admin-db"
    echo "  - customerservice-db"
    echo "  - firstproject-db"
    echo "  - user-db"
    echo "  - hangfire-db"
    echo "  - phpmyadmin"
    echo "  - nginx"
    echo ""
    echo "Examples:"
    echo "  ./scripts/logs.sh firstproject-service    # View FirstProject logs"
    echo "  ./scripts/logs.sh admin-db                # View Admin database logs"
    echo "  ./scripts/logs.sh                         # View all logs"
    
    echo ""
    echo "📊 Showing logs from all services..."
    docker-compose logs -f
else
    echo "📊 Showing logs from: $1"
    docker-compose logs -f $1
fi 