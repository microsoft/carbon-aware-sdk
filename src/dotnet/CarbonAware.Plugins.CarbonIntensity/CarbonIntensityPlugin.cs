using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Plugins.CarbonIntensity
{
    public class CarbonIntensityPlugin : ICarbonAware
    {
        private ILogger<CarbonIntensityPlugin> Logger { get; }

        private ICarbonIntensityDataSource DataSource { get; }

        public CarbonIntensityPlugin(ILogger<CarbonIntensityPlugin> logger, ICarbonIntensityDataSource dataSource)
        {
            this.Logger = logger;
            this.DataSource = dataSource;
        }

        public Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            throw new NotImplementedException();
        }
    }
}
