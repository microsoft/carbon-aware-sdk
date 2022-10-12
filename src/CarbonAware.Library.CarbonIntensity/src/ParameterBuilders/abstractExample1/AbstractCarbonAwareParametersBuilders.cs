using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Library.CarbonIntensity;

/// <summary>
/// Abstract class with children builder where children do validation at `Build()` call by overriding implementation
/// </summary>
public abstract class AbstractCarbonAwareParametersBuilder
{
    internal CarbonAwareParametersBaseDTO parameters;

    internal string[]? locations;

    public AbstractCarbonAwareParametersBuilder() 
    {
        parameters = new CarbonAwareParametersBaseDTO();
    }

    public abstract CarbonAwareParameters Build();

    public void AddStartTime(DateTimeOffset start) {
        parameters.Start = start;
    }
    public void AddEndTime(DateTimeOffset end) {
        parameters.End = end;
    }
    public void AddLocations(string[] locs) {
        locations = locs;
    }

    public void AddDuration(int duration) {
        parameters.Duration = duration;
    }
}