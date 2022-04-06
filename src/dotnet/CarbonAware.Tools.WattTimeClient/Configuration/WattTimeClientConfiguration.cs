
namespace CarbonAware.Tools.WattTimeClient.Configuration;

/// <summary>
/// A configuration class for holding WattTime client config values.
/// </summary>
public class WattTimeClientConfiguration
{
    public const string Key = "WattTimeClient";

    /// <summary>
    /// Gets or sets the username to use when connecting to WattTime.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password to use when connecting to WattTime
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Validate that this object is properly configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(this.Username))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.Username)} is required for WattTime.");
        }

        if (string.IsNullOrEmpty(this.Password))
        {
            throw new ConfigurationException($"{Key}:{nameof(this.Password)} is required for WattTime.");
        }        
    }
}
