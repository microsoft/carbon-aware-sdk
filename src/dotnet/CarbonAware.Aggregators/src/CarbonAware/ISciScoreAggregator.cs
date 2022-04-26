using CarbonAware.Model;
using System.Collections;

namespace CarbonAware.Aggregators.SciScore

{
    public interface ISciScoreAggregator : IAggregator
    {
        /// <summary>
        /// Returns a float that is made up of 
        /// </summary>
        /// <param name="location">Location object representing the location for the carbon intensity data.</param>
        /// <param name="timeInterval">ISO8601 time interval representing the interval to calculate carbon intensity over.</param>
        /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
        // Location, 
        public Task<float> CalculateAverageCarbonIntensityAsync(Location location, string timeInterval);
    }
}