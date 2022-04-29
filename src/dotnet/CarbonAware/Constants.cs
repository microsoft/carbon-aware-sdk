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
/// Carbon Aware Environment variables bindings
/// </summary>
public class EnvironmentVariablesConfiguration
{
    public const string Key = "EnvironmentVariables";
    public string CarbonIntensityDataSource { get; set; }
}
