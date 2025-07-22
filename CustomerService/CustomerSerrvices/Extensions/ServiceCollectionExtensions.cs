using CustomerSerrvices.ApplicationDbContext;
using CustomerSerrvices.Configuration;
using CustomerSerrvices.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CustomerSerrvices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DB>(options =>
                options.UseMySQL(configuration.GetConnectionString("Connection")!));
            
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("JWT").Get<JwtConfig>();
            
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
                    ValidIssuer = jwtConfig?.Issuer,
                    ValidAudience = jwtConfig?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig?.SecretKey ?? string.Empty)),
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

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var corsConfig = configuration.GetSection("Cors").Get<CorsConfig>() ?? new CorsConfig();
            
            services.AddCors(options =>
            {
                options.AddPolicy(name: corsConfig.PolicyName, policy =>
                {
                    policy.WithOrigins(corsConfig.AllowedOrigins)
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