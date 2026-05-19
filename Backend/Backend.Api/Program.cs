using System.Text.Json.Serialization;
using Backend.Api.Auth;
using Backend.Api.Middleware;
using Backend.Api.OpenApi.Transformers;
using Backend.Application;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddMarketplaceAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
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
    await app.Services.SeedAdminDataAsync(builder.Configuration);
    await app.Services.SeedSellerDataAsync(builder.Configuration);

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .AddPreferredSecuritySchemes("Bearer")
            .AddHttpAuthentication(
                "Bearer",
                auth => { }
            )
            .EnablePersistentAuthentication();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

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
