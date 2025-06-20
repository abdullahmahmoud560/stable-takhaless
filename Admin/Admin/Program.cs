using System.Text;
using Admin.ApplicationDbContext;
using Admin.DTO;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithMachineName(); // لإضافة معلومات الجهاز إلى السجلات
});

builder.Services.AddDbContext<DB>(options =>
{
    options.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__Connection")!);
});

SelfLog.Enable(Console.Out);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

builder.Services.AddHttpClient();
builder.Services.AddScoped<Functions>();
builder.Services.AddHttpContextAccessor();

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

// ✅ 1. تسجيل الطلبات في اللوج
app.UseSerilogRequestLogging();

// ✅ 2. تفعيل Swagger (عادةً يستخدم في التطوير أو لو حابب تعرض واجهة الـ API)
app.UseSwagger();
app.UseSwaggerUI();

// ✅ 3. إجبار التحويل لـ HTTPS (اختياري لكن مهم للأمان)
app.UseHttpsRedirection();

// ✅ 4. تفعيل CORS للسماح للطلبات من مصادر معينة
app.UseCors("MyCors");

// ✅ 5. إضافة Authentication (يقرأ التوكن من الكوكي أو الهيدر)
app.UseAuthentication();

// ✅ 6. إضافة Authorization (يتحقق من صلاحيات الدور)
app.UseAuthorization();

// ✅ 7. ربط الـ Controllers مع الـ Endpoints
app.MapControllers();

// ✅ 8. تشغيل التطبيق
app.Run();
