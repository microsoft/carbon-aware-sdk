

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareParametersBuilder
{
    public enum ParameterType { EmissionsParameters, CurrentForecastParameters, ForecastParameters, CarbonIntensityParameters };
    private CarbonAwareParametersBaseDTO parameters;
    private ParameterType parameterType;

    private bool startIsSet = false;
    private bool endIsSet = false;
    private bool locationsAreSet = false;
    private bool durationIsSet = false;

    private string[]? locations;


    public CarbonAwareParametersBuilder(ParameterType type) 
    {
        parameters = new CarbonAwareParametersBaseDTO();
        parameterType = type;
    }

    public CarbonAwareParameters Build() 
    {
        ValidateParameterType();
        return parameters;
    }
    
    public void AddStartTime(DateTimeOffset start) {
        parameters.Start = start;
        startIsSet = true;
    }
    public void AddEndTime(DateTimeOffset end) {
        parameters.End = end;
        endIsSet = true;
    }
    public void AddLocations(string[] locs) {
        locations = locs;
        locationsAreSet = true;
    }

    public void AddDuration(int duration) {
        parameters.Duration = duration;
        durationIsSet = true;
    }

    private void ValidateParameterType()
    {
        switch(parameterType)
        {
            case ParameterType.EmissionsParameters:
            case ParameterType.CurrentForecastParameters:
            {
                if (locationsAreSet)
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
                if (locations != null)
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
}