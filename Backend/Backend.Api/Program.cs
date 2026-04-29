using System.Text.Json.Serialization;
using Backend.Api.Auth;
using Backend.Api.Middleware;
using Backend.Api.OpenApi.Transformers;
using Backend.Application;
using Backend.Application.Common.Abstractions;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

var tokenSecret = builder.Configuration["Authentication:TokenSecret"];
if (!builder.Environment.IsDevelopment() &&
    !builder.Environment.IsEnvironment("Testing") &&
    (string.IsNullOrWhiteSpace(tokenSecret) || tokenSecret.Length < 32))
{
    throw new InvalidOperationException("Authentication token secret must be configured and at least 32 characters long.");
}

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logDirectory = context.Configuration["LogFiles:DirectoryPath"];
    var resolvedLogDirectory = string.IsNullOrWhiteSpace(logDirectory)
        ? Path.Combine(AppContext.BaseDirectory, "logs")
        : Path.GetFullPath(logDirectory);

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: Path.Combine(resolvedLogDirectory, "backend-.log"),
            rollingInterval: RollingInterval.Day,
            shared: true,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddAuthentication(MarketplaceBearerAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, MarketplaceBearerAuthenticationHandler>(
        MarketplaceBearerAuthenticationHandler.SchemeName,
        options => { });
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<DefaultResponsesTransformer>();
});

var app = builder.Build();
app.Logger.LogInformation("Backend API host built successfully.");

var applyMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
var seedOlistOnStartup = builder.Configuration.GetValue<bool>("OlistImport:Enabled");

if (applyMigrationsOnStartup || seedOlistOnStartup)
{
    await app.Services.ApplyMigrationsAsync();
}

if (seedOlistOnStartup)
{
    await app.Services.SeedOlistDataAsync();
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    app.Logger.LogInformation("Backend API starting.");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
