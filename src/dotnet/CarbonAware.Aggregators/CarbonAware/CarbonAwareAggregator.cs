using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Aggregators.CarbonAware
{
    public class CarbonAwareAggregator : ICarbonAwareAggregator
    {
        private ILogger<CarbonAwareAggregator> Logger { get; }

        private ICarbonAware Plugin { get; }

        public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonAware plugin)
        {
            this.Logger = logger;
            this.Plugin = plugin;
        }

        public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            return await this.Plugin.GetEmissionsDataAsync(props);

        }
    }
}
