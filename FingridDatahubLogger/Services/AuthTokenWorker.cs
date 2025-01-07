using FingridDatahubLogger.Settings;
using Microsoft.Extensions.Options;

namespace FingridDatahubLogger.Services;

public class AuthTokenWorker : BackgroundService
{
    private readonly ILogger<AuthTokenWorker> _logger;
    private readonly DatahubSettings _datahubSettings;
    private readonly IDatahubClient _datahubClient;

    public AuthTokenWorker(ILogger<AuthTokenWorker> logger, IOptions<DatahubSettings> datahubSettings, IDatahubClient datahubClient)
    {
        _logger = logger;
        _datahubClient = datahubClient;
        _datahubSettings = datahubSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auth Token Worker is running");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_datahubClient.IsTokenExpired())
            {
                _logger.LogWarning("Token has expired and a manual login is required");
            }
            else if (_datahubClient.IsTokenRefreshNeeded())
            {
                _logger.LogInformation("Refreshing auth token");
                await _datahubClient.RefreshTokenAsync();
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Token is still valid until {TokenExpiry}", _datahubClient.GetTokenExpiry());
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(_datahubSettings.TokenExpiryCheckInterval), stoppingToken);
        }

        _logger.LogInformation("Auth Token Worker is stopping");
    }
}