namespace CarbonAware.LocationSources.Azure.Model;

public class AzureRegionJson
{
    public string Name { get; set; } = string.Empty;
    public AzureRegionMetadata Metadata { get; set; } = new AzureRegionMetadata();
}
