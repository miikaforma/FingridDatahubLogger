using System.Text.Json;
using System.Text.Json.Serialization;
using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Converters;

public class ProductTypeConverter : JsonConverter<ProductType>
{
    public override ProductType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "8716867000030" => ProductType.ActiveEnergy,
            "8716867000047" => ProductType.ReactiveEnergyAll,
            _ => throw new JsonException($"Unknown ProductType value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ProductType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ProductType.ActiveEnergy => "8716867000030",
            ProductType.ReactiveEnergyAll => "8716867000047",
            _ => throw new JsonException($"Unknown ProductType value: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}