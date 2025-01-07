using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Services;

public interface IDatahubClient
{
    bool IsTokenExpired();

    bool IsTokenRefreshNeeded();

    DateTime? GetTokenExpiry();
    
    Task RefreshTokenAsync();
    
    Task<OrganisationData[]> GetOrganisationsAsync();

    Task<TimeSeriesResponse> GetConsumptionsAsync(ConsumptionRequest request);
}