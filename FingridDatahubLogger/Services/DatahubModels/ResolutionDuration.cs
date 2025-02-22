using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

// ReSharper disable InconsistentNaming

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(ResolutionDurationConverter))]
public enum ResolutionDuration
{
    [JsonPropertyName("")]
    None,
    
    [JsonPropertyName("PT15M")]
    PT15M,

    [JsonPropertyName("PT1H")]
    PT1H,

    [JsonPropertyName("P1D")]
    P1D,

    [JsonPropertyName("P1M")]
    P1M,

    [JsonPropertyName("P1Y")]
    P1Y
}