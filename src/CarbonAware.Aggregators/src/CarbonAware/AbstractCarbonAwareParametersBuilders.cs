namespace CarbonAware.Aggregators.CarbonAware;

public abstract class AbstractCarbonAwareParametersBuilder
{
    internal CarbonAwareParametersBaseDTO parameters;

    internal bool startIsSet = false;
    internal bool endIsSet = false;
    internal bool locationsAreSet = false;
    internal bool durationIsSet = false;
    internal string[]? locations;

    public AbstractCarbonAwareParametersBuilder() 
    {
        parameters = new CarbonAwareParametersBaseDTO();
    }

    public abstract CarbonAwareParameters Build();

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
}