# Takhleesak API - Project Documentation

## 📋 Project Overview

**Takhleesak API** is a comprehensive ASP.NET Core 8.0 web application built using Onion Architecture principles. This document provides essential information about the project structure, architecture, and key components.

## 🏗️ Architecture Overview

### Onion Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Controllers   │  │   Middleware    │  │   Program    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Interfaces    │  │   Services      │  │   DTOs       │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Data Access   │  │   External APIs │  │   Services  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Entities      │  │   Interfaces    │  │   Models     │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

```
firstProject/
├── Application/                    # Application Layer
│   ├── Application.csproj
│   ├── Interface/                 # Service Interfaces
│   │   ├── IEmailService.cs
│   │   ├── IFunctionService.cs
│   │   ├── IServiceManager.cs
│   │   └── ITokenService.cs
│   └── Service/                   # Application Services
│       └── EmailService.cs
│
├── Domain/                        # Domain Layer
│   ├── Domain.csproj
│   ├── Entities/                  # Domain Entities
│   ├── Interface/                 # Domain Interfaces
│   └── Models/                    # Domain Models
│
├── Infrastructure/                # Infrastructure Layer
│   ├── Infrastructure.csproj
│   ├── ApplicationDbContext/      # Database Context
│   │   ├── DB.cs
│   │   └── RepositoryContextFactory.cs
│   ├── Exceptions/                # Exception Handling
│   │   ├── ExceptionMiddleware.cs
│   │   └── ExceptionMiddlewareExtensions.cs
│   ├── Extensions/                # Service Extensions
│   │   └── ServiceExtensions.cs
│   ├── Identities/                # Identity Models
│   │   ├── User.cs
│   │   └── TwoFactorVerify.cs
│   ├── Migrations/                # EF Core Migrations
│   ├── Repositories/              # Data Access Layer
│   │   ├── IUserRepository.cs
│   │   └── UserRepository.cs
│   ├── Services/                  # Infrastructure Services
│   │   ├── FunctionService.cs
│   │   ├── TokenService.cs
│   │   ├── UserService.cs
│   │   └── ServiceManager.cs
│   └── Validation/                # Input Validation
│       └── InputSanitizer.cs
│
├── firstProject/                  # Presentation Layer
│   ├── firstProject.csproj
│   ├── Controllers/               # API Controllers
│   │   ├── AdminController.cs
│   │   ├── LoginController.cs
│   │   ├── PasswordController.cs
│   │   ├── SignupController.cs
│   │   ├── VerifyEmail.cs
│   │   └── SelectController.cs
│   ├── MappingProfiles/           # AutoMapper Profiles
│   │   └── UserProfile.cs
│   ├── Program.cs                 # Application Entry Point
│   └── appsettings.json          # Configuration
│
├── Shared/                        # Shared DTOs
│   ├── Shared.csproj
│   └── DataTransferObject.cs
│
├── Dockerfile                     # Docker Configuration
├── environment.env               # Environment Variables
└── firstProject.sln             # Solution File
```

## 🔧 Technology Stack

### Core Technologies
- **.NET 8.0** - Latest LTS version
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM
- **MySQL** - Database (via Pomelo.EntityFrameworkCore.MySql)

### Key Packages
```xml
<!-- Authentication & Authorization -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.20" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.20" />

<!-- Database -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.20" />

<!-- Security & Rate Limiting -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />

<!-- Utilities -->
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="DotNetEnv" Version="3.1.1" />

<!-- External Services -->
<PackageReference Include="PayPalCheckoutSdk" Version="1.0.4" />
<PackageReference Include="Vonage" Version="7.14.0" />

<!-- Development -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.HealthChecks" Version="2.2.0" />
```

## 🎯 Key Features

### Authentication & Authorization
- **JWT Bearer Token** authentication
- **Role-based authorization** (Admin, Manager, User, Company, Broker, etc.)
- **Account lockout** after failed attempts
- **Password policies** with complexity requirements

### Security Features
- **XSS protection** via InputSanitizer
- **SQL injection prevention**
- **Rate limiting** (10 requests/minute)
- **Security headers** middleware
- **Input validation** with Data Annotations

### Performance Optimizations
- **Connection pooling** (100 connections)
- **Query splitting** for complex queries
- **AsNoTracking** for read-only operations
- **Batch operations** (MaxBatchSize: 100)
- **Memory caching** with 5-minute expiration

### API Endpoints
- **User Registration** (User, Company, Broker)
- **Authentication** (Login, Logout)
- **Email Verification** (Two-factor authentication)
- **Password Management** (Reset, Change)
- **Admin Functions** (User management, Statistics)
- **Data Selection** (User data retrieval)

## 🔐 Security Implementation
### Rate Limiting
- **IP-based limiting** (10 requests/minute)
- **Endpoint-specific limits**
- **Automatic blocking** of abusive requests

## 📊 Database Design

### Entity Framework Core
- **Code First** approach
- **Migrations** for database schema
- **Connection pooling** for performance
- **Query optimization** with AsNoTracking

### Key Entities
- **User** - Main user entity with Identity
- **TwoFactorVerify** - Email verification codes
- **Role** - User roles and permissions

## 🚀 Application Flow

### Registration Flow
1. **Input Validation** - Data sanitization
2. **Duplicate Check** - Email/Phone/Identity validation
3. **User Creation** - Account creation with role assignment
4. **Email Verification** - Send verification code
5. **Token Generation** - Generate JWT token
6. **Cookie Setting** - Set authentication cookie

### Login Flow
1. **Credential Validation** - Email/Password check
2. **Account Status** - Check if blocked/active
3. **Verification Code** - Generate and send code
4. **Token Generation** - Create JWT token
5. **Authentication** - Set cookies and headers

### Verification Flow
1. **Code Validation** - Verify entered code
2. **Email Activation** - Activate user email
3. **Token Refresh** - Generate new access token
4. **Role Assignment** - Assign user roles

## 🔧 Configuration

### Environment Variables
- **Database Connection** - MySQL connection string
- **JWT Settings** - Secret key, issuer, audience
- **Email Configuration** - SMTP settings
- **CORS Settings** - Allowed origins
- **Rate Limiting** - Request limits

### Application Settings
- **Security Headers** - XSS, CSRF protection
- **Caching** - Memory cache configuration
- **Health Checks** - System monitoring
- **Swagger** - API documentation

## 📝 API Documentation

### Swagger Integration
- **Auto-generated** API documentation
- **Endpoint descriptions** and examples
- **Request/Response** schemas
- **Authentication** requirements

### Available Endpoints
- **Authentication**: `/api/Login`, `/api/Register-*`
- **Verification**: `/api/VerifyCode`, `/api/Resend-Code`
- **Password**: `/api/Forget-Password`, `/api/Reset-Password`
- **Admin**: `/api/Get-User`, `/api/Blocked-User`, `/api/Statistics`
- **Data**: `/api/Select-Data`, `/api/Get-Information`

## 🏥 Health Monitoring

### Health Checks
- **Database connectivity** check
- **Memory usage** monitoring
- **Application status** verification
- **Endpoint availability** testing

### Monitoring Endpoints
- **Health**: `/health`
- **Status**: Application status monitoring
- **Metrics**: Performance metrics

--------------------------------------------
**Last Updated**: January 2025  
**Version**: 1.0.0  
**Maintainer**: Development Team
