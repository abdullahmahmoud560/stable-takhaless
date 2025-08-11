using CustomerSerrvices.Extensions;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services using extensions
builder.Services
    .AddDatabaseServices()
    .AddJwtAuthentication()
    .AddCorsPolicy()
    .AddApplicationServices();

builder.Services.AddSignalR();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

