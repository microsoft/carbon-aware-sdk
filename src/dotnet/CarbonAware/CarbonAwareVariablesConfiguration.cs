namespace CarbonAware;

/// <summary>
/// Carbon Aware Variables bindings
/// </summary>
public class CarbonAwareVariablesConfiguration
{
    public const string Key = "CarbonAwareVars";
    public string CarbonIntensityDataSource { get; set; }
    
    /// <summary>
    /// Sets the UseWebProxy
    /// </summary>
    public bool UseWebProxy { get; set; }

    /// <summary>
    /// Sets the WebProxy url
    /// </summary>
    public string WebProxyUrl { get; set; }

    /// <summary>
    /// Sets the WebProxy username
    /// </summary>
    public string WebProxyUsername { get; set; }

    /// <summary>
    /// Sets the WebProxy password
    /// </summary>
    public string WebProxyPassword { get; set; }

}
