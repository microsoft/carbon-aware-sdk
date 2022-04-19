using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

namespace CarbonAware.DataSources.WattTime;

/// <summary>
/// Reprsents a wattime data source.
/// </summary>
public class WattTimeDataSource : ICarbonIntensityDataSource
{
    private ILogger<WattTimeDataSource> Logger { get; }

    private IWattTimeClient WattTimeClient { get; }

    private ActivitySource ActivitySource { get; }

    private IRegionConverter RegionConverter { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The WattTime Client</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    /// <param name="regionConverter">The region converter to be used to convert Azure regions to BA's.</param>
    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client, ActivitySource activitySource, IRegionConverter regionConverter)
    {
        this.Logger = logger;
        this.WattTimeClient = client;
        this.ActivitySource = activitySource;
        this.RegionConverter = regionConverter;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(string region, DateTimeOffset startPeriod, DateTimeOffset endPeriod)
    {
        this.Logger.LogInformation("Getting carbon intensity for region {region} for period {startPeriod} to {endPeriod}.", region, startPeriod, endPeriod);

        using (var activity = ActivitySource.StartActivity())
        {
            var balancingAuthority = await this.RegionConverter.ConvertAzureRegionAsync(region);

            if (balancingAuthority == null)
            {
                Logger.LogError("Unable to find a balancing authority for region {region}", region);
                throw new Exception($"Unable to find a balancing authority for region {region}");
            }

            activity?.AddTag("region", region);
            activity?.AddTag("balancingAuthorityAbbreviation", balancingAuthority.Abbreviation);

            Logger.LogDebug("Converted region {region} to balancing authority {balancingAuthorityAbbreviation}", region, balancingAuthority.Abbreviation);

            var data = (await this.WattTimeClient.GetForecastByDateAsync(balancingAuthority, startPeriod.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), endPeriod.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))).ToList();

            Logger.LogDebug("Found {count} total forcasts for region {region} for period {startPeriod} to {endPeriod}.", data.Count, region, startPeriod, endPeriod);

            var result = data.SelectMany(i => i.ForecastData).Select(e => new EmissionsData() 
            { 
                Location = region, 
                Rating = e.Value, 
                Time = e.PointTime 
            }).ToList();

            Logger.LogDebug("Found {count} total emissions data records for region {region} for period {startPeriod} to {endPeriod}.", result.Count, region, startPeriod, endPeriod);

            return result;
        }
    }
}
