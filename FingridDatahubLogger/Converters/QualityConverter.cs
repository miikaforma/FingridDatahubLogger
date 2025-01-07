using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class QualityConverter : JsonConverter<Quality>
{
    public override Quality Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Enum.Parse<Quality>(reader.GetString() ?? "NONE", true);
    }

    public override void Write(Utf8JsonWriter writer, Quality value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}