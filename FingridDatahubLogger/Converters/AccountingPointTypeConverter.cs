using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class AccountingPointTypeConverter : JsonConverter<AccountingPointType>
{
    public override AccountingPointType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        foreach (var field in typeof(AccountingPointType).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute attribute)
            {
                if (attribute.Name == value)
                {
                    return (AccountingPointType)field.GetValue(null);
                }
            }
        }
        throw new JsonException($"Unable to convert \"{value}\" to {nameof(AccountingPointType)}.");
    }

    public override void Write(Utf8JsonWriter writer, AccountingPointType value, JsonSerializerOptions options)
    {
        var field = typeof(AccountingPointType).GetField(value.ToString());
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