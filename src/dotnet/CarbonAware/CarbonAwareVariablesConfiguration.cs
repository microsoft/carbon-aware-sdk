using Microsoft.AspNetCore.Http;

namespace CarbonAware;

/// <summary>
/// Carbon Aware Variables bindings
/// </summary>
public class CarbonAwareVariablesConfiguration
{
    /// <summary>
    /// The Key containing the configuration values.
    /// </summary>
    public const string Key = "CarbonAwareVars";

    /// <summary>
    /// Gets or sets the route prefix to use for all web api routes.
    /// </summary>
    public PathString WebApiRoutePrefix { get; set; }

    /// <summary>
    /// Gets or sets the the carbon intensity data source to use.
    /// </summary>
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
