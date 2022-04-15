using CarbonAware.Interfaces;
using CarbonAware.Tools.WattTimeClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime
{
    public class WattTimeDataSource : ICarbonIntensityDataSource
    {
        private ILogger<WattTimeDataSource> Logger { get; }
        private IWattTimeClient WattTimeClient { get; }

        public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client)
        {
            this.Logger = logger;
            this.WattTimeClient = client;
        }
    }
}
