using CarbonAware.Model;

namespace CarbonAware.Aggregators.CarbonAware;

public class Property<T>
{
    private T? _value;
    private bool _isSet = false;
    public bool IsRequired = false;
    public bool IsValid => (IsRequired && _isSet) || !IsRequired;
    public string DisplayName;
    public virtual T Value
    {
        get {
            if (IsValid)
            {
                return _value ?? throw new NullReferenceException("Value is null");
            } else {
                throw new InvalidOperationException("Required property is not set");
            }
        }
        set { _value = value; _isSet = true; }
    }

    public Property(string displayName)
    {
        DisplayName = displayName;
    }
}

public class DefaultableProperty<T> : Property<T>
{
    public T DefaultValue;

    public override T Value
    {
        get {
            try
            {
                return base.Value;
            } catch (NullReferenceException)
            {
                return DefaultValue;
            }
        }
        set => base.Value = value;
    }
    public DefaultableProperty(string displayName, T defaultValue) : base(displayName)
    {
        DefaultValue = defaultValue;
    }
}

public class CarbonAwareProperties
{
    public Property<IEnumerable<Location>> MultipleLocations;
    public DefaultableProperty<DateTimeOffset> Start;
    public DefaultableProperty<DateTimeOffset> End;

    public CarbonAwareProperties(Property<IEnumerable<Location>> multipleLocations, DefaultableProperty<DateTimeOffset> start, DefaultableProperty<DateTimeOffset> end)
    {
        MultipleLocations = multipleLocations;
        Start = start;
        End = end;
    }
}

public class CarbonAwareParameters
{
    private Property<IEnumerable<Location>> _multipleLocations = new Property<IEnumerable<Location>>("locations");
    private DefaultableProperty<DateTimeOffset> _start = new DefaultableProperty<DateTimeOffset>("start", DateTimeOffset.MinValue);
    private DefaultableProperty<DateTimeOffset> _end = new DefaultableProperty<DateTimeOffset>("end", DateTimeOffset.MaxValue);

    public CarbonAwareProperties Props { get; init; }

    public IEnumerable<Location> MultipleLocations
    {
        get => Props.MultipleLocations.Value;
        set { if (value.Any()) { Props.MultipleLocations.Value = value; } }
    }
    public DateTimeOffset Start
    {
        get => Props.Start.Value;
        set => Props.Start.Value = value;
    }
    public DateTimeOffset End
    {
        get => Props.End.Value;
        set => Props.End.Value = value;
    }
    public CarbonAwareParameters()
    {
        Props = new CarbonAwareProperties(_multipleLocations, _start, _end);
    }

    public void Validate(bool startBeforeEndRequired = false)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validate Properties
        if (!Props.MultipleLocations.IsValid) { errors.AppendValue(Props.MultipleLocations.DisplayName, $"{Props.MultipleLocations.DisplayName} is not set"); }
        if (!Props.Start.IsValid) { errors.AppendValue(Props.Start.DisplayName, $"{Props.Start.DisplayName} is not set"); }
        if (!Props.End.IsValid) { errors.AppendValue(Props.End.DisplayName, $"{Props.End.DisplayName} is not set"); }

        CheckErrors(errors);

        // Validate Relationships
        if (startBeforeEndRequired)
        {
            if (Start > End)
            {
                errors.AppendValue(Props.Start.DisplayName, $"{Props.Start.DisplayName} must be before {Props.End.DisplayName}");
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