using Admin.Configuration;
using Admin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Admin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext.DB>(options =>
            {
                options.UseMySQL(connectionString);
            });
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtConfiguration jwtConfig)
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
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
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

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, CorsConfiguration corsConfig)
        {
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
            services.AddHttpClient();
            services.AddScoped<DTO.Functions>();
            services.AddScoped<ILogService, LogService>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
} 