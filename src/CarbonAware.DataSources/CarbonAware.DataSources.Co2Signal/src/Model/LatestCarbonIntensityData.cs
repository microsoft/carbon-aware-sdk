using CarbonAware.Model;
using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.Co2Signal.Model;

/// <summary>
/// History Carbon Intensity collection data.
/// </summary>
[Serializable]
public record LatestCarbonIntensityData
{
    /// <summary>
    /// Country Code.
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>
    /// Latest Carbon Intensity Data.
    /// </summary>
    [JsonPropertyName("data")]
    public CarbonIntensity Data { get; init; } = new CarbonIntensity();

    /// <summary>
    /// Status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Units
    /// </summary>
    [JsonPropertyName("units")]
    public DataUnits Units { get; init; } = new DataUnits();
}

/// <summary>
/// A history carbon intensity.
/// </summary>
[Serializable]
public record CarbonIntensity
{
    /// <summary>
    /// Carbon Intensity value.
    /// </summary>
    [JsonPropertyName("carbonIntensity")]
    public int Value { get; init; }

    /// <summary>
    /// Indicates the datetime of the carbon intensity
    /// </summary>
    [JsonPropertyName("datetime")]
    public DateTimeOffset DateTime { get; init; }

    /// <summary>
    /// Percentage of fossil fuels used at the given datetime
    /// </summary>
    [JsonPropertyName("fossilFuelPercentage")]
    public Double FossilFuelPercentage { get; init; }

    public static explicit operator EmissionsData(CarbonIntensity carbonIntensity)
    {
        return new EmissionsData
        {
            Rating = carbonIntensity.Value,
            Time = carbonIntensity.DateTime,
        };
    }
}

/// <summary>
/// Units for LatestCarbonIntensityData
/// </summary>
[Serializable]
public record DataUnits
{
    /// <summary>
    /// Carbon Intensity units.
    /// </summary>
    [JsonPropertyName("carbonIntensity")]
    public string CarbonIntensity { get; init; } = string.Empty;
}

