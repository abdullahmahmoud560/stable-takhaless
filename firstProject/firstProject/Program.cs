using firstProject.MappingProfiles;
using AspNetCoreRateLimit;
using firstProject.Exceptions;
using Infrastructure.Extensions;
using firstProject.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDotEnv();
builder.Services.ConfigureSqlContext();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureUserRepository();
builder.Services.ConfigureUserService();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureJWT();
builder.Services.ConfigureCORS();
builder.Services.ConfigureDataProtection();
builder.Services.AddMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
    options.TrackStatistics = true;
});
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Takhleesak-API/1.0");
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureRateLimiting(builder.Configuration);
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseIpRateLimiting();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

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
    "/api/Select-Data",
};

var sensitiveRoutes = new[]
{
    "/api/Login",
    "/api/Login-Mobile",
    "/api/Forget-Password",
    "/api/Register-user",
    "/api/Register-company",
    "/api/Register-Broker"
};

app.UseWhen(
    context => protectedRoutes.Any(route => context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseMiddleware<AuthenticationMiddleware>()
);

app.UseWhen(
    context => sensitiveRoutes.Any(route => context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseIpRateLimiting()
);

app.MapHealthChecks("/health");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
