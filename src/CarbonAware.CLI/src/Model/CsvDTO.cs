using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.CLI.Model;
class CsvDTO
{
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Location { get; set; } = string.Empty;
    public DateTimeOffset DataStartAt { get; set; }
    public DateTimeOffset DataEndAt { get; set; }
    public int WindowSize { get; set; }
    ///<example> 2022-09-19T17:30:00Z </example>   
    public DateTimeOffset? Time { get; set; }
    ///<example> 140.5 </example>
    public double Rating { get; set; }
    ///<example>1.12:24:02 </example>
    public TimeSpan? Duration { get; set; }
}
