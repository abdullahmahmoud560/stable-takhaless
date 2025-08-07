# 🚀 Takhlees Tech - Docker Deployment Guide

This guide covers the production-grade Docker Compose setup with environment variable management and CI/CD workflows.

## 📋 Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Environment Configuration](#environment-configuration)
- [Development Setup](#development-setup)
- [Production Deployment](#production-deployment)
- [CI/CD Workflows](#cicd-workflows)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

The new deployment system provides:

- **Environment-based configuration** using `.env` files
- **Separate development and production environments**
- **Automated CI/CD workflows** for both environments
- **Health checks and monitoring** built-in
- **Security-first approach** with secrets management

### Architecture Components

```
🏗️ Infrastructure
├── 🗄️  MySQL Databases (5 instances)
│   ├── Admin DB
│   ├── Customer Service DB
│   ├── First Project DB
│   ├── User DB
│   └── HangFire DB
├── 🌐 Application Services (4 services)
│   ├── Admin Service (Port 5001)
│   ├── Customer Service (Port 5002)
│   ├── First Project Service (Port 9100)
│   └── User Service (Port 5003)
├── 🔄 Reverse Proxy (Nginx)
├── 🔧 phpMyAdmin (Database Management)
└── 📧 MailHog (Development Email Testing)
```

## ⚡ Quick Start

### Prerequisites

- Docker and Docker Compose installed
- Git repository access
- Environment files configured

### Development Environment

```bash
# 1. Clone the repository
git clone <repository-url>
cd takhles-back-stable

# 2. Create development environment file
cp .env.dev.example .env.dev  # Create this file with development values

# 3. Start development environment
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d

# 4. Access services
open http://localhost:8090  # Main application
open http://localhost:8080  # phpMyAdmin
open http://localhost:8025  # MailHog (Email testing)
```

### Production Environment

```bash
# 1. Create production environment file (SECURE VALUES!)
cp .env.example .env  # Configure with production secrets

# 2. Deploy to production
docker-compose --env-file .env up -d

# 3. Verify deployment
docker-compose ps
```

## 🔧 Environment Configuration

### Environment Files Structure

```
📁 Environment Files
├── .env.example          # Production template (commit to git)
├── .env                  # Production secrets (DO NOT commit)
├── .env.dev.example      # Development template (commit to git)
└── .env.dev              # Development config (safe to commit)
```

### Required Environment Variables

#### Database Configuration
```env
# Admin Database
ADMIN_DB_ROOT_PASSWORD=secure_password_here
ADMIN_DB_NAME=u676203545_logs
ADMIN_DB_USER=admin_user
ADMIN_DB_PASSWORD=secure_password_here

# Customer Service Database  
CUSTOMERSERVICE_DB_ROOT_PASSWORD=secure_password_here
CUSTOMERSERVICE_DB_NAME=u676203545_CustomerServic
CUSTOMERSERVICE_DB_USER=customerservice_user
CUSTOMERSERVICE_DB_PASSWORD=secure_password_here

# ... (repeat for other databases)
```

#### Application Configuration
```env
# JWT Settings
JWT_ISSUER=https://firstproject.takhleesak.com
JWT_AUDIENCE=https://firstproject.takhleesak.com
JWT_SECRET_KEY=your_super_secure_jwt_secret_256_bits_minimum

# PayPal Configuration
PAYPAL_CLIENT_ID=your_paypal_client_id
PAYPAL_SECRET=your_paypal_secret
PAYPAL_IS_SANDBOX=false  # true for development

# Email Configuration
EMAIL_FROM=takhleesak@takhleesak.com
EMAIL_SMTP_SERVER=smtp.hostinger.com
EMAIL_USERNAME=takhleesak@takhleesak.com
EMAIL_PASSWORD=your_email_password
```

## 🛠️ Development Setup

### Development Features

- **Hot reload** with source code mounting
- **MailHog** for email testing
- **Separate databases** from production
- **Development-friendly defaults**
- **Debug logging enabled**

### Development Commands

```bash
# Start development environment
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d

# View logs
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev logs -f

# Restart specific service
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev restart admin-service

# Stop development environment
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev down

# Clean up development data
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev down -v
```

### Development URLs

| Service | URL | Description |
|---------|-----|-------------|
| Main App | http://localhost:8090 | Nginx reverse proxy |
| Admin Service | http://localhost:5001 | Admin API |
| Customer Service | http://localhost:5002 | Customer Service API |
| First Project | http://localhost:9100 | Authentication API |
| User Service | http://localhost:5003 | User management API |
| phpMyAdmin | http://localhost:8080 | Database management |
| MailHog | http://localhost:8025 | Email testing UI |

## 🚀 Production Deployment

### Production Security Features

- **Secrets management** via GitHub Secrets
- **Health checks** for all services
- **Backup creation** before deployment
- **Zero-downtime deployment** strategy
- **SSL/TLS** configuration ready
- **Resource monitoring** included

### Manual Production Deployment

```bash
# 1. Ensure .env file has production secrets
# 2. Deploy services
docker-compose --env-file .env up --build -d

# 3. Wait for services to be ready
docker-compose ps

# 4. Run health checks
./scripts/health-check.sh  # If available

# 5. Monitor logs
docker-compose logs -f
```

### Production Monitoring

```bash
# Check service status
docker-compose ps

# View resource usage
docker stats

# Check logs for errors
docker-compose logs --tail=100 | grep -i error

# Database connectivity test
docker-compose exec admin-db mysql -u admin_user -p -e "SELECT 1"
```

## 🔄 CI/CD Workflows

### Development Workflow (`dev-deploy.yml`)

**Trigger:** Push to `dev` branch

**Steps:**
1. 🔍 Pre-deployment checks
2. 🧹 Clean up existing containers
3. 🔨 Build and deploy services
4. ⏳ Wait for services to be ready
5. 🔍 Verify deployment
6. 📋 Generate deployment summary

**Usage:**
```bash
# Automatic trigger
git push origin dev

# Manual trigger (GitHub UI)
# Go to Actions → Development Deployment → Run workflow
```

### Production Workflow (`prod-deploy.yml`)

**Trigger:** Push to `main` branch

**Steps:**
1. 🔒 Security and validation checks
2. 💾 Create production backup
3. 🔐 Generate secure environment from secrets
4. 🚀 Deploy to production
5. 🧪 Run smoke tests
6. 📊 Post-deployment monitoring

**Usage:**
```bash
# Automatic trigger
git push origin main

# Manual trigger with options
# Go to Actions → Production Deployment → Run workflow
# Options: Force deploy, Backup before deploy
```

## 🔒 Security Best Practices

### Environment Variables Security

✅ **DO:**
- Use GitHub Secrets for production values
- Rotate passwords regularly
- Use strong, unique passwords
- Enable database SSL connections
- Use HTTPS in production

❌ **DON'T:**
- Commit `.env` files to git
- Use default passwords
- Share production credentials
- Use development settings in production
- Store secrets in plain text

### GitHub Secrets Setup

Required secrets for production workflow:

```
Database Secrets:
- ADMIN_DB_ROOT_PASSWORD
- ADMIN_DB_PASSWORD
- CUSTOMERSERVICE_DB_ROOT_PASSWORD
- CUSTOMERSERVICE_DB_PASSWORD
- FIRSTPROJECT_DB_ROOT_PASSWORD
- FIRSTPROJECT_DB_PASSWORD
- USER_DB_ROOT_PASSWORD
- USER_DB_PASSWORD
- HANGFIRE_DB_ROOT_PASSWORD
- HANGFIRE_DB_PASSWORD

Application Secrets:
- JWT_SECRET_KEY
- ADMIN_SUPER_ADMIN_PASSWORD
- PAYPAL_CLIENT_ID
- PAYPAL_SECRET
- EMAIL_PASSWORD

Configuration:
- JWT_ISSUER
- JWT_AUDIENCE
- CORS_ALLOWED_ORIGINS
- EMAIL_FROM
- EMAIL_SMTP_SERVER
- EMAIL_USERNAME
```

### SSL/TLS Configuration

For production HTTPS:

1. **Obtain SSL certificates**
2. **Place certificates in `nginx/ssl/`**
3. **Update nginx configuration**
4. **Set `NGINX_HTTPS_PORT=443`**

## 🐛 Troubleshooting

### Common Issues

#### Services Not Starting

```bash
# Check container logs
docker-compose logs <service-name>

# Check resource usage
docker system df
docker system prune  # Clean up if needed

# Verify environment variables
docker-compose config
```

#### Database Connection Issues

```bash
# Test database connectivity
docker-compose exec admin-db mysql -u admin_user -p

# Check database logs
docker-compose logs admin-db

# Verify environment variables
echo $ADMIN_DB_PASSWORD  # Should show password
```

#### Port Conflicts

```bash
# Check port usage
netstat -tulpn | grep :<port>

# Kill process using port
sudo kill -9 $(lsof -t -i:<port>)

# Use different ports in .env file
ADMIN_SERVICE_PORT=5011  # Instead of 5001
```

### Health Check Commands

```bash
# Test all service endpoints
curl http://localhost:8090/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:9100/health
curl http://localhost:5003/health

# Test database connections
docker-compose exec admin-db mysqladmin ping
docker-compose exec user-db mysqladmin ping

# Check phpMyAdmin
curl http://localhost:8080
```

### Performance Optimization

```bash
# Monitor resource usage
docker stats

# Optimize images
docker system prune -a

# Scale services if needed
docker-compose up -d --scale user-service=2
```

## 📞 Support

### Getting Help

1. **Check logs:** `docker-compose logs <service>`
2. **Review this guide:** Common solutions above
3. **Check GitHub Actions:** Review failed workflow logs
4. **Contact team:** Use internal support channels

### Useful Commands Reference

```bash
# Environment management
docker-compose config                    # Validate configuration
docker-compose ps                        # List running containers
docker-compose logs -f <service>         # Follow logs for service
docker-compose exec <service> /bin/bash  # Access container shell
docker-compose down && docker-compose up -d  # Restart all services

# Database management
docker-compose exec admin-db mysql -u admin_user -p
docker-compose exec user-db mysqldump -u user_user -p dev_Orders > backup.sql

# Development helpers
docker-compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d
docker system prune -f --volumes  # Clean up development data
```

---

## 🎉 Congratulations!

You now have a production-grade Docker deployment system with:

- ✅ Environment-based configuration
- ✅ Automated CI/CD workflows  
- ✅ Security best practices
- ✅ Development and production environments
- ✅ Health monitoring and checks
- ✅ Comprehensive documentation

**Happy deploying! 🚀** 