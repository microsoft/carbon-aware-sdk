using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Plugins.CarbonIntensity
{
    public class CarbonIntensityPlugin : ICarbonAwarePlugin
    {
        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string Author => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public object URL => throw new NotImplementedException();

        private ILogger<CarbonIntensityPlugin> Logger { get; }

        private ICarbonIntensityDataSource DataSource { get; }

        public CarbonIntensityPlugin(ILogger<CarbonIntensityPlugin> logger, ICarbonIntensityDataSource dataSource)
        {
            this.Logger = logger;
            this.DataSource = dataSource;
        }

        public void Configure(IConfigurationSection config)
        {
            throw new NotImplementedException();
        }

        public List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
        {
            throw new NotImplementedException();
        }

        public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
        {
            throw new NotImplementedException();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
        {
            throw new NotImplementedException();
        }
    }
}
