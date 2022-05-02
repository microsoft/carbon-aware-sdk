namespace CarbonAware.Interfaces;

/// <summary>
/// Represents a location source for Location type.
/// </summary>
public interface ILocationSource
{
    string Name { get; }
    string Description { get; }

    public Location ToGeopositionLocation(Location location);
}
