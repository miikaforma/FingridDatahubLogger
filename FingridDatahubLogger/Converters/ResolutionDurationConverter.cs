using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class ResolutionDurationConverter : JsonConverter<ResolutionDuration>
{
    public override ResolutionDuration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Enum.TryParse<ResolutionDuration>(value, out var result) ? result : ResolutionDuration.None; // Default value if parsing fails
    }

    public override void Write(Utf8JsonWriter writer, ResolutionDuration value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ResolutionDuration.None => "",
            ResolutionDuration.PT15M => "PT15M",
            ResolutionDuration.PT1H => "PT1H",
            ResolutionDuration.P1D => "P1D",
            ResolutionDuration.P1M => "P1M",
            ResolutionDuration.P1Y => "P1Y",
            _ => throw new JsonException($"Unknown ResolutionDuration value: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}