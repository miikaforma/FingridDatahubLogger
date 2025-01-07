using Microsoft.Extensions.Options;

namespace FingridDatahubLogger.Settings;

public record DatahubSettings
{
    public const string SectionName = "Datahub";

    public string BaseUrl { get; set; } = "https://oma.datahub.fi";
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0";
    public string Origin { get; set; } = "https://oma.datahub.fi";
    public string Referer { get; set; } = "https://oma.datahub.fi/";

    public int TokenExpiryCheckInterval { get; set; } = 60 * 5; // 5 minute by default
    public int TokenExpiryThreshold { get; set; } = 60 * 60 * 1; // 1 hour by default
}

public class DatahubSettingsValidation : IValidateOptions<DatahubSettings>
{
    public ValidateOptionsResult Validate(string? name, DatahubSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return ValidateOptionsResult.Fail("BaseUrl must be provided.");
        }

        return ValidateOptionsResult.Success;
    }
}
