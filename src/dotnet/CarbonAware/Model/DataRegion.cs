namespace CarbonAware.Model;

public class DataRegion
{
    public string Name { get; set; } = string.Empty;
    public RegionMetadata Metadata { get; set; } = new RegionMetadata();
}
