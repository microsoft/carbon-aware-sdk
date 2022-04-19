using CarbonAware.Model;
using CarbonAware.Plugins;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Linq;

namespace CarbonAware.Aggregators.CarbonAware
{
    public class CarbonAwareAggregator : ICarbonAwareAggregator
    {
        private readonly ILogger<CarbonAwareAggregator> _logger;

        private readonly ICarbonAware _plugin;

        public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonAware plugin)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._plugin = plugin;
        }

        public async Task<double> GetEmissionsAveragageAsync(IDictionary props)
        {
            var list = await GetEmissionsDataAsync(props);
            return list.Any() ? list.Select(x => x.Rating).Average() : 0;
        }

        public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            return await this._plugin.GetEmissionsDataAsync(props);

        }
    }
}