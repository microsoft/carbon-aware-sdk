

namespace CarbonAware.Aggregators.CarbonAware;

public class EmissionsParametersBuilder : AbstractCarbonAwareParametersBuilder
{
    public override CarbonAwareParameters Build()
    {
        if (locationsAreSet)
        { 
            parameters.MultipleLocations = locations;
        } 
        else {
            // throw error that at least one location is required
        }
        return parameters;
    }
}