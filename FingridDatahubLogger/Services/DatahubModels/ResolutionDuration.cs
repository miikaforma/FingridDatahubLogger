using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResolutionDuration
{
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