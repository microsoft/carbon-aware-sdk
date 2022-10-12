using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Library.CarbonIntensity;

/// <summary>
/// Abstract class with children builder where certain functions are internal only and children expose specific functions to enforce valid inputs from the getgo
/// </summary>
public abstract class AbstractCarbonAwareParametersBuilder2
{
    internal CarbonAwareParametersBaseDTO parameters;

    public AbstractCarbonAwareParametersBuilder2() 
    {
        parameters = new CarbonAwareParametersBaseDTO();
    }

    public CarbonAwareParameters Build() {
        return parameters;
    }

    public void AddStartTime(DateTimeOffset start) {
        parameters.Start = start;
    }
    public void AddEndTime(DateTimeOffset end) {
        parameters.End = end;
    }
    public void AddDuration(int duration) {
        parameters.Duration = duration;
    }
    internal void AddMultipleLocations(string[] locations) {
        parameters.MultipleLocations = locations;
    }

    internal void AddSingleLocation(string location) {
        parameters.SingleLocation = location;
    }
}