using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.DataSources.WattTime;

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
