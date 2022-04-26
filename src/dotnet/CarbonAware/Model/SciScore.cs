namespace CarbonAware.Model;

[Serializable]
public record SciScore
{
    public float? SciScoreValue { get; set; }
    public float? EnergyValue { get; set; }
    public float? MarginalCarbonEmissionsValue { get; set; }
    public float? EmbodiedEmissionsValue { get; set; }
    public Int64? FunctionalUnitValue { get; set; }
}
