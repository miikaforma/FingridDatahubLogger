using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public record InvoiceAddress
(
    [property: JsonPropertyName("invoicingAddressType")]
    string InvoicingAddressType,
    
    [property: JsonPropertyName("streetName")]
    string StreetName,
    
    [property: JsonPropertyName("buildingNumber")]
    string BuildingNumber,
    
    [property: JsonPropertyName("postalCode")]
    string PostalCode,
    
    [property: JsonPropertyName("postOffice")]
    string PostOffice,
    
    [property: JsonPropertyName("countryCode")]
    string CountryCode
);
