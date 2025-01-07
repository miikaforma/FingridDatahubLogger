using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

// ReSharper disable InconsistentNaming

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(ReadingTypeConverter))]
public enum ReadingType
{
    [JsonPropertyName("0")]
    BN01 = 0, // Mitattu

    [JsonPropertyName("100")]
    BN02 = 100, // Netotettu

    [JsonPropertyName("101")]
    BN03 = 101 // Energia yhteisö
}