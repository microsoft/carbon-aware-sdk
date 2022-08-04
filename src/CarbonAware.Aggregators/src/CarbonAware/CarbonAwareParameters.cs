using CarbonAware.Model;
using System.Reflection;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareParameters
{
    internal Location? _singleLocation;
    internal IEnumerable<Location>? _multipleLocations;
    internal DateTimeOffset? _start;
    internal DateTimeOffset? _end;
    internal DateTimeOffset? _requested;
    internal TimeSpan? _duration;
    
    // Public getters and setters for the parameters.
    public Location SingleLocation
    {
        get => _singleLocation ?? throw new ArgumentNullException(nameof(SingleLocation));
        set => _singleLocation = value; 
    }
    public IEnumerable<Location> MultipleLocations
    {
        get => _multipleLocations ?? throw new ArgumentNullException(nameof(MultipleLocations));
        set => _multipleLocations = value;
    }
    public DateTimeOffset Start
    {
        get => _start ?? throw new ArgumentNullException(nameof(Start));
        set => _start = value;
    }
    public DateTimeOffset End
    {
        get => _end ?? throw new ArgumentNullException(nameof(End));
        set => _end = value;
    }
    public DateTimeOffset Requested
    {
        get => _requested ?? throw new ArgumentNullException(nameof(Requested));
        set => _requested = value;
    }
    public TimeSpan Duration
    {
        get => _duration ?? throw new ArgumentNullException(nameof(Duration));
        set => _duration = value;
    }
    public IDictionary<string, object> ExtraParameters { get; }

    public List<Func<CarbonAwareParameters, (string key, string message)?>> Validators { get; } = new List<Func<CarbonAwareParameters, (string key, string message)?>>();
    public List<Specification<CarbonAwareParameters>> Specifications { get; } = new List<Specification<CarbonAwareParameters>>();

    // User-facing display names for the parameters.
    public string SingleLocationDisplayName { get; set; } = "location";
    public string MultipleLocationsDisplayName { get; set; }= "locations";
    public string StartDisplayName { get; set; } = "start";
    public string EndDisplayName { get; set; } = "end";
    public string RequestedDisplayName { get; set; } = "requested";
    public string DurationDisplayName { get; set; } = "duration";

    // Accessors with defaults 
    public DateTimeOffset StartOrDefault(DateTimeOffset defaultStart) => _start ?? defaultStart;
    public DateTimeOffset EndOrDefault(DateTimeOffset defaultEnd) => _end ?? defaultEnd;
    public DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested) => _requested ?? defaultRequested;
    public TimeSpan DurationOrDefault(TimeSpan defaultDuration) => _duration ?? defaultDuration;

    public CarbonAwareParameters(
        Location? singleLocation = null,
        IEnumerable<Location>? multipleLocations = null,
        DateTimeOffset? start = null,
        DateTimeOffset? end = null,
        DateTimeOffset? requested = null,
        TimeSpan? duration = null,
        IDictionary<string, object>? extraParameters = null
    )
    {
        _singleLocation = singleLocation;
        _multipleLocations = multipleLocations;
        _start = start;
        _end = end;
        _requested = requested;
        _duration = duration;
        ExtraParameters = extraParameters ?? new Dictionary<string, object>();
    }

    public void Validate(
        bool singleLocationRequired = false,
        bool multipleLocationsRequired = false,
        bool startRequired = false,
        bool endRequired = false,
        bool requestedRequired = false,
        bool durationRequired = false
    )
    {
        var errors = new Dictionary<string, List<string>>();

        // Use arguments to define validation rules.
        if (singleLocationRequired && _singleLocation == null)
        {
            errors.AppendValue(SingleLocationDisplayName, $"{SingleLocationDisplayName} is required");
        }
        if (multipleLocationsRequired && (_multipleLocations == null || !_multipleLocations.Any()))
        {
            errors.AppendValue(MultipleLocationsDisplayName, $"{MultipleLocationsDisplayName} is required");
        }
        if (startRequired && _start == null)
        {
            errors.AppendValue(StartDisplayName, $"{StartDisplayName} is required");
        }
        if (endRequired && _end == null)
        {
            errors.AppendValue(EndDisplayName, $"{EndDisplayName} is required");
        }
        if (requestedRequired && _requested == null)
        {
            errors.AppendValue(RequestedDisplayName, $"{RequestedDisplayName} is required");
        }
        if (durationRequired && _duration == null)
        {
            errors.AppendValue(DurationDisplayName, $"{DurationDisplayName} is required");
        }

        // Loop through registered validators and run them.
        foreach (var validator in Validators)
        {
            if (validator(this) is (string key, string message))
            {
                errors.AppendValue(key, message);
            }
        }

        // Loop through registered specifications and run them.
        foreach (var spec in Specifications)
        {
            if (!spec.IsSatisfiedBy(this))
            {
                errors.AppendValue(spec.DisplayName, spec.Message);
            }
        }

        // Throw an exception if there are any errors.
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

    // Lambda expression for validating start before end (for use with Validators List).
    public static Func<CarbonAwareParameters, (string key, string message)?> StartBeforeEndValidator
    {
        get
        {
            return (p) =>
            {
                if (p._start != null && p._end != null && p._start >= p._end)
                {
                    return (p.StartDisplayName, $"CarbonAwareParameters.StartBeforeEndValidator#{p.StartDisplayName}: '{p._start}' must be before {p.EndDisplayName}: '{p._end}'");
                }
                return null;
            };
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

///////////////////////////////////////////////
// Exploring different approaches to validation.
///////////////////////////////////////////////


// Static validation class w/ static validation method.
public static class CarbonAwareParametersValidator
{
    public static (string key, string message)? ValidateStartBeforeEnd(CarbonAwareParameters p)
    {
        if (p._start != null && p._end != null && p._start >= p._end)
        {
            return (p.StartDisplayName, $"CarbonAwareParametersValidator#{p.StartDisplayName}: '{p._start}' must be before {p.EndDisplayName}: '{p._end}'");
        }
        return null;
    }
}

// Specification pattern validation classes
public abstract class Specification<T> {

    public abstract Func<T, bool> ValidateSpecification();
    public abstract Func<T, (string key, string message)> GetDetails();
    public string DisplayName { get; set; } = "";
    public string Message { get; set; } = "";

    public bool IsSatisfiedBy(T entity) {
        Func<T, bool> predicate = ValidateSpecification();
        Func<T, (string key, string message)> details = GetDetails();
        var result = predicate(entity);
        if (!result)
        {
            (DisplayName, Message) = details(entity);
        }
        return result;
    }
}

// Single use specification.
public class StartBeforeEnd : Specification<CarbonAwareParameters>
{
    public override Func<CarbonAwareParameters, (string key, string message)> GetDetails()
    {
        return p => (p.StartDisplayName, $"Specification<CarbonAwareParameters>#{p.StartDisplayName}: '{p._start}' must be before {p.EndDisplayName}: '{p._end}'");
    }

    public override Func<CarbonAwareParameters, bool> ValidateSpecification()
    {
        return p => p._start == null || p._end == null || p._start < p._end;
    }
}

// Dynamic property specification.
public class PropertyIsSet : Specification<CarbonAwareParameters>
{
    private readonly string _valueMemberName;
    private readonly string _displayNameGetMethodName;
    private readonly FieldInfo _valueField;
    private readonly MethodInfo _displayNameGetMethod;


    private readonly Type t = typeof(CarbonAwareParameters);

    public PropertyIsSet(string propertyName)
    {
        _valueMemberName = $"_{Char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
        _displayNameGetMethodName = $"get_{propertyName}DisplayName";
        _valueField = t.GetField(_valueMemberName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentNullException($"{_valueMemberName} is not a property of {t.Name}");
        _displayNameGetMethod = t.GetMethod(_displayNameGetMethodName) ?? throw new ArgumentNullException($"{_displayNameGetMethodName} is not a property of {t.Name}");
    }
    public override Func<CarbonAwareParameters, (string key, string message)> GetDetails()
    {
        return (p) => 
        {
            var displayName = _displayNameGetMethod.Invoke(p, null)?.ToString() ?? "";
            return (displayName, $"{displayName} is required.");
        };
    }

    public override Func<CarbonAwareParameters, bool> ValidateSpecification()
    {
        return p => _valueField.GetValue(p) != null;
    }
}