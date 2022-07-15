namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record MarginalCarbonIntensityDTO
{

    /// <summary>
    /// </summary>
    /// <example>
    /// </example>
    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// </summary>
    /// <example>
    /// </example>
    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }
    public IEnumerable<EmissionsDataDTO>? ForecastData { get; set; }
}