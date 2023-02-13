using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.Co2Signal.Configuration;

/// <summary>
/// A configuration class for holding CO2 Signal client config values.
/// </summary>
public class Co2SignalClientConfiguration
{
    /// <summary>
    /// API Token Header (i.e 'auth-token')
    /// </summary>
    public string APITokenHeader { get; set; } = "auth-token";

    /// <summary>
    /// Token value to be used with API Token Header
    /// </summary>
    public string APIToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base url to use when connecting to CO2 Signal
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.co2signal.com/v1/";


    /// <summary>
    /// Validate that this object is properly configured.
    /// </summary>
    public void Validate()
    {
        if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
        {
            throw new ConfigurationException($"{nameof(this.BaseUrl)} is not a valid absolute url.");
        }

        // Required to provide API Token
        if (string.IsNullOrWhiteSpace(this.APIToken)){
            throw new ConfigurationException($"Incomplete auth config: must set {nameof(this.APIToken)}.");
        }
    }
}
