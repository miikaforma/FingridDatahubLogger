using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public record AgreementData
(
    [property: JsonPropertyName("agreementStatus")]
    AgreementStatus AgreementStatus,
    [property: JsonPropertyName("agreementStartDate")]
    DateTimeOffset AgreementStartDate,
    [property: JsonPropertyName("agreementEndDate")]
    DateTimeOffset? AgreementEndDate,
    [property: JsonPropertyName("accountingPoint")]
    AccountingPoint AccountingPoint,
    [property: JsonPropertyName("invoiceAddresses")]
    InvoiceAddress[] InvoiceAddresses
);
