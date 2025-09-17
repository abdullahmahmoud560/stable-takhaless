#!/bin/bash

# Stop All Takhlees Services Script
echo "🛑 Stopping Takhlees Tech Backend Services..."

# Choose env file
ENV_FILE="${ENV_FILE:-.env}"
if [[ "$1" == "--prod" || "$1" == "-p" ]]; then
	ENV_FILE=".env.prod"
fi

echo "📦 Using environment file: $ENV_FILE"

# Stop all services
echo "⏹️ Stopping application services..."
docker-compose --env-file "$ENV_FILE" down

# Optional: Remove volumes (uncomment if you want to reset data)
# echo "🗑️ Removing volumes..."
# docker-compose --env-file "$ENV_FILE" down -v

echo ""
echo "✅ All services stopped successfully!"
echo ""
echo "💡 To start again: ENV_FILE=$ENV_FILE ./scripts/start-all.sh" 