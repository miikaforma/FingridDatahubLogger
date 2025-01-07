namespace FingridDatahubLogger.Services.DatahubModels;

public record OrganisationData
(
    string OrganisationID,
    string OrganisationIdentifier,
    string TertiaryOrganisationIdentifier,
    string Name,
    string MarketRole,
    string Status,
    List<OrganisationAddress> OrganisationAddresses,
    List<OrganisationContact> OrganisationContacts
);