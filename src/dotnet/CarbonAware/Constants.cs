namespace CarbonAware;

/// <summary>
/// Carbon Aware Constants
/// </summary>
public class Constants
{
    public const string Locations = "locations";
    public const string Start = "start";
    public const string End = "end";
    public const string Duration = "duration";
    public const string Best = "best";
}

/// <summary>
/// Carbon Aware Variables bindings
/// </summary>
public class CarbonAwareVariablesConfiguration
{
    public const string Key = "CarbonAwareVars";
    public string CarbonIntensityDataSource { get; set; }
}
