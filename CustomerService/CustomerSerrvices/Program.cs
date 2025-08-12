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

<<<<<<< HEAD

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
=======
// Configure the HTTP request pipeline
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI();

// Temporarily disable HTTPS redirection for testing
// app.UseHttpsRedirection();
>>>>>>> origin/prod
app.UseRouting();
app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

