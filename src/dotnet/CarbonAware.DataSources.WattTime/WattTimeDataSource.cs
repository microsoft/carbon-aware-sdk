﻿using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
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

    private ILocationConverter LocationConverter { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The WattTime Client</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    /// <param name="locationConverter">The location converter to be used to convert a location to BA's.</param>
    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client, ActivitySource activitySource, ILocationConverter locationConverter)
    {
        this.Logger = logger;
        this.WattTimeClient = client;
        this.ActivitySource = activitySource;
        this.LocationConverter = locationConverter;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset startPeriod, DateTimeOffset endPeriod)
    {
        this.Logger.LogInformation("Getting carbon intensity for location {location} for period {startPeriod} to {endPeriod}.", location, startPeriod, endPeriod);

        using (var activity = ActivitySource.StartActivity())
        {
            BalancingAuthority balancingAuthority;
            try
            {
                balancingAuthority = await this.LocationConverter.ConvertLocationToBalancingAuthorityAsync(location);
            }
            catch(LocationConversionException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogError(ex, "Failed to convert the location {location} into a Balancying Authority.", location);
                throw;
            }

            activity?.AddTag("location", location);
            activity?.AddTag("balancingAuthorityAbbreviation", balancingAuthority.Abbreviation);

            Logger.LogDebug("Converted location {location} to balancing authority {balancingAuthorityAbbreviation}", location, balancingAuthority.Abbreviation);

            var data = (await this.WattTimeClient.GetForecastByDateAsync(balancingAuthority, startPeriod, endPeriod)).ToList();

            Logger.LogDebug("Found {count} total forecasts for location {location} for period {startPeriod} to {endPeriod}.", data.Count, location, startPeriod, endPeriod);

            // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
            var result = data.SelectMany(i => i.ForecastData).Select(e => new EmissionsData() 
            { 
                Location = balancingAuthority.Abbreviation, 
                Rating = e.Value, 
                Time = e.PointTime 
            });

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Found {count} total emissions data records for location {location} for period {startPeriod} to {endPeriod}.", result.ToList().Count, location, startPeriod, endPeriod);
            }

            return result;
        }
    }
}
