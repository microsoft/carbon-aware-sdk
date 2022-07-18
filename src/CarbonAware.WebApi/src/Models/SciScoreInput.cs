using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreInput
{
    /// <example> 
    /// {
    ///     "locationType": "CloudProvider",
    ///     "latitude": 0,
    ///     "longitude": 0,
    ///     "cloudProvider": "Azure",
    ///     "regionName": "eastus"
    /// }
    /// </example>
    [JsonPropertyName("location")]
    [Required()]
    public LocationInput Location { get; set; } = new LocationInput();

    /// <example> "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z" </example>
    [JsonPropertyName("timeInterval")]
    [Required()]
    public string TimeInterval { get; set; } = string.Empty;
}

[Serializable]
public record LocationInput
{
    /// <example>Cloud Provider</example>
    [JsonPropertyName("locationType")]
    [Required()]
    public string? LocationType { get; set; }

    /// <example>153.23</example>
    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    /// <example>72.23</example>
    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    /// <example>Azure</example>
    [JsonPropertyName("cloudProvider")]
    public string? CloudProvider { get; set; }

    /// <example>useast</example>
    [JsonPropertyName("regionName")]
    public string? RegionName { get; set; }
}