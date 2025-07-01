using DotNetEnv;
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
builder.Services.AddDbContext<DB>(options =>
{
    options.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__Connection")!);
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

builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(
       Environment.GetEnvironmentVariable("ConnectionStrings__HangeFire")!,
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
