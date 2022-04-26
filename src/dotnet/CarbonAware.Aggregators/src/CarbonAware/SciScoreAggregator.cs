using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Globalization;

namespace CarbonAware.Aggregators.SciScore
{
    public class SciScoreAggregator : ISciScoreAggregator
    {
        private readonly ILogger<SciScoreAggregator> _logger;

        private readonly ICarbonIntensityDataSource _carbonIntensityDataSource;

        public SciScoreAggregator(ILogger<SciScoreAggregator> logger, ICarbonIntensityDataSource carbonIntensityDataSource)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._carbonIntensityDataSource = carbonIntensityDataSource;
        }
        public async Task<double> CalculateAverageCarbonIntensityAsync(Location location, string timeInterval)
        {
            (DateTimeOffset start, DateTimeOffset end) = this.ParseTimeInterval(timeInterval);
            var emissionData = await this._carbonIntensityDataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);

            // check whether the emissionData list is empty, if not, return Rating's average, otherwise 0.
            var value = emissionData.Any() ? emissionData.Select(x => x.Rating).Average() : 0;
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }

        private (DateTimeOffset start, DateTimeOffset end) ParseTimeInterval(string timeInterval)
        {
            DateTimeOffset start;
            DateTimeOffset end;
            try
            {
                var timeIntervals = timeInterval.Split('/');
                start = DateTimeOffset.Parse(timeIntervals[0], CultureInfo.InvariantCulture.DateTimeFormat);
                end = DateTimeOffset.Parse(timeIntervals[1], CultureInfo.InvariantCulture.DateTimeFormat);
            }
            catch (Exception) 
            {
                throw new ArgumentException("Invalid TimeInterval");
            }

            return (start, end);
        }

    }
}