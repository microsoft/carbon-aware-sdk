using CarbonAware.DataSources.ElectricityMaps.Model;

namespace CarbonAware.DataSources.ElectricityMaps.Client;

/// <summary>
/// An interface for interacting with the Electricity Maps API.
/// </summary>
public interface IElectricityMapsClient
{
    public const string NamedClient = "ElectricityMapsClient";

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given latitude and longitude.
    /// </summary>
    /// <param name="latitude">Latitude for query</param>
    /// <param name="longitude">Longitude for query</param>
    /// <returns>A <see cref="Task{ForecastedCarbonIntensityData}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    public Task<ForecastedCarbonIntensityData> GetCurrentForecastAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get the most recent 24 hour forecasted emission data for a given zone name.
    /// </summary>
    /// <param name="zoneName">Zone name for query</param>
    /// <returns>A <see cref="Task{ForecastedCarbonIntensityData}"/> which contains forecasted emissions data points.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    public Task<ForecastedCarbonIntensityData> GetCurrentForecastAsync(string zoneName);

    /// <summary>
    /// Async method to get the most recent 24 hour observed emission data for a given latitude and longitude.
    /// </summary>
    /// <param name="latitude">Latitude for query</param>
    /// <param name="longitude">Longitude for query</param>
    /// <returns>A <see cref="Task{HistoryCarbonIntensityData}"/> which contains all emissions data points in the 24 hour period.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    public Task<HistoryCarbonIntensityData> GetHistoryCarbonIntensityDataAsync(string latitude, string longitude);

    /// <summary>
    /// Async method to get the most recent 24 hour observed emission data for a given latitude and longitude.
    /// </summary>
    /// <param name="zoneName">Zone name for query</param>
    /// <returns>A <see cref="Task{HistoryCarbonIntensityData}"/> which contains all emissions data points in the 24 hour period.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    public Task<HistoryCarbonIntensityData> GetHistoryCarbonIntensityDataAsync(string zoneName);
}
