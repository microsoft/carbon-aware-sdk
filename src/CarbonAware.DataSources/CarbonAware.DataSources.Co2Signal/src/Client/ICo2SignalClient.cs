using CarbonAware.DataSources.Co2Signal.Model;

namespace CarbonAware.DataSources.Co2Signal.Client;

/// <summary>
/// An interface for interacting with the CO2 Signal API.
/// </summary>
public interface ICo2SignalClient
{
    public const string NamedClient = "Co2SignalClient";

    /// <summary>
    /// Async method to get the latest carbon intensity for a zone given latitude and longitude
    /// </summary>
    /// <param name="latitude">Latitude for query</param>
    /// <param name="longitude">Longitude for query</param>
    /// <returns>A <see cref="Task{LatestCarbonIntensityData}"/> which contains the latest emissions data point for the given lat/long.</returns>
    /// <exception cref="Co2SignalClientException">Can be thrown when errors occur connecting to CO2 Signal client.  See the Co2SignalClientException class for documentation of expected status codes.</exception>
    public Task<LatestCarbonIntensityData> GetLatestCarbonIntensityAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get the latest carbon intensity for a country
    /// </summary>
    /// <param name="countryCode">Zone name for query</param>
    /// <returns>A <see cref="Task{LatestCarbonIntensityData}"/> which contains the latest emissions data point for the given country code.</returns>
    /// <exception cref="Co2SignalClientException">Can be thrown when errors occur connecting to CO2 Signal client.  See the Co2SignalClientException class for documentation of expected status codes.</exception>
    public Task<LatestCarbonIntensityData> GetLatestCarbonIntensityAsync(string countryCode);
}
