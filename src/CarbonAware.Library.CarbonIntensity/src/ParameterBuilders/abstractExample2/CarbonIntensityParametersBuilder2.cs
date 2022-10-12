namespace CarbonAware.Library.CarbonIntensity;

public class CarbonIntensityParametersBuilder2 : AbstractCarbonAwareParametersBuilder2
{
    // Class defines location function to access specific, internal only location setter
    public void AddLocation(string location) {
        AddSingleLocation(location);
    }
}