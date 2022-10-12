

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonIntensityParametersBuilder : AbstractCarbonAwareParametersBuilder
{
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