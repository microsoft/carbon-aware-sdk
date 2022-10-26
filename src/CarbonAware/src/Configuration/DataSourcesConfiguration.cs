using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Configuration;
public class DataSourcesConfiguration
{
    public const string Key = "DataSources";
    public string EmissionsDataSource { get; set; }
    public string ForecastDataSource { get; set; }
    public IDictionary<string, IDataSourceConfiguration> Configurations { get; set; }

}
