using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;
using Npgsql;

namespace FingridDatahubLogger.Services.DatahubModels;

public record Observation
{
    [JsonPropertyName("Epoch")]
    [JsonConverter(typeof(LongConverter))]
    public long Epoch { get; set; }

    [JsonPropertyName("PeriodStartTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime PeriodStartTime { get; set; }

    // [JsonPropertyName("Quantity")]
    // public string Quantity { get; set; }
    
    [JsonPropertyName("Quantity")]
    [JsonConverter(typeof(DoubleConverter))]
    public double Quantity { get; set; }

    [JsonPropertyName("Quality")]
    [JsonConverter(typeof(QualityConverter))]
    public Quality Quality { get; set; }
    
    public void AddMetrics(NpgsqlParameterCollection parameterCollection)
    {
        parameterCollection.AddWithValue("period_start", DateTimeOffset.FromUnixTimeMilliseconds(Epoch));
        parameterCollection.AddWithValue("quantity", Quantity);
        parameterCollection.AddWithValue("quality", NpgsqlTypes.NpgsqlDbType.Text, Quality.ToString());
    }
    
    public async Task AddMetrics(NpgsqlBinaryImporter writer, CancellationToken cancellationToken = default)
    {
        await writer.WriteAsync(DateTimeOffset.FromUnixTimeMilliseconds(Epoch), NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
        await writer.WriteAsync(Quantity, NpgsqlTypes.NpgsqlDbType.Double, cancellationToken);
        await writer.WriteAsync(Quality.ToString(), NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
    }
}