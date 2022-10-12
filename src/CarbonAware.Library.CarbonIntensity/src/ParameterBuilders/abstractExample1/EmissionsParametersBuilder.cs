using CarbonAware.Aggregators.CarbonAware;
namespace CarbonAware.Library.CarbonIntensity;

public class EmissionsParametersBuilder : AbstractCarbonAwareParametersBuilder
{
    // Class overrides build and throws error if setup not valid for this type of parameter
    public override CarbonAwareParameters Build()
    {
        if (locations != null && locations.Any())
        { 
            parameters.MultipleLocations = locations;
        } 
        else {
            // throw error that at least one location is required
        }
        return parameters;
    }
}