#!/bin/bash

# Stop All Takhlees Services Script
echo "🛑 Stopping Takhlees Tech Backend Services..."

# Stop all services
echo "⏹️ Stopping application services..."
docker-compose down

# Optional: Remove volumes (uncomment if you want to reset data)
# echo "🗑️ Removing volumes..."
# docker-compose down -v

echo ""
echo "✅ All services stopped successfully!"
echo ""
echo "💡 To start again: ./scripts/start-all.sh"
echo "🗑️ To remove all data: docker-compose down -v" 