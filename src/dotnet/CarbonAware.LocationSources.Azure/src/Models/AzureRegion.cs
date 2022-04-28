namespace CarbonAware.LocationSources.Azure.Model;

public class AzureRegion
{
    public string Name { get; set; } = string.Empty;
    public AzureRegionMetadata Metadata { get; set; } = new AzureRegionMetadata();
}
