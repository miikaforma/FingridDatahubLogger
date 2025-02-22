// ReSharper disable InconsistentNaming

using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(AccountingPointTypeConverter))]
public enum AccountingPointType
{
    [JsonPropertyName("AG01")]
    Consumption,          // Consumption (Kulutus)
    [JsonPropertyName("AG02")]
    Production,          // Production (Tuotanto)
}