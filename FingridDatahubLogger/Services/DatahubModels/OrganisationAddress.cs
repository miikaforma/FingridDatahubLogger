namespace FingridDatahubLogger.Services.DatahubModels;

public record OrganisationAddress
(
    string StreetName,
    string BuildingNumber,
    string FloorID,
    string RoomID,
    string Postcode,
    string PostOfficeBox,
    string CityName
);