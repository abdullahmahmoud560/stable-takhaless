# Takhleesak API - Services Documentation

## 📋 Overview

This document provides comprehensive documentation for all services in the Takhleesak API project. Each service is designed to handle specific business logic and operations within the Onion Architecture framework.

## 🏗️ Service Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Service Manager                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │  Email Service  │  │  Token Service  │  │Function Svc │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    User Service                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ User Management │  │ Authentication  │  │ Authorization│ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Repository Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ User Repository │  │ Data Access     │  │ EF Core     │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 Service Manager

### Overview
The `ServiceManager` acts as a central hub that manages and provides access to all core services in the application. It implements the **Service Locator Pattern** and uses **Lazy Loading** for optimal performance.

### Interface
```csharp
public interface IServiceManager
{
    public IEmailService EmailService { get; }
    public ITokenService TokenService { get; }
    public IFunctionService FunctionService { get; }
}
```

### Implementation
```csharp
public class ServiceManager : IServiceManager
{
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<ITokenService> _tokenService;
    private readonly Lazy<IFunctionService> _functionService;
    
    public ServiceManager(UserManager<User> userManager, IMapper mapper, DB db, 
                         IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
    {
        _emailService = new Lazy<IEmailService>(() => new EmailService());
        _tokenService = new Lazy<ITokenService>(() => new TokenService(userManager));
        _functionService = new Lazy<IFunctionService>(() => new FunctionService(db, httpContextAccessor, httpClient));
    }
}
```

### Key Features
- **Lazy Loading**: Services are instantiated only when first accessed
- **Dependency Injection**: Properly configured with DI container
- **Service Locator**: Centralized access to all services
- **Performance Optimization**: Reduces memory footprint

---

## 📧 Email Service

### Overview
The `EmailService` handles all email-related operations including sending verification codes, notifications, and system emails.

### Interface
```csharp
public interface IEmailService
{
    public Task<(bool Success, string Error)> SendEmailAsync(string toEmail, string subject, string htmlBody);
}
```

### Implementation
```csharp
public class EmailService : IEmailService
{
    public async Task<(bool Success, string Error)> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            // SMTP configuration and email sending logic
            // Returns success/failure with error message
        }
        catch (Exception ex)
        {
            return (false, "حدث خطأ اثناء توليد الكود");
        }
    }
}
```

### Key Features
- **SMTP Integration**: Uses Hostinger SMTP server
- **HTML Email Support**: Sends rich HTML formatted emails
- **Error Handling**: Comprehensive error handling and logging
- **Async Operations**: Non-blocking email operations

### Email Types
1. **Verification Emails**: Account activation codes
2. **Login Verification**: Two-factor authentication codes
3. **Password Reset**: Password reset links and codes
4. **System Notifications**: Important system updates

---

## 🔐 Token Service

### Overview
The `TokenService` manages JWT token generation, validation, and token lifecycle management for authentication and authorization.

### Interface
```csharp
public interface ITokenService
{
    public Task<(bool Success, string Error)> GenerateAccessToken(string Email);
    public Task<(bool Success, string Error)> GenerateActiveToken(string Email);
}
```

### Implementation
```csharp
public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;

    public async Task<(bool Success, string Error)> GenerateAccessToken(string Email)
    {
        // JWT token generation logic
        // Includes user claims, roles, and expiration
    }

    public async Task<(bool Success, string Error)> GenerateActiveToken(string Email)
    {
        // Temporary token generation for email verification
        // Shorter expiration time
    }
}
```

### Key Features
- **JWT Bearer Tokens**: Industry-standard authentication
- **Role-Based Claims**: Includes user roles in token claims
- **Token Expiration**: Configurable expiration times
- **User Validation**: Validates user existence before token generation

### Token Types
1. **Access Tokens**: Long-lived tokens for API access (7 days)
2. **Active Tokens**: Short-lived tokens for email verification (1 hour)

### Claims Included
- **User ID**: Unique user identifier
- **Email**: User email address
- **Full Name**: User's full name
- **Phone Number**: User's phone number
- **Roles**: User roles and permissions
- **JTI**: Unique token identifier

---

## ⚙️ Function Service

### Overview
The `FunctionService` handles utility functions including verification code generation, API integrations, and system operations.

### Interface
```csharp
public interface IFunctionService
{
    public Task<string> GenerateVerifyCode(string Email, string typeOfGenerate);
    Task<JsonElement?> SendAPI(string ID);
}
```

### Implementation
```csharp
public class FunctionService : IFunctionService
{
    private readonly DB _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public async Task<string> GenerateVerifyCode(string Email, string typeOfGenerate)
    {
        // Verification code generation logic
        // Rate limiting and validation
        // Database storage of codes
    }

    public async Task<JsonElement?> SendAPI(string ID)
    {
        // External API integration
        // Data processing and response handling
    }
}
```

### Key Features
- **Verification Code Generation**: Secure 6-digit codes
- **Rate Limiting**: Prevents code spam (2-minute intervals)
- **Database Storage**: Stores codes with expiration
- **External API Integration**: Handles third-party services

### Verification Code Types
1. **VerifyUserEmail**: User account verification
2. **VerifyCompanyEmail**: Company account verification
3. **VerifyBrokerEmail**: Broker account verification
4. **VerifyLogin**: Login verification
5. **ForgetPassword**: Password reset verification

---

## 👤 User Service

### Overview
The `UserService` is the core business logic service that handles all user-related operations including authentication, authorization, user management, and data operations.

### Interface
```csharp
public interface IUserService
{
    // User Management
    Task<(bool Success, string Error)> CreateAsync(BrokerDTO user, string password);
    Task<User?> FindByEmailAsync(string Email);
    Task<User?> FindByIdAsync(string Id);
    Task<IdentityResult> UpdateAsync(User user);
    
    // Authentication & Authorization
    Task<(bool Success, string Error)> LoginUser(LoginDTO loginDTO);
    Task<IList<string>> GetRole(string Email);
    Task<(bool Success, string Error)> changeRoles(ChangeRolesDTO roles);
    
    // Data Operations
    Task<List<UserDTO>> GetByPaging(int PageNumber, int PageSize, string targetRole);
    Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber, int PageSize);
    
    // Validation
    Task<(bool Success, string Error)> CheckIsFoundEmailOrPhoneOrIdentity(string Email, string Phone, string Identity);
    Task<(bool Success, string Error)> CheckIsFoundtaxRecordOrInsuranceNumber(string InsuranceNumber, string taxRecord, string license);
    
    // Password Management
    Task<(bool Success, string Error)> ResetPassword(ResetPasswordDTO reset, string Id);
    
    // Email Verification
    Task<(bool Success, string Error)> VerifyCode(VerifyCode verifyCode, string Email);
    Task<(bool Success, string Error)> ActiveEmail(string Email);
    
    // User Status Management
    Task<(bool Success, string Error)> Blocked(string Email);
    Task<(bool Success, string Error)> UnBlocked(string Email);
    
    // Permissions Management
    Task<(bool Success, string Message, IEnumerable<string>? AddedClaims)> SetPermissionsAsync(RolesDTO rolesDTO);
    Task<(bool Success, string Message, IEnumerable<string>? Claims)> GetPermissionsAsync(string userId);
    Task<(bool Success, string Message, IEnumerable<string>? RemainingClaims)> DeletePermissionsAsync(RolesDTO rolesDTO);
    
    // Statistics
    Task<int> GetUserCountByRole(string role);
}
```

### Key Features

#### 1. User Management
- **User Creation**: Creates users with role assignment
- **User Retrieval**: Finds users by email or ID
- **User Updates**: Updates user information
- **Role Management**: Assigns and changes user roles

#### 2. Authentication & Authorization
- **Login Validation**: Validates user credentials
- **Account Lockout**: Implements failed attempt protection
- **Role-Based Access**: Manages user permissions
- **Session Management**: Handles user sessions

#### 3. Data Operations
- **Pagination**: Efficient data pagination
- **Conditional Queries**: Flexible data filtering
- **Role-Based Filtering**: Filters users by roles
- **Statistics**: User count by role

#### 4. Security Features
- **Input Validation**: Validates all user inputs
- **Duplicate Prevention**: Prevents duplicate registrations
- **Account Security**: Manages account status
- **Permission Management**: Granular permission control

### User Roles
1. **Admin**: Full system access
2. **Manager**: Management functions
3. **User**: Basic user access
4. **Company**: Company-specific access
5. **Broker**: Broker-specific access
6. **CustomerService**: Customer service access
7. **Account**: Account management access

---

## 🗄️ User Repository

### Overview
The `UserRepository` handles all data access operations for user-related data. It implements the **Repository Pattern** and provides a clean abstraction over Entity Framework Core.

### Interface
```csharp
public interface IUserRepository
{
    Task<(bool Success, string Error)> CreateAsync(User user, string password);
    Task<User?> FindByEmailAsync(string Email);
    Task<User?> FindByIdAsync(string Id);
    Task<IdentityResult> UpdateAsync(User user);
    Task<List<User>?> GetByPaging(int PageNumber, int PageSize);
    Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber, int PageSize);
    Task<IList<string>> GetRole(User user);
    Task<(bool Success, string Error)> changeRoles(ChangeRolesDTO roles);
}
```

### Key Features
- **Data Access Abstraction**: Clean separation from business logic
- **Entity Framework Integration**: Uses EF Core for database operations
- **Async Operations**: Non-blocking database operations
- **Error Handling**: Comprehensive error handling
- **Performance Optimization**: Uses AsNoTracking for read operations

---

## 🔒 Security Services

### Authentication Middleware
```csharp
public class AuthenticationMiddleware
{
    public async Task Invoke(HttpContext context)
    {
        // JWT token validation
        // User status checking (blocked/active)
        // Cookie management
        // Request authorization
    }
}
```

### Input Sanitization
```csharp
public static class InputSanitizer
{
    public static string SanitizeString(string? input);
    public static string SanitizeEmail(string? email);
    public static string SanitizePhoneNumber(string? phone);
    public static string SanitizeIdentity(string? identity);
    public static string SanitizeName(string? name);
}
```

### Key Security Features
- **XSS Protection**: Prevents cross-site scripting attacks
- **SQL Injection Prevention**: Protects against SQL injection
- **Input Validation**: Validates all user inputs
- **Rate Limiting**: Prevents brute force attacks
- **Account Lockout**: Protects against failed login attempts

---

## 📊 Service Dependencies

### Dependency Graph
```
ServiceManager
├── EmailService
├── TokenService
│   └── UserManager<User>
└── FunctionService
    ├── DB (DbContext)
    ├── IHttpContextAccessor
    └── HttpClient

UserService
├── IUserRepository
├── UserManager<User>
├── SignInManager<User>
├── IMapper
└── DB (DbContext)
```

### Service Registration
```csharp
// In Program.cs
builder.Services.ConfigureUserService();
builder.Services.ConfigureServiceManager();

// In ServiceExtensions.cs
public static void ConfigureUserService(this IServiceCollection services) =>
    services.AddScoped<IUserService, UserService>();

public static void ConfigureServiceManager(this IServiceCollection services) =>
    services.AddScoped<IServiceManager, ServiceManager>();
```

---

## 🚀 Usage Examples

### User Registration
```csharp
// In Controller
var result = await _userService.CreateAsync(userDTO, password);
if (result.Success)
{
    var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(email, "VerifyUserEmail");
    var emailResult = await _serviceManager.EmailService.SendEmailAsync(email, "Verification", htmlBody);
}
```

### User Authentication
```csharp
// In Controller
var loginResult = await _userService.LoginUser(loginDTO);
if (loginResult.Success)
{
    var token = await _serviceManager.TokenService.GenerateAccessToken(email);
    // Set authentication cookie
}
```

### Email Verification
```csharp
// In Controller
var verifyResult = await _userService.VerifyCode(verifyCode, userId);
if (verifyResult.Success)
{
    var activeResult = await _userService.ActiveEmail(email);
    var token = await _serviceManager.TokenService.GenerateAccessToken(email);
}
```

---

## 🔧 Configuration

### Service Configuration
```csharp
// Email Configuration
EmailConfiguration__From=takhleesak@takhleesak.com
EmailConfiguration__SmtpServer=smtp.hostinger.com
EmailConfiguration__Port=587
EmailConfiguration__Username=takhleesak@takhleesak.com
EmailConfiguration__Password=Takhleesak@12345

// JWT Configuration
JWT__Issuer=https://firstproject.takhleesak.com
JWT__Audience=https://firstproject.takhleesak.com
JWT__SecretKey=Z1mRne9MKIvOSeLb4wNBzpOcV1mW0qeoypr1W-AC5K7IN29SvFgRzC1QffZgF4Kw2X_aV_ZXRpppTec2kEwU2Q

// Rate Limiting
IpRateLimiting__GeneralRules__Limit=10
IpRateLimiting__GeneralRules__Period=1m
```

---

## 🚨 Error Handling

### Error Response Format
```csharp
public class ApiResponse
{
    public string? Message { get; set; }
    public Object? Data { get; set; }
    public string? State { get; set; }
}
```

### Common Error Scenarios
1. **User Not Found**: When user doesn't exist
2. **Invalid Credentials**: Wrong email/password
3. **Account Blocked**: User account is blocked
4. **Email Already Exists**: Duplicate email registration
5. **Invalid Verification Code**: Wrong or expired code
6. **Rate Limit Exceeded**: Too many requests

---

## 📈 Performance Considerations

### Optimization Strategies
1. **Lazy Loading**: Services loaded on demand
2. **Connection Pooling**: Database connection optimization
3. **Async Operations**: Non-blocking operations
4. **Caching**: Memory cache for frequently accessed data
5. **Query Optimization**: Efficient database queries

### Monitoring
- **Health Checks**: Service health monitoring
- **Performance Metrics**: Response time tracking
- **Error Logging**: Comprehensive error logging
- **Usage Statistics**: Service usage analytics

---

## 🔄 Service Lifecycle

### Service Initialization
1. **Dependency Injection**: Services registered in DI container
2. **Lazy Loading**: Services instantiated on first access
3. **Configuration**: Services configured with settings
4. **Database Connection**: Database connections established

### Service Operations
1. **Request Processing**: Handle incoming requests
2. **Business Logic**: Execute business operations
3. **Data Access**: Interact with database
4. **Response Generation**: Return processed results

### Service Cleanup
1. **Resource Disposal**: Clean up resources
2. **Connection Closing**: Close database connections
3. **Memory Cleanup**: Release memory resources
4. **Logging**: Log service operations

---

## 📞 Support & Maintenance

### Service Monitoring
- **Health Endpoints**: `/health` for service status
- **Logging**: Comprehensive service logging
- **Metrics**: Performance and usage metrics
- **Alerts**: Automated error notifications

### Maintenance Tasks
- **Database Migrations**: Schema updates
- **Service Updates**: Feature enhancements
- **Security Updates**: Security patches
- **Performance Tuning**: Optimization improvements

---

**Last Updated**: January 2025  
**Version**: 1.0.0  
**Maintainer**: Development Team
