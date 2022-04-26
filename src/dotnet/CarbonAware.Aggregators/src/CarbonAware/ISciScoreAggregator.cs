using CarbonAware.Model;
using System.Collections;

namespace CarbonAware.Aggregators.SciScore

{
    public interface ISciScoreAggregator : IAggregator
    {
        /// <summary>
        /// Returns emissions data records.
        /// </summary>
        /// <param name="props">IDictionary with properties required by concrete classes</param>
        /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
        // Location, 
        Task<IEnumerable<EmissionsData>> CalculateCarbonIntensityAsync(IDictionary props);
    }
}