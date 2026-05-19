using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Application.Services;
using TestManagement.Api.Application.Validation;
using TestManagement.Api.Infrastructure.Data;
using TestManagement.Api.Infrastructure.Repositories;
using TestManagement.Api.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=TestManagement.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:1420",
                "http://127.0.0.1:1420",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ITestTakingService, TestTakingService>();
builder.Services.AddSingleton<ITestRequestValidator, TestRequestValidator>();
builder.Services.AddSingleton<IScoreCalculator, ScoreCalculator>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Frontend");
app.MapControllers();
app.MapFallbackToFile("index.html");

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program
{
}
