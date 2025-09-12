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

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("MyCors");

app.UseAuthentication();
app.UseAuthorization();

app.UseGlobalExceptionHandler(); // يفضل يكون بعد كل الـ Auth عشان يلتقط أي استثناء

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHealthChecks("/health");

app.Run();

