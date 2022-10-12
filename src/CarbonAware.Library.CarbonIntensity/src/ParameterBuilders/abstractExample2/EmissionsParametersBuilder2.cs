namespace CarbonAware.Library.CarbonIntensity;

public class EmissionsParametersBuilder2 : AbstractCarbonAwareParametersBuilder2
{
    // Class defines location function to access specific, internal only location setterQ
    public void AddLocations(string[] locations) {
        AddMultipleLocations(locations);
    }
}