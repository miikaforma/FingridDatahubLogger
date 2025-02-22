using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Services;

public interface IDatahubClient
{
    bool IsTokenExpired();

    bool IsTokenRefreshNeeded();

    DateTime? GetTokenExpiry();
    
    Task RefreshTokenAsync();

    Task<AgreementData[]> GetAgreementDataAsync(AgreementRequest request);
    
    Task<OrganisationData[]> GetOrganisationsAsync();

    Task<TimeSeriesResponse> GetConsumptionsAsync(ConsumptionRequest request);
}