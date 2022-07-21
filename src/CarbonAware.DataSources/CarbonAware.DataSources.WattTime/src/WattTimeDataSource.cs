﻿using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

    private static readonly ActivitySource Activity = new ActivitySource(nameof(WattTimeDataSource));

    private ILocationSource LocationSource { get; }

    const double MWH_TO_KWH_CONVERSION_FACTOR = 1000.0;
    const double LBS_TO_GRAMS_CONVERSION_FACTOR = 453.59237;
    public double MinSamplingWindow => 120; // 2hrs of data


    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The WattTime Client</param>
    /// <param name="locationSource">The location source to be used to convert a location to BA's.</param>
    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.WattTimeClient = client;
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
    
    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location}");

        using (var activity = Activity.StartActivity())
        {
            var balancingAuthority = await this.GetBalancingAuthority(location, activity);
            var data = await this.WattTimeClient.GetCurrentForecastAsync(balancingAuthority);
            return ForecastToEmissionsForecast(data, location);
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset generatedAt)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location} requested at {generatedAt}");

        using (var activity = Activity.StartActivity())
        {
            var balancingAuthority = await this.GetBalancingAuthority(location, activity);
            Forecast? forecast = await this.WattTimeClient.GetForecastOnDateAsync(balancingAuthority, generatedAt);
            if (forecast == null) {
                Exception ex = new ArgumentException($"No forecast was generated at the requested time {generatedAt}");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogError(ex, ex.Message);
                throw ex;
            }
            return ForecastToEmissionsForecast(forecast, location);
        }
    }

    private EmissionsForecast ForecastToEmissionsForecast(Forecast forecast, Location location) 
    {
        var duration = GetDurationFromGridEmissionDataPoints(forecast.ForecastData.FirstOrDefault(), forecast.ForecastData.Skip(1)?.FirstOrDefault());
        var forecastData = forecast.ForecastData.Select(e => new EmissionsData()
        {
            Location = e.BalancingAuthorityAbbreviation,
            Rating = ConvertMoerToGramsPerKilowattHour(e.Value),
            Time = e.PointTime,
            Duration = duration
        });
        var emForecast = new EmissionsForecast()
        {
            GeneratedAt = forecast.GeneratedAt,
            Location = location,
            ForecastData = forecastData
        };
        return emForecast;
    }

    private async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        Logger.LogInformation($"Getting carbon intensity for location {location} for period {periodStartTime} to {periodEndTime}.");

        using (var activity = Activity.StartActivity())
        {
            var balancingAuthority = await this.GetBalancingAuthority(location, activity);
            var (newStartTime, newEndTime) = IntervalHelper.ExtendTimeByWindow(periodStartTime, periodEndTime, MinSamplingWindow);
            var data = await this.WattTimeClient.GetDataAsync(balancingAuthority, newStartTime, newEndTime);
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug($"Found {data.Count()} total forecasts for location {location} for period {periodStartTime} to {periodEndTime}.");
            }
            var windowData = ConvertToEmissionsData(data);
            var filteredData = IntervalHelper.FilterByDuration(windowData, periodStartTime, periodEndTime);

            if (!filteredData.Any())
            {
                Logger.LogInformation($"Not enough data with {MinSamplingWindow} window");
            }
            return filteredData;
        }
    }

    internal double ConvertMoerToGramsPerKilowattHour(double value)
    {
        return value * LBS_TO_GRAMS_CONVERSION_FACTOR / MWH_TO_KWH_CONVERSION_FACTOR;
    }

    private IEnumerable<EmissionsData> ConvertToEmissionsData(IEnumerable<GridEmissionDataPoint> data)
    {
        // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
        return data.Select(e => new EmissionsData() 
                    { 
                        Location = e.BalancingAuthorityAbbreviation, 
                        Rating = ConvertMoerToGramsPerKilowattHour(e.Value), 
                        Time = e.PointTime,
                        Duration = FrequencyToTimeSpan(e.Frequency)
                    });
    }

    private TimeSpan GetDurationFromGridEmissionDataPoints(GridEmissionDataPoint? firstPoint, GridEmissionDataPoint? secondPoint)
    {
        var first = firstPoint ?? throw new WattTimeClientException("Too few data points returned"); 
        var second = secondPoint ?? throw new WattTimeClientException("Too few data points returned");

        return second.PointTime - first.PointTime;
    }

    private TimeSpan FrequencyToTimeSpan(int? frequency)
    {
        return (frequency != null) ? TimeSpan.FromSeconds((double)frequency) : TimeSpan.Zero;
    }

    private async Task<BalancingAuthority> GetBalancingAuthority(Location location, Activity? activity)
    {
        BalancingAuthority balancingAuthority;
        try
        {
            var geolocation = await this.LocationSource.ToGeopositionLocationAsync(location);
            balancingAuthority = await WattTimeClient.GetBalancingAuthorityAsync(geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "");
        }
        catch(Exception ex) when (ex is LocationConversionException ||  ex is WattTimeClientHttpException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Logger.LogError(ex, "Failed to convert the location {location} into a Balancying Authority.", location);
            throw;
        }

        activity?.AddTag("location", location);
        activity?.AddTag("balancingAuthorityAbbreviation", balancingAuthority.Abbreviation);

        Logger.LogDebug("Converted location {location} to balancing authority {balancingAuthorityAbbreviation}", location, balancingAuthority.Abbreviation);

        return balancingAuthority;
    }
}
