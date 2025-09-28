using Application.Interface;
using Application.Service;
using AspNetCoreRateLimit;
using firstProject.ApplicationDbContext;
using firstProject.Model;
using Infrastructure.Identites;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSqlContext(this IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Connection");
            
            services.AddDbContextPool<DB>(opts =>
                opts.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => 
                    {
                        options.MigrationsAssembly("Infrastructure");
                        options.CommandTimeout(30); 
                        options.EnableRetryOnFailure(3);
                        options.MaxBatchSize(100);
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                ), 
                poolSize: 100);
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true; 
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<User>)));
            })
            .AddEntityFrameworkStores<DB>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureUserRepository(this IServiceCollection services) =>
            services.AddScoped<IUserRepository, UserRepository>();

        public static void ConfigureUserService(this IServiceCollection services) =>
            services.AddScoped<IUserService, UserService>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureDotEnv(this IServiceCollection services)
        {
            DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        }

        public static void ConfigureJWT(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = Environment.GetEnvironmentVariable("JWT__Issuer"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT__Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey")!)
                    ),

                    RoleClaimType = "Role",
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void ConfigureCORS(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyCors", builder =>
                    builder.WithOrigins(
                        "https://test.takhleesak.com",
                        "http://localhost:3000",
                        "https://f67h0v6n-3000.euw.devtunnels.ms"
                    )
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                     .WithHeaders("Authorization", "Content-Type")  
                     .AllowCredentials()
                );
            });
        }

        public static void ConfigureDataProtection(this IServiceCollection services)
        {
            services.AddDataProtection();
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(1);
            });
        }

        public static void ConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                configuration.GetSection("IpRateLimiting").Bind(options);

                options.QuotaExceededResponse = new QuotaExceededResponse
                {
                    StatusCode = 429,
                    ContentType = "application/json; charset=utf-8",
                    Content = "{\"status\":429,\"message\":\"لقد تجاوزت الحد المسموح لعدد طلبات الـ API! يرجى الانتظار قبل المحاولة مرة أخرى.\",\"data\":null}"
                };
            });

            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }

    }
}
