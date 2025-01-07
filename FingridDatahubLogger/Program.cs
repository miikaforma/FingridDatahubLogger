using FingridDatahubLogger.Services;
using FingridDatahubLogger.Settings;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add environment variables as configuration option
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
        true, true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddSingleton<IValidateOptions<TimescaleDbSettings>, TimescaleDbSettingsValidation>();
builder.Services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();
builder.Services.AddSingleton<IValidateOptions<DatahubSettings>, DatahubSettingsValidation>();

builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection(AppSettings.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<TimescaleDbSettings>()
    .Bind(builder.Configuration.GetSection(TimescaleDbSettings.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<DatahubSettings>()
    .Bind(builder.Configuration.GetSection(DatahubSettings.SectionName))
    .ValidateOnStart();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDatahubClient, DatahubClient>();
builder.Services.AddTransient<ITimescaleClient, TimescaleClient>();

builder.Services.AddHostedService<AuthTokenWorker>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Initialize DatahubClient at startup so it crashes the app if the settings are invalid
app.Services.GetRequiredService<IDatahubClient>();

#if DEBUG
// var datahubClient = app.Services.GetRequiredService<IDatahubClient>();
// await datahubClient.RefreshTokenAsync();
// var organisations = await datahubClient.GetOrganisationsAsync();
#endif

app.Run();