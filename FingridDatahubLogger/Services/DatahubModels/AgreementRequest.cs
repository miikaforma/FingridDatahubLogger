using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public class AgreementRequest
{
    [JsonPropertyName("AgreementStatus")]
    public required AgreementStatus AgreementStatus { get; set; } = AgreementStatus.Confirmed;
}