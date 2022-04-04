
namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// A configuration class for holding WattTime client config values.
    /// </summary>
    public class WattTimeClientConfiguration
    {
        public const string Key = "WattTimeClient";

        public string? Username { get; set; }
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
}