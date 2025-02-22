using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

namespace FingridDatahubLogger.Services.DatahubModels;

public class ConsumptionRequest
{
    [JsonPropertyName("MeteringPointEAN")]
    public required string MeteringPointEan { get; set; }

    [JsonPropertyName("PeriodStartTS")]
    [JsonConverter(typeof(DateTimeConverter))]
    public required DateTime PeriodStartTs { get; set; }

    [JsonPropertyName("PeriodEndTS")]
    [JsonConverter(typeof(DateTimeConverter))]
    public required DateTime PeriodEndTs { get; set; }

    [JsonPropertyName("UnitType")]
    public string UnitType { get; set; } = "kWh";

    [JsonPropertyName("ProductType")]
    public ProductType ProductType { get; set; } = ProductType.ActiveEnergy;

    [JsonPropertyName("SettlementRelevant")]
    public bool SettlementRelevant { get; set; } = false;

    [JsonPropertyName("ResolutionDuration")]
    public ResolutionDuration? ResolutionDuration { get; set; } = null;

    [JsonPropertyName("ReadingType")]
    public ReadingType ReadingType { get; set; } = ReadingType.BN01;
}