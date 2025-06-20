#!/bin/bash

# Start All Takhlees Services Script
echo "🚀 Starting Takhlees Tech Backend Services..."

# Check if Docker and Docker Compose are installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create necessary directories
echo "📁 Creating necessary directories..."
mkdir -p mysql-init
mkdir -p nginx/ssl
mkdir -p logs

# Start databases first
echo "🗄️ Starting databases..."
docker-compose up -d admin-db customerservice-db firstproject-db user-db hangfire-db

# Wait for databases to be ready
echo "⏳ Waiting for databases to initialize..."
sleep 30

# Start phpMyAdmin
echo "🔧 Starting phpMyAdmin..."
docker-compose up -d phpmyadmin

# Start application services
echo "🚀 Starting application services..."
docker-compose up -d admin-service customerservice-service firstproject-service user-service

# Wait for services to be ready
echo "⏳ Waiting for services to initialize..."
sleep 20

# Start Nginx reverse proxy
echo "🌐 Starting Nginx reverse proxy..."
docker-compose up -d nginx

echo ""
echo "✅ All services started successfully!"
echo ""
echo "📊 Service Status:"
echo "├── 🗄️  Databases: Ready"
echo "├── 🔧 phpMyAdmin: http://localhost:8080"
echo "├── 🔐 FirstProject (Auth): http://localhost:9100"
echo "├── 👑 Admin Service: http://localhost:5001"
echo "├── 🎧 Customer Service: http://localhost:5002"
echo "├── 👤 User Service: http://localhost:5003"
echo "└── 🌐 Nginx Proxy: http://localhost:8090"
echo ""
echo "🔍 To check logs: docker-compose logs -f [service-name]"
echo "🛑 To stop all: ./scripts/stop-all.sh" 