using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public record MeteringPointAddress
(
    [property: JsonPropertyName("streetName")]
    string StreetName,
    
    [property: JsonPropertyName("buildingNumber")]
    string BuildingNumber,
    
    [property: JsonPropertyName("postalCode")]
    string PostalCode,
    
    [property: JsonPropertyName("postOffice")]
    string PostOffice,
    
    [property: JsonPropertyName("addressType")]
    string AddressType,
    
    [property: JsonPropertyName("language")]
    string Language,
    
    [property: JsonPropertyName("countryCode")]
    string CountryCode
);
