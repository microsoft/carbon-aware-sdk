using CarbonAware.Model;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareParameters
{
    struct RequiredProperties
    {
        public bool SingleLocation { get; set; }
        public bool MultipleLocations { get; set; }
        public bool Start { get; set; }
        public bool End { get; set; }
        public bool Requested { get; set; }
        public bool Duration { get; set; }
    }

    private RequiredProperties _requiredProperties;
    private DateTimeOffset _startDefault = DateTimeOffset.MinValue;
    private DateTimeOffset _start;

    private DateTimeOffset _endDefault = DateTimeOffset.MaxValue;
    private DateTimeOffset _end;

    public Location? SingleLocation { get; set; }
    public IEnumerable<Location>? MultipleLocations { get; set; }
    public DateTimeOffset Start
    {
        get => _requiredProperties.Start && _start == _startDefault ? throw new InvalidOperationException("Start is not set") : _start;
        set => _start = value;
    }
    public DateTimeOffset Start
    {
        get => _requiredProperties.End && _end == _endDefault ? throw new InvalidOperationException("End is not set") : _end;
        set => _end = value;
    }
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
    public DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested) => Requested ?? defaultRequested;
    public TimeSpan DurationOrDefault(TimeSpan defaultDuration) => Duration ?? defaultDuration;

    public CarbonAwareParameters()
    {
        _requiredProperties = new RequiredProperties
        {
            SingleLocation = false,
            MultipleLocations = false,
            Start = false,
            End = false,
            Requested = false,
            Duration = false
        };
        _start = _startDefault;
        _end = _endDefault;
    }

    public void SetRequiredProperties(
        bool singleLocation = false,
        bool multipleLocations = false,
        bool start = false,
        bool end = false,
        bool requested = false,
        bool duration = false)
    {
        _requiredProperties.SingleLocation = singleLocation;
        _requiredProperties.MultipleLocations = multipleLocations;
        _requiredProperties.Start = start;
        _requiredProperties.End = end;
        _requiredProperties.Requested = requested;
        _requiredProperties.Duration = duration;
    }

    public void SetPropertyDefaults(
        DateTimeOffset start = _startDefault,
        DateTimeOffset end = _endDefault)
    {
        _startDefault = start;
        _endDefault = end;
    }

    public void Validate(bool startBeforeEndRequired = false)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validate Properties
        if (_requiredProperties.SingleLocation && SingleLocation == null)
        {
            errors.AppendValue(SingleLocationDisplayName, $"{SingleLocationDisplayName} is required");
        }
        if (_requiredProperties.MultipleLocations && (MultipleLocations == null || !MultipleLocations.Any()))
        {
            errors.AppendValue(MultipleLocationsDisplayName, $"{MultipleLocationsDisplayName} is required");
        }
        if (_requiredProperties.Start && Start == _startDefault)
        {
            errors.AppendValue(StartDisplayName, $"{StartDisplayName} is required");
        }
        if (_requiredProperties.End && End == _endDefault)
        {
            errors.AppendValue(EndDisplayName, $"{EndDisplayName} is required");
        }
        if (_requiredProperties.Requested && Requested == null)
        {
            errors.AppendValue(RequestedDisplayName, $"{RequestedDisplayName} is required");
        }
        if (_requiredProperties.Duration && Duration == null)
        {
            errors.AppendValue(DurationDisplayName, $"{DurationDisplayName} is required");
        }

        CheckErrors(errors);

        // Validate Relationships
        if (startBeforeEndRequired)
        {
            if (Start > End)
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