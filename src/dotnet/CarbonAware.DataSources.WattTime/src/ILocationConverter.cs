using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.DataSources.WattTime;

/// <summary>
/// Represents a WattTime location converter.
/// </summary>
public interface ILocationConverter
{
    /// <summary>
    /// Converts a location to a its geological coordinates (lat/long).
    /// </summary>
    /// <param name="location">The location to convert.</param>
    /// <returns>Region metadata that contains latitude and longitude for the region</returns>
    /// <exception cref="LocationConversionException">Thrown when the given location can't be converted to lat/long.</exception>
    public Task<RegionMetadata> ConvertLocationToLatLongAsync(Location location);
}
