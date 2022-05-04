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

        /// <inheritdoc />
        public async Task<double> CalculateAverageCarbonIntensityAsync(Location location, string timeInterval)
        {
            (DateTimeOffset start, DateTimeOffset end) = this.ParseTimeInterval(timeInterval);
            var emissionData = await this._carbonIntensityDataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);

            // check whether the emissionData list is empty, if not, return Rating's average, otherwise 0.
            var value = emissionData.Any() ? emissionData.Select(x => x.Rating).Average() : 0;
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }

        // Validate and parse time interval string into a tuple of (start, end) DateTimeOffsets.
        // Throws ArgumentException for invalid input.
        private (DateTimeOffset start, DateTimeOffset end) ParseTimeInterval(string timeInterval)
        {
            DateTimeOffset start;
            DateTimeOffset end;

            var timeIntervals = timeInterval.Split('/');
            // Check that the time interval was split into exactly 2 parts
            if(timeIntervals.Length != 2)
            {
                throw new ArgumentException(
                    $"Invalid TimeInterval. Expected exactly 2 dates separated by '/', recieved: {timeInterval}"
                );
            }

            var rawStart = timeIntervals[0];
            var rawEnd = timeIntervals[1];

            if(!DateTimeOffset.TryParse(rawStart, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out start))
            {
                throw new ArgumentException($"Invalid TimeInterval. Could not parse start time: {rawStart}");
            }

            if(!DateTimeOffset.TryParse(rawEnd, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out end))
            {
                throw new ArgumentException($"Invalid TimeInterval. Could not parse end time: {rawEnd}");
            }

            if(start > end)
            {
                throw new ArgumentException($"Invalid TimeInterval. Start time must come before end time: {timeInterval}");
            }

            return (start, end);
        }

    }
}