using AspNetCoreRateLimit;
using DotNetEnv;
using firstProject.ApplicationDbContext;
using firstProject.DTO;
using firstProject.Exceptions;
using firstProject.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddControllers();

// ✅ إضافة Rate Limiting
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();  
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();  
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();  


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<Token_verfy>(); // إضافة Token_verfy لحل مشكلة DI

// ✅ إضافة MySQL
builder.Services.AddDbContext<DB>(options =>
{
    options.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__Connection")!);
});

// ✅ إضافة Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
    options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<User>)));
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<DB>()
.AddDefaultTokenProviders();

// ✅ إضافة JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = Environment.GetEnvironmentVariable("JWT__Issuer"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT__Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey")!)),
        RoleClaimType = "Role",
        ClockSkew = TimeSpan.Zero 
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1️⃣ جرب تاخد من Authorization header
            //var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            //if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            //{
            //    context.Token = authHeader.Substring("Bearer ".Length).Trim();
            //    return Task.CompletedTask;
            //}

            //Cookies
            var token = context.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});


// ✅ إضافة Health Checks
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
// ✅ تخصيص الأخطاء في الـ API

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(m => m.Value!.Errors.Any())
            .Select(m => new
            {
                Field = m.Key,
                Errors = m.Value!.Errors.Select(e => e.ErrorMessage).ToList()
            })
            .ToList();

        return new OkObjectResult(new ApiResponse
        {
            Message = "هناك بعض الأخطاء في البيانات المدخلة ",
        });
    };
});
builder.Services.AddDataProtection();

// ✅ تخصيص وقت صلاحية التوكنات
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(1);
});

// ✅ إعدادات CORS
var MyCors = "MyCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyCors,
        policy =>
        {
            policy.WithOrigins("https://test.takhleesak.com")
                  .AllowCredentials() 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseGlobalExceptionHandler();

app.UseIpRateLimiting();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("MyCors");

app.UseAuthentication();
app.UseAuthorization();

var protectedRoutes = new[]
{
    "/api/Reset-Password",
    "/api/VerifyCode",
    "/api/Blocked-User",
    "/api/Unblocked-User",
    "/api/Black-List",
    "/api/Change-Roles",
    "/api/Remove-All-Access",
    "/api/Get-Information",
    "/api/Get-User",
    "/api/Get-Broker",
    "/api/Get-CustomerService",
    "/api/Get-Account",
    "/api/Get-All-Peaple-Admin",
    "/api/Statictis",
    "/api/Get-Information",
    "/api/Select-Data",
};

app.UseWhen(
    context => protectedRoutes.Any(route => context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseMiddleware<AuthenticationMiddleware>()
);

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
