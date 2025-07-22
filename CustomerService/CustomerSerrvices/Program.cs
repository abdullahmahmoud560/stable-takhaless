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
    .AddDatabaseServices(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddCorsPolicy(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

