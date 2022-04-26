using CarbonAware.Model;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreCalculation
{
    public Location Location { get; set; }
    public string TimeInterval { get; set; }
}