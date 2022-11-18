using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMaps.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmissionsFactor
{
    Lifecycle,
    Direct
}
