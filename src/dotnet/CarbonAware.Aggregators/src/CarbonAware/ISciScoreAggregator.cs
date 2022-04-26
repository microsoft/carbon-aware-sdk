using CarbonAware.Model;
using System.Collections;

namespace CarbonAware.Aggregators.SciScore

{
    public interface ISciScoreAggregator : IAggregator
    {
        /// <summary>
        /// Returns emissions data records.
        /// </summary>
        /// <param name="location">Location object representing the location for the carbon intensity data.</param>
        /// <param name="timeInterval">ISO8601 time interval representing the interval to calculate carbon intensity over.</param>
        /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
        // Location, 
        Task<IEnumerable<EmissionsData>> CalculateCarbonIntensityAsync(Location location, string timeInterval);
    }
}