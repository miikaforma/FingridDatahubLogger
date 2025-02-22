using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public record AccountingPoint
(
    [property: JsonPropertyName("meteringPointEAN")]
    string MeteringPointEAN,
    [property: JsonPropertyName("accountingPointType")]
    AccountingPointType AccountingPointType,
    [property: JsonPropertyName("meteringPointAddresses")]
    MeteringPointAddress[] MeteringPointAddresses
);
