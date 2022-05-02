using CarbonAware.Model;
namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreInput
{
    public Location? Location { get; set; }
    public string TimeInterval { get; set; } = string.Empty;
}