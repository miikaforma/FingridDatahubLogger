using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using FingridDatahubLogger.CustomExceptions;
using FingridDatahubLogger.Services.DatahubModels;
using FingridDatahubLogger.Settings;
using Flurl.Http;
using Microsoft.Extensions.Options;

namespace FingridDatahubLogger.Services;

public class DatahubClient : IDatahubClient
{

    private readonly ILogger<DatahubClient> _logger;
    private readonly DatahubSettings _datahubSettings;
    private readonly AppSettings _appSettings;
    private readonly CookieJar _cookieJar;

    private const string CookieFilePath = "cookies.txt";

    public DatahubClient(ILogger<DatahubClient> logger, IOptions<DatahubSettings> datahubSettings, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _datahubSettings = datahubSettings.Value;
        _appSettings = appSettings.Value;
        _cookieJar = LoadCookiesFromFile();
    }

    public bool IsTokenExpired()
    {
        try
        {
            var token = ExtractAuthToken(_cookieJar);
            var jwt = new JwtSecurityToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract token from cookie jar");
            return true;
        }
    }
    
    public bool IsTokenRefreshNeeded()
    {
        try
        {
            var token = ExtractAuthToken(_cookieJar);
            var jwt = new JwtSecurityToken(token);
            return jwt.ValidTo < DateTime.UtcNow.AddSeconds(_datahubSettings.TokenExpiryThreshold);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract token from cookie jar");
            return true;
        }
    }
    
    public DateTime? GetTokenExpiry()
    {
        try
        {
            var token = ExtractAuthToken(_cookieJar);
            var jwt = new JwtSecurityToken(token);
            return jwt.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract token from cookie jar");
            return null;
        }
    }
    
    public async Task RefreshTokenAsync()
    {
        try
        {
            var response = await ConfigureRequest("_api/register/hpa")
                .AllowAnyHttpStatus()
                .WithAutoRedirect(false)
                .GetAsync();

            if (response.StatusCode == 302)
            {
                SaveCookiesToFile();
                
                var token = ExtractAuthToken(_cookieJar);
                var jwt = new JwtSecurityToken(token);
                _logger.LogInformation("Token refreshed. New expiry: {TokenExpiry}", jwt.ValidTo);
            }
            else
            {
                _logger.LogError("Failed to refresh token. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (FlurlHttpException ex)
        {
            var err = await ex.GetResponseStringAsync();
            _logger.LogError(ex, "Error returned from {RequestUrl}: {ErrorResponse}", ex.Call.Request.Url, err);
            throw;
        }
    }
    
    public Task<AgreementData[]> GetAgreementDataAsync(AgreementRequest request)
    {
        return PostDataAsync<AgreementData[]>("_api/GetAgreementData", request);
    }
        
    public Task<OrganisationData[]> GetOrganisationsAsync()
    {
        return PostDataAsync<OrganisationData[]>("_api/GetOrganisationData", null);
    }
    
    public Task<TimeSeriesResponse> GetConsumptionsAsync(ConsumptionRequest request)
    {
        return PostDataAsync<TimeSeriesResponse>("_api/GetConsumptionData", request);
    }
    
    private IFlurlRequest ConfigureRequest(string endpoint)
    {
        if (!_appSettings.SkipTokenCheck && IsTokenExpired())
        {
            // If the token has expired, the application can't refresh the token and needs manual intervention
            throw new Exception("Token has expired. Please login again.");
        }
        
        var authToken = ExtractAuthToken(_cookieJar);
        var xsrfToken = ExtractXsrfToken(_cookieJar);
        
        return new FlurlRequest($"{_datahubSettings.BaseUrl}/{endpoint}")
            .WithCookies(_cookieJar)
            .WithHeader("Origin", _datahubSettings.Origin)
            .WithHeader("User-Agent", _datahubSettings.UserAgent)
            .WithHeader("Referer", _datahubSettings.Referer)
            .WithHeader("Authorization", $"Bearer {authToken}")
            .WithHeader("X-XSRF-TOKEN", xsrfToken);
    }

    private async Task<string> GetDataAsync(string endpoint)
    {
        try {
            return await ConfigureRequest(endpoint)
                .GetStringAsync();
        }
        catch (FlurlHttpException ex) {
            var err = await ex.GetResponseStringAsync();
            _logger.LogError(ex, "Error returned from {RequestUrl}: {ErrorResponse}", ex.Call.Request.Url, err);
            throw;
        }
    }
    
    private async Task<T> GetJsonDataAsync<T>(string endpoint)
    {
        try {
            return await ConfigureRequest(endpoint)
                .GetJsonAsync<T>();
        }
        catch (FlurlHttpException ex) {
            var err = await ex.GetResponseStringAsync();
            _logger.LogError(ex, "Error returned from {RequestUrl}: {ErrorResponse}", ex.Call.Request.Url, err);
            throw;
        }
    }

    private async Task<T> PostDataAsync<T>(string endpoint, object? data)
    {
        try {
            var response = await ConfigureRequest(endpoint)
                .PostJsonAsync(data ?? new { })
                .ReceiveString();

            if (response.Contains("\"ReasonCode\":\"RC-MDM-RMV-100\""))
            {
                throw new NoDataFoundException("No data found");
            }

#if DEBUG
            // await File.WriteAllTextAsync("response.json", response);
#endif

            return JsonSerializer.Deserialize<T>(response) ?? throw new InvalidOperationException();
        }
        catch (FlurlHttpException ex) {
            var err = await ex.GetResponseStringAsync();
            _logger.LogError(ex, "Error returned from {RequestUrl}: {ErrorResponse}", ex.Call.Request.Url, err);
            
            throw;
        }
    }
    
    private void SaveCookiesToFile()
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, CookieFilePath);
        var cookies = _cookieJar.ToList();
        
        // Remove expiry from cap-user cookie (the cookie is valid for only 15 minutes while the token is valid for 6 hours)
        var capUserCookie = cookies
            .Where(c => c.Name == "cap-user")
            .OrderByDescending(c => c.DateReceived)
            .FirstOrDefault();
        if (capUserCookie != null)
        {
            var newCapUserCookie = new FlurlCookie(capUserCookie.Name, capUserCookie.Value, capUserCookie.OriginUrl, capUserCookie.DateReceived);
            _cookieJar.Remove(x => x.Name == "cap-user");
            if (!_cookieJar.TryAddOrReplace(newCapUserCookie, out var reason))
            {
                _logger.LogError("Failed to add new cap-user cookie: {Reason}", reason);
            }
        }
        
        File.WriteAllText(absolutePath, _cookieJar.ToString());
    }

    private static CookieJar LoadCookiesFromFile()
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, CookieFilePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"{absolutePath} not found. Please login before using this application.");
        }
        
        var cookies = File.ReadAllText(absolutePath);
        return CookieJar.LoadFromString(cookies);
    }
    
    private static string ExtractAuthToken(CookieJar cookieJar)
    {
        var capUserCookie = cookieJar.FirstOrDefault(c => c.Name == "cap-user");
        if (capUserCookie == null)
        {
            throw new Exception("cap-user cookie not found.");
        }

        var capUserJson = JsonDocument.Parse(Uri.UnescapeDataString(capUserCookie.Value));
        if (!capUserJson.RootElement.TryGetProperty("token", out var tokenElement))
        {
            throw new Exception("Token property not found in cap-user cookie.");
        }

        return tokenElement.GetString() ?? throw new InvalidOperationException("Token property is null.");
    }
    
    private static string ExtractXsrfToken(CookieJar cookieJar)
    {
        var xsrfToken = cookieJar.FirstOrDefault(c => c.Name == "XSRF-TOKEN");
        if (xsrfToken == null)
        {
            throw new Exception("XSRF-TOKEN cookie not found.");
        }

        return xsrfToken.Value;
        // return Uri.UnescapeDataString(xsrfToken.Value);
    }
}