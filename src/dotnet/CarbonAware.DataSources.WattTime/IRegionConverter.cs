using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.DataSources.WattTime;

public interface IRegionConverter
{
    /// <summary>
    /// Converts an azure region string into a balancing authority for WattTime.
    /// </summary>
    /// <param name="region">The region name to convert.</param>
    /// <returns>The balancing authority to use or null if not found.</returns>
    public Task<BalancingAuthority?> ConvertAzureRegionAsync(string region);
}
