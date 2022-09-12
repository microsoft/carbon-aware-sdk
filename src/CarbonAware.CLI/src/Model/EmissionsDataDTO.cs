using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.CLI.Model
{
    class EmissionsDataDTO
    {
        public string? Location { get; set; }
        ///<example> 01-01-2022 </example>   
        public DateTimeOffset? Time { get; set; }
        ///<example> 140.5 </example>
        public double Rating { get; set; }
        ///<example>1.12:24:02 </example>
        public TimeSpan? Duration { get; set; }

    }
}
