using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;
using Npgsql;

namespace FingridDatahubLogger.Services.DatahubModels;

public record TimeSerie
{
    [JsonPropertyName("MeteringPointEAN")]
    public string MeteringPointEan { get; set; }

    [JsonPropertyName("ResolutionDuration")]
    public ResolutionDuration ResolutionDuration { get; set; }

    [JsonPropertyName("PeriodStartTS")]
    public DateTime PeriodStartTs { get; set; }

    [JsonPropertyName("PeriodEndTS")]
    public DateTime PeriodEndTs { get; set; }

    [JsonPropertyName("ProductType")]
    public ProductType ProductType { get; set; }

    [JsonPropertyName("UnitType")]
    public string UnitType { get; set; }

    [JsonPropertyName("ReadingType")]
    public ReadingType ReadingType { get; set; }

    [JsonPropertyName("Observations")]
    public List<Observation> Observations { get; set; }
        
    public void AddMetrics(NpgsqlParameterCollection parameterCollection)
    {
        parameterCollection.AddWithValue("metering_point_ean", MeteringPointEan);
        parameterCollection.AddWithValue("resolution_duration", NpgsqlTypes.NpgsqlDbType.Text, ResolutionDuration.ToString());
        parameterCollection.AddWithValue("product_type", NpgsqlTypes.NpgsqlDbType.Text, JsonSerializer.Serialize(ProductType, new JsonSerializerOptions { Converters = { new ProductTypeConverter() } }).Trim('"'));
        parameterCollection.AddWithValue("unit_type", UnitType);
        parameterCollection.AddWithValue("reading_type", NpgsqlTypes.NpgsqlDbType.Text, ReadingType.ToString());
        parameterCollection.AddWithValue("measurement_source", "FingridDatahub");
    }
}