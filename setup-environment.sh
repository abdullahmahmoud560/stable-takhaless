#!/bin/bash

# ==================================================
# Takhlees Tech Backend - Environment Setup Script
# ==================================================
# This script helps you set up the environment files for development and production

set -e

echo "🚀 Takhlees Tech Backend - Environment Setup"
echo "=============================================="
echo ""

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose not found. Please install Docker and Docker Compose first."
    exit 1
fi

echo "✅ Docker Compose found"

# Function to create development environment file
create_dev_env() {
    echo "📝 Creating development environment file (.env.dev)..."
    
    cat > .env.dev << 'EOF'
# Takhlees Tech Backend - Development Environment
# Safe to commit to version control (development values only)

ASPNETCORE_ENVIRONMENT=Development
MYSQL_VERSION=8.0

# Database Configuration - Development
ADMIN_DB_CONTAINER_NAME=admin-db
ADMIN_DB_ROOT_PASSWORD=dev_admin_root_password_123
ADMIN_DB_NAME=dev_logs
ADMIN_DB_USER=dev_admin
ADMIN_DB_PASSWORD=dev_admin_password_123
ADMIN_DB_PORT=3307

CUSTOMERSERVICE_DB_CONTAINER_NAME=customerservice-db
CUSTOMERSERVICE_DB_ROOT_PASSWORD=dev_customerservice_root_password_123
CUSTOMERSERVICE_DB_NAME=dev_CustomerService
CUSTOMERSERVICE_DB_USER=dev_customerservice
CUSTOMERSERVICE_DB_PASSWORD=dev_customerservice_password_123
CUSTOMERSERVICE_DB_PORT=3308

FIRSTPROJECT_DB_CONTAINER_NAME=firstproject-db
FIRSTPROJECT_DB_ROOT_PASSWORD=dev_firstproject_root_password_123
FIRSTPROJECT_DB_NAME=dev_mainData
FIRSTPROJECT_DB_USER=dev_firstproject
FIRSTPROJECT_DB_PASSWORD=dev_firstproject_password_123
FIRSTPROJECT_DB_PORT=3309

USER_DB_CONTAINER_NAME=user-db
USER_DB_ROOT_PASSWORD=dev_user_root_password_123
USER_DB_NAME=dev_Orders
USER_DB_USER=dev_user
USER_DB_PASSWORD=dev_user_password_123
USER_DB_PORT=3310

HANGFIRE_DB_CONTAINER_NAME=hangfire-db
HANGFIRE_DB_ROOT_PASSWORD=dev_hangfire_root_password_123
HANGFIRE_DB_NAME=dev_HangFire
HANGFIRE_DB_USER=dev_hangfire
HANGFIRE_DB_PASSWORD=dev_hangfire_password_123
HANGFIRE_DB_PORT=3311

# phpMyAdmin Configuration
PHPMYADMIN_CONTAINER_NAME=phpmyadmin
PHPMYADMIN_ROOT_PASSWORD=dev_phpmyadmin_root_password_123
PHPMYADMIN_PORT=8080

# JWT Configuration (Development Only)
JWT_ISSUER=http://localhost:9100
JWT_AUDIENCE=http://localhost:9100
JWT_SECRET_KEY=dev_jwt_secret_key_for_development_only_not_secure_123456789

# Service Configuration
ADMIN_SERVICE_CONTAINER_NAME=admin-service
ADMIN_SERVICE_PORT=5001
ADMIN_SUPER_ADMIN_EMAIL=admin@localhost
ADMIN_SUPER_ADMIN_PASSWORD=DevAdmin123!
ADMIN_DEFAULT_PAGE_SIZE=20
ADMIN_MAX_PAGE_SIZE=100

CUSTOMERSERVICE_SERVICE_CONTAINER_NAME=customerservice-service
CUSTOMERSERVICE_SERVICE_PORT=5002

FIRSTPROJECT_SERVICE_CONTAINER_NAME=firstproject-service
FIRSTPROJECT_SERVICE_PORT=9100
FIRSTPROJECT_SERVICE_INTERNAL_PORT=9100

USER_SERVICE_CONTAINER_NAME=user-service
USER_SERVICE_PORT=5003

# PayPal Configuration (Sandbox)
PAYPAL_CLIENT_ID=sandbox_paypal_client_id_for_development
PAYPAL_SECRET=sandbox_paypal_secret_for_development
PAYPAL_IS_SANDBOX=true

# Email Configuration (MailHog)
EMAIL_FROM=dev@localhost
EMAIL_SMTP_SERVER=mailhog
EMAIL_PORT=1025
EMAIL_USERNAME=dev@localhost
EMAIL_PASSWORD=dev_password_123
EMAIL_USE_SSL=false

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:8090,http://localhost:9100,http://localhost:5001,http://localhost:5002,http://localhost:5003
CORS_ALLOW_CREDENTIALS=true

# Nginx Configuration
NGINX_CONTAINER_NAME=nginx-proxy
NGINX_HTTP_PORT=8090
NGINX_HTTPS_PORT=8443

# Development Tools
MAILHOG_CONTAINER_NAME=mailhog
MAILHOG_SMTP_PORT=1025
MAILHOG_WEB_PORT=8025
EOF

    echo "✅ Development environment file created (.env.dev)"
}

# Function to create production environment template
create_prod_env_template() {
    echo "📝 Creating production environment template (.env.example)..."
    
    cat > .env.example << 'EOF'
# Takhlees Tech Backend - Production Environment Template
# INSTRUCTIONS:
# 1. Copy this file to .env
# 2. Replace REPLACE_WITH_* values with actual secure values
# 3. Never commit .env file to version control

ASPNETCORE_ENVIRONMENT=Production
MYSQL_VERSION=8.0

# Database Configuration - REPLACE WITH SECURE VALUES
ADMIN_DB_CONTAINER_NAME=admin-db
ADMIN_DB_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_1
ADMIN_DB_NAME=u676203545_logs
ADMIN_DB_USER=admin_user
ADMIN_DB_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_2
ADMIN_DB_PORT=3307

CUSTOMERSERVICE_DB_CONTAINER_NAME=customerservice-db
CUSTOMERSERVICE_DB_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_3
CUSTOMERSERVICE_DB_NAME=u676203545_CustomerServic
CUSTOMERSERVICE_DB_USER=customerservice_user
CUSTOMERSERVICE_DB_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_4
CUSTOMERSERVICE_DB_PORT=3308

FIRSTPROJECT_DB_CONTAINER_NAME=firstproject-db
FIRSTPROJECT_DB_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_5
FIRSTPROJECT_DB_NAME=u676203545_mainData
FIRSTPROJECT_DB_USER=firstproject_user
FIRSTPROJECT_DB_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_6
FIRSTPROJECT_DB_PORT=3309

USER_DB_CONTAINER_NAME=user-db
USER_DB_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_7
USER_DB_NAME=u676203545_Orders
USER_DB_USER=user_user
USER_DB_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_8
USER_DB_PORT=3310

HANGFIRE_DB_CONTAINER_NAME=hangfire-db
HANGFIRE_DB_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_9
HANGFIRE_DB_NAME=u676203545_HangFire
HANGFIRE_DB_USER=hangfire_user
HANGFIRE_DB_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_10
HANGFIRE_DB_PORT=3311

# phpMyAdmin Configuration
PHPMYADMIN_CONTAINER_NAME=phpmyadmin
PHPMYADMIN_ROOT_PASSWORD=REPLACE_WITH_SECURE_PASSWORD_11
PHPMYADMIN_PORT=8080

# JWT Configuration - CRITICAL: Replace with secure values
JWT_ISSUER=https://firstproject.takhleesak.com
JWT_AUDIENCE=https://firstproject.takhleesak.com
JWT_SECRET_KEY=REPLACE_WITH_SECURE_JWT_SECRET_KEY_MINIMUM_256_BITS

# Admin Configuration
ADMIN_SERVICE_CONTAINER_NAME=admin-service
ADMIN_SERVICE_PORT=5001
ADMIN_SUPER_ADMIN_EMAIL=superadmin@takhlees.com
ADMIN_SUPER_ADMIN_PASSWORD=REPLACE_WITH_SECURE_ADMIN_PASSWORD
ADMIN_DEFAULT_PAGE_SIZE=20
ADMIN_MAX_PAGE_SIZE=100

# Service Configuration
CUSTOMERSERVICE_SERVICE_CONTAINER_NAME=customerservice-service
CUSTOMERSERVICE_SERVICE_PORT=5002

FIRSTPROJECT_SERVICE_CONTAINER_NAME=firstproject-service
FIRSTPROJECT_SERVICE_PORT=9100
FIRSTPROJECT_SERVICE_INTERNAL_PORT=9100

USER_SERVICE_CONTAINER_NAME=user-service
USER_SERVICE_PORT=5003

# PayPal Configuration - Production
PAYPAL_CLIENT_ID=REPLACE_WITH_PRODUCTION_PAYPAL_CLIENT_ID
PAYPAL_SECRET=REPLACE_WITH_PRODUCTION_PAYPAL_SECRET
PAYPAL_IS_SANDBOX=false

# Email Configuration - Production
EMAIL_FROM=takhleesak@takhleesak.com
EMAIL_SMTP_SERVER=smtp.hostinger.com
EMAIL_PORT=587
EMAIL_USERNAME=takhleesak@takhleesak.com
EMAIL_PASSWORD=REPLACE_WITH_SECURE_EMAIL_PASSWORD
EMAIL_USE_SSL=false

# CORS Configuration
CORS_ALLOWED_ORIGINS=https://firstproject.takhleesak.com,https://admin.takhleesak.com,https://api.takhleesak.com
CORS_ALLOW_CREDENTIALS=true

# Nginx Configuration
NGINX_CONTAINER_NAME=nginx-proxy
NGINX_HTTP_PORT=8090
NGINX_HTTPS_PORT=8444
EOF

    echo "✅ Production environment template created (.env.example)"
}

# Main menu
show_menu() {
    echo ""
    echo "Please choose what you want to set up:"
    echo ""
    echo "1) 🛠️  Development environment only"
    echo "2) 🚀 Production environment template only"  
    echo "3) 📋 Both development and production templates"
    echo "4) 🏃 Quick start development environment"
    echo "5) ❌ Exit"
    echo ""
}

# Quick start function
quick_start() {
    echo "🏃 Quick Start Development Environment"
    echo "====================================="
    
    create_dev_env
    
    echo ""
    echo "🔨 Starting development environment..."
    echo "This may take a few minutes on first run..."
    
    # Start the development environment
    docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d
    
    echo ""
    echo "⏳ Waiting for services to start..."
    sleep 30
    
    echo ""
    echo "🎉 Development environment is ready!"
    echo ""
    echo "📋 Access your services:"
    echo "   🌐 Main Application: http://localhost:8090"
    echo "   🔧 phpMyAdmin:       http://localhost:8080"
    echo "   📧 MailHog:          http://localhost:8025"
    echo "   ⚙️  Admin Service:    http://localhost:5001"
    echo "   👥 User Service:     http://localhost:5003"
    echo ""
    echo "🔍 To view logs: docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev logs -f"
    echo "🛑 To stop:      docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev down"
}

# Main script
main() {
    while true; do
        show_menu
        read -p "Enter your choice (1-5): " choice
        
        case $choice in
            1)
                create_dev_env
                echo ""
                echo "🎯 Next steps:"
                echo "   1. Review and modify .env.dev if needed"
                echo "   2. Run: docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d"
                break
                ;;
            2)
                create_prod_env_template
                echo ""
                echo "⚠️  Next steps for production:"
                echo "   1. Copy .env.example to .env"
                echo "   2. Replace ALL 'REPLACE_WITH_*' values with secure production values"
                echo "   3. Never commit .env to version control"
                echo "   4. Set up GitHub Secrets for CI/CD"
                break
                ;;
            3)
                create_dev_env
                create_prod_env_template
                echo ""
                echo "🎯 Environment files created successfully!"
                echo ""
                echo "For development:"
                echo "   • .env.dev is ready to use"
                echo "   • Run: docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d"
                echo ""
                echo "For production:"
                echo "   • Copy .env.example to .env"
                echo "   • Replace REPLACE_WITH_* values with secure values"
                echo "   • Set up GitHub Secrets for automated deployment"
                break
                ;;
            4)
                quick_start
                break
                ;;
            5)
                echo "👋 Goodbye!"
                exit 0
                ;;
            *)
                echo "❌ Invalid choice. Please enter 1-5."
                ;;
        esac
    done
}

# Check if files already exist
echo "🔍 Checking existing environment files..."

if [[ -f ".env.dev" ]]; then
    echo "⚠️  .env.dev already exists"
    read -p "Do you want to overwrite it? (y/N): " overwrite
    if [[ $overwrite != "y" && $overwrite != "Y" ]]; then
        echo "❌ Skipping .env.dev creation"
        DEV_EXISTS=true
    fi
fi

if [[ -f ".env.example" ]]; then
    echo "⚠️  .env.example already exists"
    read -p "Do you want to overwrite it? (y/N): " overwrite
    if [[ $overwrite != "y" && $overwrite != "Y" ]]; then
        echo "❌ Skipping .env.example creation"
        PROD_EXISTS=true
    fi
fi

# Run main menu
main

echo ""
echo "✅ Setup completed successfully!"
echo ""
echo "📚 For more information, see:"
echo "   • DOCKER_DEPLOYMENT_GUIDE.md - Comprehensive deployment guide"
echo "   • README.md - Project overview"
echo ""
echo "🆘 Need help? Check the troubleshooting section in DOCKER_DEPLOYMENT_GUIDE.md" 