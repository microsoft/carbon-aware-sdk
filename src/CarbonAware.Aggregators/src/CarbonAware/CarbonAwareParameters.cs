using CarbonAware.Model;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareParameters
{
    public Location? SingleLocation { get; set; }
    public IEnumerable<Location>? MultipleLocations { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public DateTimeOffset? Requested { get; set; }
    public TimeSpan? Duration { get; set; }
    
    // User-facing display names for the parameters.
    public string SingleLocationDisplayName { get; set; } = "location";
    public string MultipleLocationsDisplayName { get; set; }= "locations";
    public string StartDisplayName { get; set; } = "start";
    public string EndDisplayName { get; set; } = "end";
    public string RequestedDisplayName { get; set; } = "requested";
    public string DurationDisplayName { get; set; } = "duration";

    // Accessors with defaults 
    public DateTimeOffset StartOrDefault(DateTimeOffset defaultStart) => Start ?? defaultStart;
    public DateTimeOffset EndOrDefault(DateTimeOffset defaultEnd) => End ?? defaultEnd;
    public DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested) => Requested ?? defaultRequested;
    public TimeSpan DurationOrDefault(TimeSpan defaultDuration) => Duration ?? defaultDuration;


    public void Validate(
        bool singleLocationRequired = false,
        bool multipleLocationsRequired = false,
        bool startRequired = false,
        bool endRequired = false,
        bool requestedRequired = false,
        bool durationRequired = false,
        bool startBeforeEndRequired = false
    )
    {
        var errors = new Dictionary<string, List<string>>();

        // Validate Properties
        if (singleLocationRequired && SingleLocation == null)
        {
            errors.AppendValue(SingleLocationDisplayName, $"{SingleLocationDisplayName} is required");
        }
        if (multipleLocationsRequired && (MultipleLocations == null || !MultipleLocations.Any()))
        {
            errors.AppendValue(MultipleLocationsDisplayName, $"{MultipleLocationsDisplayName} is required");
        }
        if (startRequired && Start == null)
        {
            errors.AppendValue(StartDisplayName, $"{StartDisplayName} is required");
        }
        if (endRequired && End == null)
        {
            errors.AppendValue(EndDisplayName, $"{EndDisplayName} is required");
        }
        if (requestedRequired && Requested == null)
        {
            errors.AppendValue(RequestedDisplayName, $"{RequestedDisplayName} is required");
        }
        if (durationRequired && Duration == null)
        {
            errors.AppendValue(DurationDisplayName, $"{DurationDisplayName} is required");
        }

        CheckErrors(errors);

        // Validate Relationships
        if (startBeforeEndRequired)
        {
            if (Start != null && End != null && Start > End)
            {
                errors.AppendValue(StartDisplayName, $"{StartDisplayName} must be before {EndDisplayName}");
            }
        }

        CheckErrors(errors);
    }

    private void CheckErrors(Dictionary<string, List<string>> errors)
    {
        if (errors.Keys.Count > 0)
        {
            ArgumentException error = new ArgumentException("Invalid parameters");
            foreach (KeyValuePair<string, List<string>> message in errors)
            {
                error.Data[message.Key] = message.Value.ToArray();
            }
            throw error;
        }
    }
}

// Ease-of-use extension method for our error dictionary.
public static class CarbonAwareParametersExtensions
{
    public static void AppendValue(this Dictionary<string, List<string>> dict, string key, string value)
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = new List<string>();
        }
        dict[key].Add(value);
    }
}