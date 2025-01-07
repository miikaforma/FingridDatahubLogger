namespace FingridDatahubLogger.Services.DatahubModels;

public record OrganisationContact
(
    string FirstName,
    string LastName,
    string FullName,
    string EmailAddress,
    string PhoneNumber1,
    string WebAddressURL
);