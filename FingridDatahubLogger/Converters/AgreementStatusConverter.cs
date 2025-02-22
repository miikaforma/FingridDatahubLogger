using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class AgreementStatusConverter : JsonConverter<AgreementStatus>
{
    public override AgreementStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        foreach (var field in typeof(AgreementStatus).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute attribute)
            {
                if (attribute.Name == value)
                {
                    return (AgreementStatus)field.GetValue(null);
                }
            }
        }
        throw new JsonException($"Unable to convert \"{value}\" to {nameof(AgreementStatus)}.");
    }

    public override void Write(Utf8JsonWriter writer, AgreementStatus value, JsonSerializerOptions options)
    {
        var field = typeof(AgreementStatus).GetField(value.ToString());
        if (Attribute.GetCustomAttribute(field, typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute attribute)
        {
            writer.WriteStringValue(attribute.Name);
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}