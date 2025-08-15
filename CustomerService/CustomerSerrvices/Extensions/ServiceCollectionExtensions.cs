using CustomerSerrvices.ApplicationDbContext;
using CustomerSerrvices.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CustomerSerrvices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
        {
            services.AddDbContext<DB>(options =>
                options.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__Connection")!));
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {            
            services.AddAuthentication(options =>
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
                        var token = context.Request.Cookies["token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyCors", policy =>
                {
                    policy.WithOrigins("https://test.takhleesak.com", "https://sender-chat.vercel.app/")
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddHealthChecks();
            return services;
        }
    }
} 