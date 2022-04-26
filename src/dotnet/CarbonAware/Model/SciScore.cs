namespace CarbonAware.Model;

[Serializable]
public record SciScore
{
    public double? SciScoreValue { get; set; }
    public double? EnergyValue { get; set; }
    public double? MarginalCarbonEmissionsValue { get; set; }
    public double? EmbodiedEmissionsValue { get; set; }
    public Int64? FunctionalUnitValue { get; set; }
}
