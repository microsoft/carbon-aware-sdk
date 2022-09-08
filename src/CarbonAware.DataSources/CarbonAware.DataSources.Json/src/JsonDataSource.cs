﻿using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CarbonAware.DataSources.Json;

/// <summary>
/// Represents a JSON data source.
/// </summary>
public class JsonDataSource : ICarbonIntensityDataSource
{
    public string Name => "JsonDataSource";

    public string Description => "Example plugin to read data from a json for Carbon Aware SDK";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    public double MinSamplingWindow => 1440;  // 24 hrs

    private List<EmissionsData>? emissionsData;

    private readonly ILogger<JsonDataSource> _logger;

    private const double DURATION = 8; // 8 hrs

    private IOptionsMonitor<JsonDataConfiguration> ConfigurationMonitor { get; }

    private JsonDataConfiguration Configuration => ConfigurationMonitor.CurrentValue;



    /// <summary>
    /// Creates a new instance of the <see cref="JsonDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public JsonDataSource(ILogger<JsonDataSource> logger, IOptionsMonitor<JsonDataConfiguration> monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    /// <inheritdoc />
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        return GetCarbonIntensityAsync(new List<Location>() { location }, periodStartTime, periodEndTime);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        _logger.LogInformation("JSON data source getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);

        IEnumerable<EmissionsData>? emissionsData = await GetSampleJsonAsync();
        if (emissionsData == null || !emissionsData.Any()) {
            _logger.LogDebug("Emission data list is empty");
            return Enumerable.Empty<EmissionsData>();
        }
        _logger.LogDebug($"Total emission records retrieved {emissionsData.Count()}");
        var stringLocations = locations.Select(loc => loc.RegionName);
            
        emissionsData = FilterByLocation(emissionsData, stringLocations);
        emissionsData = FilterByDateRange(emissionsData, periodStartTime, periodEndTime);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Found {count} total emissions data records for locations {stringLocations} for period {periodStartTime} to {periodEndTime}.", emissionsData.Count(), stringLocations, periodStartTime, periodEndTime);
        }

        return emissionsData;
    }

    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        throw new NotImplementedException();
    }

    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset generatedAt)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<EmissionsData> FilterByDateRange(IEnumerable<EmissionsData> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var (newStartTime, newEndTime) = IntervalHelper.ExtendTimeByWindow(startTime, endTime, MinSamplingWindow);
        var windowData = data.Where(ed => ed.TimeBetween(newStartTime, newEndTime));
        var filteredData = IntervalHelper.FilterByDuration(windowData, startTime, endTime, TimeSpan.FromHours(DURATION));

        if (!filteredData.Any())
        {
            _logger.LogInformation($"Not enough data with {MinSamplingWindow} window");
        }
        return filteredData;
    }

    private IEnumerable<EmissionsData> FilterByLocation(IEnumerable<EmissionsData> data, IEnumerable<string?> locations)
    {
        if (locations.Any()) 
        {
            data = data.Where(ed => locations.Contains(ed.Location));
        }

        return data;
    }

    protected virtual async Task<List<EmissionsData>?> GetSampleJsonAsync()
    {
        if (emissionsData is not null)
        {
            return emissionsData;
        }
        using Stream stream = GetStreamFromFileLocation();
        var jsonObject = await JsonSerializer.DeserializeAsync<EmissionsJsonFile>(stream);
        if (emissionsData is null || !emissionsData.Any()) {
            emissionsData = jsonObject?.Emissions;
        }
        return emissionsData;
    }

    private Stream GetStreamFromFileLocation()
    {
        _logger.LogInformation($"Reading Json data from {Configuration.DataFileLocation}");
        return File.OpenRead(Configuration.DataFileLocation!);
    }
}
