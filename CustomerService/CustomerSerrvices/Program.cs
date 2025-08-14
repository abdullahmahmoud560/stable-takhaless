using CustomerSerrvices.Extensions;
using DotNetEnv;
using firstProject.Exceptions;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseServices().AddJwtAuthentication().AddCorsPolicy().AddApplicationServices();
builder.Services.AddSignalR();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

