using CarbonAware.Tools.WattTimeClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime
{
    /// <summary>
    /// Class RegionConverter.
    /// </summary>
    public class RegionConverter : IRegionConverter
    {
        /// <inheritdoc />
        public Task<BalancingAuthority?> ConvertAzureRegionAsync(string region)
        {
            throw new NotImplementedException();
        }
    }
}
