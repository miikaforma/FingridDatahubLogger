using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class ReadingTypeConverter : JsonConverter<ReadingType>
{
    public override ReadingType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Enum.Parse<ReadingType>(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, ReadingType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((int)value).ToString());
    }
}