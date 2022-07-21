namespace CarbonAware.WebApi.Models;

using System.Text.Json.Serialization;

[Serializable]
public record CarbonIntensityOutputDTO : CarbonIntensityBaseDTO
{
    /// <summary>value of the marginal carbon intensity in grams per kilowatt hour/</summary>
    /// <example>345.434</example>
    [JsonPropertyName("carbonIntensity")]
    public double CarbonIntensity { get; set; }

}