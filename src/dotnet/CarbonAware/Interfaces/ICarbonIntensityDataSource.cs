namespace CarbonAware.Interfaces;

/// <summary>
/// Represents a data source for carbon intensity.
/// </summary>
public interface ICarbonIntensityDataSource
{
    /// <summary>
    /// Gets the carbon intensity for a given SciScoreCalculation.
    /// </summary>
    /// <param name="region">The region to get carbon data for.</param>
    /// <param name="startPeriod">The start period.</param>
    /// <param name="endPeriod">The end period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(string region, DateTimeOffset startPeriod, DateTimeOffset endPeriod);
}
