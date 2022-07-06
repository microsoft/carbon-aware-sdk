namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastDTO : EmissionsForecastBaseDTO
{
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }

    [JsonPropertyName("optimalDataPoint")]
    public EmissionsDataDTO? OptimalDataPoint { get; set; }

    [JsonPropertyName("forecastData")]
    public IEnumerable<EmissionsDataDTO>? ForecastData { get; set; }

    public static EmissionsForecastDTO FromEmissionsForecast(EmissionsForecast emissionsForecast)
    {
        return new EmissionsForecastDTO
        {
            GeneratedAt = emissionsForecast.GeneratedAt,
            Location = emissionsForecast.Location.DisplayName,
            StartTime = emissionsForecast.StartTime,
            EndTime = emissionsForecast.EndTime,
            WindowSize = (int)emissionsForecast.WindowSize.TotalMinutes,
            OptimalDataPoint = EmissionsDataDTO.FromEmissionsData(emissionsForecast.OptimalDataPoint),
            ForecastData = emissionsForecast.ForecastData.Select(d => EmissionsDataDTO.FromEmissionsData(d))!
        };
    }
}