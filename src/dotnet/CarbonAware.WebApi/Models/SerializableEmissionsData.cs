using CarbonAware.Model;
using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SerializableEmissionsData
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    public static SerializableEmissionsData FromEmissionsData(EmissionsData emissionsData)
    {
        return new SerializableEmissionsData
        {
            Location = emissionsData.Location,
            Timestamp = emissionsData.Time,
            Duration = (int)emissionsData.Duration.TotalMinutes,
            Value = emissionsData.Rating
        };
    }
}