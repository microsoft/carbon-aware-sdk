using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// An interface for interacting with the WattTime API.
    /// </summary>
    public interface IWattTimeClient
    {
        /// <summary>
        /// Async method to get observed emission data for a given balancing authority and time period.
        /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
        /// <param name="startTime">Start time of the time period</param>
        /// <param name="endTime">End time of the time period</param>
        /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
        /// </summary>
        public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string balancingAuthorityAbbreviation, string startTime, string endTime);

        /// <summary>
        /// Async method to get observed emission data for a given balancing authority and time period.
        /// <param name="balancingAuthority">Balancing authority</param>
        /// <param name="startTime">Start time of the time period</param>
        /// <param name="endTime">End time of the time period</param>
        /// <returns>An <see cref="Task{IEnumerable}{GridEmissionDataPoint}"/> which contains all emissions data points in a period.</returns>
        /// </summary>
        public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(BalancingAuthority balancingAuthority, string startTime, string endTime);

        /// <summary>
        /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
        /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
        /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
        /// </summary>
        public Task<Forecast?> GetCurrentForecastAsync(string balancingAuthorityAbbreviation);

        /// <summary>
        /// Async method to get the most recent 24 hour forecasted emission data for a given balancing authority.
        /// <param name="balancingAuthority">Balancing authority</param>
        /// <returns>An <see cref="Task{Forecast}"/> which contains forecasted emissions data points.</returns>
        /// </summary>
        public Task<Forecast?> GetCurrentForecastAsync(BalancingAuthority balancingAuthority);

        /// <summary>
        /// Async method to get all generated forecasts in the given period and balancing authority.
        /// <param name="balancingAuthorityAbbreviation">Balancing authority abbreviation</param>
        /// <param name="startTime">Start time of the time period</param>
        /// <param name="endTime">End time of the time period</param>
        /// <returns>An <see cref="Task{IEnumerable}{Forecast}"/> which contains all forecast sets generated in the given period.</returns>
        /// </summary>
        public Task<IEnumerable<Forecast>> GetForecastByDateAsync(string balancingAuthorityAbbreviation, string startTime, string endTime);

        /// <summary>
        /// Async method to get all generated forecasts in the given period and balancing authority.
        /// <param name="balancingAuthority">Balancing authority</param>
        /// <param name="startTime">Start time of the time period</param>
        /// <param name="endTime">End time of the time period</param>
        /// <returns>An <see cref="Task{IEnumerable}{Forecast}"/> which contains all forecast sets generated in the given period.</returns>
        /// </summary>
        public Task<IEnumerable<Forecast>> GetForecastByDateAsync(BalancingAuthority balancingAuthority, string startTime, string endTime);


        /// <summary>
        /// Async method to get the balancing authority for a given location.
        /// <param name="latitude">Latitude of the location</param>
        /// <param name="longitude">Longitude of the location</param>
        /// <returns>An <see cref="Task{BalancingAuthority}"/> which contains the balancing authority details.</returns>
        /// </summary>
        public Task<BalancingAuthority?> GetBalancingAuthorityAsync(string latitude, string longitude);
    }
}
