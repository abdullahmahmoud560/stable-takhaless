using DotNetEnv;
using firstProject.Exceptions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using User.ApplicationDbContext;
using User.DTO;
using User.Service;


var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new PayPalService(
    Environment.GetEnvironmentVariable("PayPal__ClientId")!,
    Environment.GetEnvironmentVariable("PayPal__Secret")!,
    bool.Parse(Environment.GetEnvironmentVariable("PayPal__IsSandbox")!)
));

// إضافة MySQL
var dbConnectionString = builder.Configuration.GetConnectionString("Connection") 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Connection")
    ?? "Server=user-db;Database=u676203545_Orders;User=user_user;Password=user_pass;";

Console.WriteLine($"Database Connection String: {dbConnectionString}");

builder.Services.AddDbContext<DB>(options =>
{
    options.UseMySQL(dbConnectionString);
});

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
            // ✅ فقط من الكوكيز
            var tokenFromCookie = context.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(tokenFromCookie))
            {
                context.Token = tokenFromCookie;
            }

            return Task.CompletedTask;
        }
    };

});

builder.Services.AddHttpClient();
builder.Services.AddScoped<Functions>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();

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

builder.Services.AddScoped<HangFire>();
builder.Services.AddScoped<EmailService>();

// Get connection string with fallback
var hangfireConnectionString = builder.Configuration.GetConnectionString("HangeFire") 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__HangeFire")
    ?? "Server=hangfire-db;Database=u676203545_HangFire;User=hangfire_user;Password=hangfire_pass;";

Console.WriteLine($"Hangfire Connection String: {hangfireConnectionString}");

builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(
       hangfireConnectionString,
        new MySqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromMinutes(15),
            TransactionIsolationLevel = (System.Transactions.IsolationLevel)System.Data.IsolationLevel.ReadCommitted,
        }
    )));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
});

builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseStaticFiles();

// ✅ تفعيل Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("MyCors");
app.UseRouting();
app.MapHealthChecks("/health");
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/Dashboard", new DashboardOptions
{
    Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
});

app.MapHub<ChatHub>("/ChatHub");
app.MapControllers();
app.Run();

public class AllowAllUsersAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
