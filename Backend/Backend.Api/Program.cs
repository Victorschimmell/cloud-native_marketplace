using Backend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
