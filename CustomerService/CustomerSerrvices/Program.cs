using System.Text;
using CustomerSerrvices.ApplicationDbContext;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DB>(options =>
options.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__Connection")!));



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
            var token = context.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHealthChecks();

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


var app = builder.Build();

app.MapHealthChecks("/health");

// 1. التحويل لـ HTTPS (اختياري لكنه جيد للأمان)
app.UseHttpsRedirection();

// 2. تفعيل Swagger (يُفضل في بيئة التطوير)
app.UseSwagger();
app.UseSwaggerUI();

// 3. إعداد Routing
app.UseRouting();

// 4. تفعيل CORS
app.UseCors("MyCors");

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. ربط الـ Controllers
app.MapControllers();

// 7. تشغيل التطبيق
app.Run();

