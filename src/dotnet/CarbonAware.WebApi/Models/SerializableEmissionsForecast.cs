namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record SerializableEmissionsForecast
{
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;
    
    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }

    [JsonPropertyName("windowSize")]
    public int WindowSize { get; set; }

    [JsonPropertyName("optimalDataPoint")]
    public SerializableEmissionsData? OptimalDataPoint { get; set; }

    [JsonPropertyName("forecastData")]
    public IEnumerable<SerializableEmissionsData>? ForecastData { get; set; }

    public static SerializableEmissionsForecast FromEmissionsForecast(EmissionsForecast emissionsForecast)
    {
        return new SerializableEmissionsForecast
        {
            GeneratedAt = emissionsForecast.GeneratedAt,
            Location = emissionsForecast.Location.DisplayName,
            StartTime = emissionsForecast.StartTime,
            EndTime = emissionsForecast.EndTime,
            WindowSize = (int)emissionsForecast.WindowSize.TotalMinutes,
            OptimalDataPoint = SerializableEmissionsData.FromEmissionsData(emissionsForecast.OptimalDataPoint),
            ForecastData = emissionsForecast.ForecastData.Select(d => SerializableEmissionsData.FromEmissionsData(d))
        };
    }
}