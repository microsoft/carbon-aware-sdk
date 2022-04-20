using CarbonAware.Model;
using CarbonAware.Plugins;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareAggregator : ICarbonAwareAggregator
{
    private readonly ILogger<CarbonAwareAggregator> _logger;

    private readonly ICarbonAware _plugin;

    public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonAware plugin)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
    }

    public async Task<double> CalcEmissionsAverageAsync(IDictionary props)
    {
        ValidateAverageProps(props);
        var list = await GetEmissionsDataAsync(props);
        var value = list.Any() ? list.Select(x => x.Rating).Average() : 0;
        _logger.LogInformation($"Carbon Intensity Average: {value}");
        return value;
    }

    private void ValidateAverageProps(IDictionary props)
    {
        if (!props.Contains(CarbonAwareConstants.Locations) ||
            !props.Contains(CarbonAwareConstants.Start) ||
            !props.Contains(CarbonAwareConstants.End))
        {
            throw new ArgumentException("Missing properties to calculate average");
        }
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        return await _plugin.GetEmissionsDataAsync(props);
    }
}
