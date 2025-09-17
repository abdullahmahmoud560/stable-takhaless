#!/bin/bash

# View Logs Script
echo "📋 Takhlees Tech Backend - Log Viewer"
echo ""

# Choose env file
ENV_FILE="${ENV_FILE:-.env}"
if [[ "$1" == "--prod" || "$1" == "-p" ]]; then
	ENV_FILE=".env.prod"
	shift
fi

SERVICE="$1"

if [ -z "$SERVICE" ]; then
	echo "Usage: ./scripts/logs.sh [service-name] [--prod]"
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
	echo "  ENV_FILE=.env.prod ./scripts/logs.sh firstproject-service    # View FirstProject logs (prod env)"
	echo "  ./scripts/logs.sh firstproject-service --prod               # Same as above"
	
	echo ""
	echo "📊 Showing logs from all services (env: $ENV_FILE)..."
	docker-compose --env-file "$ENV_FILE" logs -f
else
	echo "📊 Showing logs from: $SERVICE (env: $ENV_FILE)"
	docker-compose --env-file "$ENV_FILE" logs -f "$SERVICE"
fi 