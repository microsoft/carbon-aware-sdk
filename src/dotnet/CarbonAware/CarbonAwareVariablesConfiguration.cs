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

    private string webApiRoutePrefix = null;

    /// <summary>
    /// Gets or sets the the carbon intensity data source to use.
    /// </summary>
    public string CarbonIntensityDataSource { get; set; }

    /// <summary>
    /// Gets or sets the route prefix to use for all web api routes.  Defaults to null.
    /// </summary>
    public string WebApiRoutePrefix
    {
        get
        {
            return this.webApiRoutePrefix;
        }

        set
        {
            if (!value.StartsWith('/'))
            {
                this.webApiRoutePrefix = $"/{value}";
            }
            else
            {
                this.webApiRoutePrefix = value;
            }
        }
    }
}
