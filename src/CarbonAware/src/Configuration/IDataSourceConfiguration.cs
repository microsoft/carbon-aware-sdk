using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Configuration;

public interface IDataSourceConfiguration
{
    public string Type { get; set; }
}
