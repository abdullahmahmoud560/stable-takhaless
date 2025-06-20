# Takhlees Tech Backend - Microservices Architecture

## 🏗️ Project Overview

This is a comprehensive .NET 8 microservices backend system for Takhlees Tech, featuring 4 main services with their own databases, authentication, and containerized deployment.

### 📋 Services Architecture

| Service | Purpose | Port | Database | Domain |
|---------|---------|------|----------|---------|
| **FirstProject** | Authentication & User Management | 9100 | firstproject-db | firstproject.takhleesak.com |
| **Admin** | Admin Panel & Logging | 5001 | admin-db | admin.takhleesak.com |
| **CustomerService** | Customer Support | 5002 | customerservice-db | support.takhleesak.com |
| **User** | User Operations & Payments | 5003 | user-db + hangfire-db | api.takhleesak.com |

### 🛠️ Technology Stack

- **Backend**: .NET 8, ASP.NET Core Web API
- **Database**: MySQL 8.0
- **Authentication**: JWT Bearer Tokens
- **Containerization**: Docker & Docker Compose
- **Reverse Proxy**: Nginx
- **Database Management**: phpMyAdmin
- **Background Jobs**: Hangfire (User Service)
- **Payments**: PayPal Integration
- **Email**: SMTP with Hostinger
- **Rate Limiting**: AspNetCoreRateLimit

## 🚀 Quick Start

### Prerequisites

- Docker and Docker Compose installed
- Git installed
- SSL certificates for production (optional for development)

### 1. Clone and Setup

```bash
git clone <repository-url>
cd takhlees-tech-back
```

### 2. Start All Services

```bash
# Start all services (databases + applications + nginx)
./scripts/start-all.sh
```

### 3. Access Services

- **phpMyAdmin**: http://localhost:8080
- **FirstProject (Auth)**: http://localhost:9100
- **Admin Service**: http://localhost:5001
- **Customer Service**: http://localhost:5002
- **User Service**: http://localhost:5003
- **Nginx Proxy**: http://localhost:8090

## 📊 Service Details

### 🔐 FirstProject Service (Authentication Hub)
- **Primary Function**: User authentication and management
- **Features**: 
  - JWT token generation and validation
  - User registration/login
  - Password reset functionality
  - Two-factor authentication
  - Rate limiting on sensitive endpoints
- **Database**: u676203545_mainData

### 👑 Admin Service
- **Primary Function**: Administrative operations and system logging
- **Features**:
  - System logs management
  - Admin user management
  - Service monitoring
  - Audit trails
- **Database**: u676203545_logs

### 🎧 Customer Service
- **Primary Function**: Customer support and ticketing
- **Features**:
  - Support ticket management
  - File uploads (up to 10MB)
  - Email notifications
  - Customer interaction tracking
- **Database**: u676203545_CustomerServic

### 👤 User Service
- **Primary Function**: User operations and payment processing
- **Features**:
  - Order management
  - PayPal payment integration
  - Background job processing (Hangfire)
  - File downloads
  - Real-time notifications (SignalR)
- **Databases**: u676203545_Orders, u676203545_HangFire

## 🔧 Configuration

### Environment Variables

All services use environment variables stored in `*/environment.env` files:

#### JWT Configuration (Updated for Production)
```env
JWT__Issuer=https://firstproject.takhleesak.com
JWT__Audience=https://firstproject.takhleesak.com
JWT__SecretKey=Z1mRne9MKIvOSeLb4wNBzpOcV1mW0qeoypr1W-AC5K7IN29SvFgRzC1QffZgF4Kw2X_aV_ZXRpppTec2kEwU2Q
```

#### Database Configuration
Each service has its own MySQL database with dedicated credentials.

#### CORS Configuration
```env
CORS__AllowedOrigins=https://firstproject.takhleesak.com,https://admin.takhleesak.com,https://api.takhleesak.com
```

## 🗄️ Database Structure

### MySQL Databases
- **admin-db** (Port 3307): Logging and admin data
- **customerservice-db** (Port 3308): Customer service tickets and forms
- **firstproject-db** (Port 3309): User accounts and authentication
- **user-db** (Port 3310): Orders and user operations
- **hangfire-db** (Port 3311): Background job processing

### phpMyAdmin Access
- **URL**: http://localhost:8080
- **Servers**: All 5 databases are accessible
- **Login**: Use the database credentials from environment files

## 🔨 Development Commands

### Start/Stop Services
```bash
# Start all services
./scripts/start-all.sh

# Stop all services
./scripts/stop-all.sh

# View logs from all services
./scripts/logs.sh

# View logs from specific service
./scripts/logs.sh firstproject-service
```

### Docker Compose Commands
```bash
# Start specific service
docker-compose up -d firstproject-service

# Rebuild and start
docker-compose up -d --build firstproject-service

# View logs
docker-compose logs -f firstproject-service

# Stop and remove volumes (CAUTION: This deletes all data)
docker-compose down -v
```

## 🌐 Production Deployment

### Domain Configuration
The system is configured for the following domains:
- **firstproject.takhleesak.com**: Main authentication service
- **admin.takhleesak.com**: Admin panel
- **support.takhleesak.com**: Customer service
- **api.takhleesak.com**: User API

### SSL Certificates
Place your SSL certificates in `nginx/ssl/`:
- `takhleesak.com.crt`
- `takhleesak.com.key`

### Environment Setup
1. Update all `environment.env` files with production values
2. Configure proper SMTP settings
3. Update PayPal credentials for production
4. Set secure database passwords

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Rate Limiting**: Protection against API abuse
- **CORS Configuration**: Controlled cross-origin requests
- **SSL/TLS**: HTTPS enforcement in production
- **Database Security**: Isolated databases with unique credentials
- **Input Validation**: Comprehensive data validation
- **Security Headers**: Nginx security headers

## 📈 Monitoring and Logging

### Application Logging
- **Serilog**: Structured logging across all services
- **Database Logging**: Centralized log storage
- **Request/Response Logging**: API call tracking

### Health Checks
- **Health Endpoint**: `/health` for service monitoring
- **Database Health**: Connection status monitoring
- **Service Dependencies**: Inter-service communication checks

## 🚨 Troubleshooting

### Common Issues

1. **Database Connection Issues**
   ```bash
   # Check database status
   docker-compose logs admin-db
   
   # Restart database
   docker-compose restart admin-db
   ```

2. **Service Not Starting**
   ```bash
   # Check service logs
   ./scripts/logs.sh [service-name]
   
   # Rebuild service
   docker-compose up -d --build [service-name]
   ```

3. **Port Conflicts**
   - Check if ports are already in use
   - Modify ports in `docker-compose.yml` if needed

### Log Locations
- **Application Logs**: Available through `docker-compose logs`
- **Database Logs**: Stored in Docker volumes
- **Nginx Logs**: Available in nginx container

## 📚 API Documentation

Each service exposes Swagger documentation:
- **FirstProject**: http://localhost:9100/swagger
- **Admin**: http://localhost:5001/swagger
- **CustomerService**: http://localhost:5002/swagger
- **User**: http://localhost:5003/swagger

## 🤝 Contributing

1. Create feature branches for new development
2. Follow .NET coding standards
3. Add appropriate tests
4. Update documentation
5. Ensure all services start successfully

## 📞 Support

For technical support or questions about this microservices architecture, please contact the development team.

---

**Last Updated**: $(date)
**Version**: 1.0.0
**Maintainer**: Takhlees Tech Development Team 