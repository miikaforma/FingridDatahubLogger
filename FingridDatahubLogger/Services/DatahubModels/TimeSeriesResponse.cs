using System.Text.Json.Serialization;

namespace FingridDatahubLogger.Services.DatahubModels;

public record TimeSeriesResponse
{
    [JsonPropertyName("TimeSeries")]
    public List<TimeSerie> TimeSeries { get; set; }
}