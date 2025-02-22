using Microsoft.Extensions.Options;

namespace FingridDatahubLogger.Settings;

public record AppSettings
{
    public const string SectionName = "App";
    
    public bool SkipTokenCheck { get; init; }
}

public class AppSettingsValidation : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings settings)
    {

        return ValidateOptionsResult.Success;
    }
}
