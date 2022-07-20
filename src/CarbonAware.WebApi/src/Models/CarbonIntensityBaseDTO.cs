namespace CarbonAware.WebApi.Models;

using System.Text.Json.Serialization;

[Serializable]
public record CarbonIntensityBaseDTO
{
    /// <summary>the azure location name </summary>
    /// <example> eastus </example>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>the time at which the job actually started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [JsonPropertyName("jobStartTime")]
    public DateTimeOffset JobStartTime { get; set; }

    /// <summary> the time at which the job actually ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [JsonPropertyName("jobEndTime")]
    public DateTimeOffset JobEndTime { get; set; }

}