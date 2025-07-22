using Admin.Configuration;
using Admin.Extensions;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// تحميل متغيرات البيئة
Env.Load();

// تكوين الخدمات
ConfigureServices(builder.Services);

var app = builder.Build();

// تكوين Middleware
ConfigureMiddleware(app);

app.MapControllers();
app.Run();


static void ConfigureServices(IServiceCollection services)
{
    // تكوين قاعدة البيانات
    ConfigureDatabase(services);
    
    // تكوين المصادقة والتفويض
    ConfigureAuthentication(services);
    
    // تكوين CORS
    ConfigureCors(services);
    
    // إضافة الخدمات الأساسية
    services.AddApplicationServices();
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}


static void ConfigureDatabase(IServiceCollection services)
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Connection") 
        ?? throw new InvalidOperationException("Connection string is not configured");
    
    services.AddDatabaseContext(connectionString);
}


static void ConfigureAuthentication(IServiceCollection services)
{
    var jwtConfig = new JwtConfiguration
    {
        Issuer = Environment.GetEnvironmentVariable("JWT__Issuer") 
            ?? throw new InvalidOperationException("JWT Issuer is not configured"),
        Audience = Environment.GetEnvironmentVariable("JWT__Audience") 
            ?? throw new InvalidOperationException("JWT Audience is not configured"),
        SecretKey = Environment.GetEnvironmentVariable("JWT__SecretKey") 
            ?? throw new InvalidOperationException("JWT Secret Key is not configured")
    };
    
    services.AddJwtAuthentication(jwtConfig);
}

static void ConfigureCors(IServiceCollection services)
{
    var corsConfig = new CorsConfiguration
    {
        PolicyName = "MyCors",
        AllowedOrigins = new[] { "https://test.takhleesak.com" },
        AllowCredentials = true
    };
    
    services.AddCorsPolicy(corsConfig);
}

static void ConfigureMiddleware(IApplicationBuilder app)
{
    app.UseCustomMiddleware("MyCors");
}
