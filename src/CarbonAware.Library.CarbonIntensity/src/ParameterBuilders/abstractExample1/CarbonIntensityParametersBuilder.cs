using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Library.CarbonIntensity;

public class CarbonIntensityParametersBuilder : AbstractCarbonAwareParametersBuilder
{
    // Class overrides build and throws error if setup not valid for this type of parameter
    public override CarbonAwareParameters Build()
    {
        if (locations != null)
        { 
            if (locations.Count() == 1) {
                parameters.SingleLocation = locations[0];
            } else {
                // throw error that only one location can be passed in
            }
        } 
        else {
            // throw error that a location is required
        }
        return parameters;
    }
}