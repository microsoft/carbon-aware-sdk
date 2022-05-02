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
    public string Name => "WattTimeDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<WattTimeDataSource> Logger { get; }

    private IWattTimeClient WattTimeClient { get; }

    private ActivitySource ActivitySource { get; }

    private ILocationSource LocationSource { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The WattTime Client</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    /// <param name="locationSource">The location source to be used to convert a location to BA's.</param>
    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client, ActivitySource activitySource, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.WattTimeClient = client;
        this.ActivitySource = activitySource;
        this.LocationSource = locationSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this.Logger.LogInformation("Getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);
        List<EmissionsData> result = new ();
        foreach (var location in locations)
        {
            IEnumerable<EmissionsData> interimResult = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
            result.AddRange(interimResult);
        }
        return result;
    }

    private async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this.Logger.LogInformation("Getting carbon intensity for location {location} for period {periodStartTime} to {periodEndTime}.", location, periodStartTime, periodEndTime);

        using (var activity = ActivitySource.StartActivity())
        {
            BalancingAuthority? balancingAuthority;
            try
            {
                this.LocationSource.GetGeopositionLocation(location);
                balancingAuthority = await WattTimeClient.GetBalancingAuthorityAsync(location.Latitude.ToString(), location.Longitude.ToString());

                if(balancingAuthority == null) {
                    throw new LocationConversionException();
                }
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

            var data = (await this.WattTimeClient.GetDataAsync(balancingAuthority, periodStartTime, periodEndTime)).ToList();

            Logger.LogDebug("Found {count} total forecasts for location {location} for period {periodStartTime} to {periodEndTime}.", data.Count, location, periodStartTime, periodEndTime);

            // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
            var result = data.Select(e => new EmissionsData() 
            { 
                Location = e.BalancingAuthorityAbbreviation, 
                Rating = e.Value, 
                Time = e.PointTime 
            });

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Found {count} total emissions data records for location {location} for period {periodStartTime} to {periodEndTime}.", result.ToList().Count, location, periodStartTime, periodEndTime);
            }

            return result;
        }
    }
}
