using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;


[Serializable]
public record SciScore
{
    /// <example>750.6</example>
    [JsonPropertyName("sciScore")]
    public double? SciScoreValue { get; set; }

    /// <example>1</example>
    [JsonPropertyName("energyValue")]
    public double? EnergyValue { get; set; }

    /// <example>750</example>
    [JsonPropertyName("marginalCarbonIntensityValue")]
    public double? MarginalCarbonIntensityValue { get; set; }

    /// <example>0</example>
    [JsonPropertyName("embodiedEmissionsValue")]
    public double? EmbodiedEmissionsValue { get; set; }

    /// <example>1</example>
    [JsonPropertyName("functionalUnitValue")]
    public Int64? FunctionalUnitValue { get; set; }

    // <example> 2007-03-01T13:00:00Z/2007-03-01T15:30:00Z </example>
    [JsonPropertyName("timeInterval")]
    public string? TimeInterval { get; set; }
}


