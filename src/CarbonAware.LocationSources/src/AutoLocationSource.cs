using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.LocationSources.Configuration;
using CarbonAware.LocationSources.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;

namespace CarbonAware.LocationSources;

/// <summary>
/// Represents a location source.
/// </summary>
public class AutoLocationSource : ILocationSource
{
    private readonly ILogger<AutoLocationSource> _logger;

    private IDictionary<string, Location> _allLocations;

    private IOptionsMonitor<LocationDataSourcesConfiguration> _configurationMonitor { get; }

    private LocationDataSourcesConfiguration _configuration => _configurationMonitor.CurrentValue;

    private readonly IIpStackClient _client;

    /// <summary>
    /// Creates a new instance of the <see cref="LocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the LocationSource</param>
    public AutoLocationSource(IIpStackClient client, ILogger<AutoLocationSource> logger, IOptionsMonitor<LocationDataSourcesConfiguration> monitor)
    {
        _client = client;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _allLocations = new Dictionary<string, Location>(StringComparer.InvariantCultureIgnoreCase);
        // TODO create client to reach https://ipstack.com/ (it might not work behind vpn)
        // TODO find from specific provider list of data centers
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, Location>> GetGeopositionLocationsAsync()
    {
        // throw new NotImplementedException();
        // HttpClient client = 
        return _allLocations;
    }
}
