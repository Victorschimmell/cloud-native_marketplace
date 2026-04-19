using System.Text.Json.Serialization;
using Backend.Api.Middleware;
using Backend.Api.OpenApi.Transformers;
using Backend.Application;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

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
    options.AddOperationTransformer<DefaultResponsesTransformer>();
});

var app = builder.Build();
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

app.Run();