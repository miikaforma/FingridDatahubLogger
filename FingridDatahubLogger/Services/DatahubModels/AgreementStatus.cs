// ReSharper disable InconsistentNaming

using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(AgreementStatusConverter))]
public enum AgreementStatus
{
    [JsonPropertyName("AE01")]
    Connected,          // Connected (Kytketty)

    [JsonPropertyName("AE02")]
    Disconnected,       // Disconnected (Katkaistu)

    [JsonPropertyName("AE03")]
    UnderConstruction,  // Under construction (Rakenteilla)

    [JsonPropertyName("AE04")]
    RemovedFromUser,    // Removed from user (Poistettu käytöstä)

    [JsonPropertyName("AE05")]
    Deleted,            // Deleted (Poistettu)

    [JsonPropertyName("PCF")]
    PreConfirmed,       // Pre-confirmed (Odottaa vahvistusta)

    [JsonPropertyName("UCF")]
    Unconfirmed,        // Unconfirmed (Vahvistamaton)

    [JsonPropertyName("CFD")]
    Confirmed,          // Confirmed (Vahvistettu)

    [JsonPropertyName("PTR")]
    PreTerminated,      // Pre-terminated (Päättäminen odottaa vahvistusta)

    [JsonPropertyName("TRM")]
    Terminated,         // Terminated (Päätetty)

    [JsonPropertyName("CLD")]
    Canceled,           // Canceled (Peruttu)

    [JsonPropertyName("RUN")]
    Running,            // Running (Voimassa)

    [JsonPropertyName("END")]
    Ended               // Ended (Lopetettu)
}