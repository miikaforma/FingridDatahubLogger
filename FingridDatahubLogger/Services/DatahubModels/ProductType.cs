using System.Text.Json.Serialization;
using FingridDatahubLogger.Converters;

namespace FingridDatahubLogger.Services.DatahubModels;

[JsonConverter(typeof(ProductTypeConverter))]
public enum ProductType
{
    [JsonPropertyName("8716867000030")]
    ActiveEnergy, // Pätöenergia
        
    [JsonPropertyName("8716867000047")]
    ReactiveEnergyAll // Loisenergia (Kaikki)
}
