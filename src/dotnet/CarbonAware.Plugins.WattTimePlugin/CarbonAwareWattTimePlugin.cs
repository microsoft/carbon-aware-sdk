using CarbonAware.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Plugins.WattTimePlugin
{
    public class CarbonAwareWattTimePlugin : ICarbonAware
    {
        public Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            throw new NotImplementedException();
        }
    }
}
