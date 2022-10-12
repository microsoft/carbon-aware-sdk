using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Library.CarbonIntensity;

/// <summary>
/// Single class builder that does field validation real-time as users try to set it based on instantiated ParameterType
/// </summary>
public class CarbonAwareParametersBuilder2
{
    public enum ParameterType { EmissionsParameters, CurrentForecastParameters, ForecastParameters, CarbonIntensityParameters };
    private CarbonAwareParametersBaseDTO parameters;
    private ParameterType parameterType;

    public CarbonAwareParametersBuilder2(ParameterType type) 
    {
        parameters = new CarbonAwareParametersBaseDTO();
        parameterType = type;
    }

    public CarbonAwareParameters Build() 
    {
        return parameters;
    }
    
    public void AddStartTime(DateTimeOffset start) {
        parameters.Start = start;
    }
    public void AddEndTime(DateTimeOffset end) {
        parameters.End = end;
    }
    public void AddLocations(string[] locations) 
    {
        switch(parameterType)
        {
            case ParameterType.EmissionsParameters:
            case ParameterType.CurrentForecastParameters:
            {
                if (locations.Any())
                { 
                    parameters.MultipleLocations = locations;
                    break;
                } 
                else {
                    // throw error that at least one location is required
                    break;
                }
            }
            case ParameterType.ForecastParameters:
            case ParameterType.CarbonIntensityParameters:
            {
                if (locations.Any())
                { 
                    if (locations.Count() == 1) {
                        parameters.SingleLocation = locations[0];
                        break;
                    } else {
                        // throw error that only one location can be passed in
                        break;
                    }
                } 
                else {
                    // throw error that a location is required
                    break;
                }
            }
        }
    }

    public void AddDuration(int duration) {
        parameters.Duration = duration;
    }
}