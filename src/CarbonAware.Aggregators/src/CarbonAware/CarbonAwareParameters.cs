using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareParametersBaseDTO
{
    virtual public string[]? MultipleLocations { get; set; }
    virtual public string? SingleLocation { get; set; }
    virtual public DateTimeOffset? Start { get; set; }
    virtual public DateTimeOffset? End { get; set; }
    virtual public int? Duration { get; set; }
    virtual public DateTimeOffset? Requested { get; set; }

    public Dictionary<string, string> GetDisplayNameMap()
    {
        var mapping = new Dictionary<string, string>();
        foreach (var DTOproperty in this.GetType().GetProperties())
        {
            var customAttributes = Attribute.GetCustomAttributes(DTOproperty, true);
            foreach (var customAttribute in customAttributes)
            {
                string? displayName = null; 
                if (customAttribute is JsonPropertyNameAttribute jsonPropertyName){ displayName = jsonPropertyName.Name; }
                if (customAttribute is FromQueryAttribute fromQuery){ displayName = fromQuery.Name; }

                if(!string.IsNullOrWhiteSpace(displayName))
                {
                    mapping.Add(DTOproperty.Name, displayName);
                }
            }
        }
        return mapping;
    }
}

public class CarbonAwareParameters
{
    private IEnumerable<Location> _mulitpleLocations = new List<Location>();
    private Location _singleLocation = new Location();
    private DateTimeOffset _start = DateTimeOffset.MinValue;
    private DateTimeOffset _end = DateTimeOffset.MaxValue;
    private TimeSpan _duration = TimeSpan.Zero;
    private DateTimeOffset _requested = DateTimeOffset.UtcNow;

    public enum PropertyName { MultipleLocations, SingleLocation, Start, End, Duration, Requested };
    public IEnumerable<Location> MultipleLocations {
        get { return _mulitpleLocations; }
        set { 
            if (value.Any()) // Only set if non-empty
            {
                _mulitpleLocations = value; 
                _props[PropertyName.MultipleLocations].IsSet = true;
            }
        }
    }
    public Location SingleLocation
    {
        get { return _singleLocation; }
        set { _singleLocation = value; _props[PropertyName.SingleLocation].IsSet = true; }
    }
    public DateTimeOffset Start {
        get { return _start; }
        set { _start = value; _props[PropertyName.Start].IsSet = true; }
    }
    public DateTimeOffset End {
        get { return _end; }
        set { _end = value; _props[PropertyName.End].IsSet = true; }
    }
    public TimeSpan Duration
    {
        get { return _duration; }
        set { _duration = value; _props[PropertyName.Duration].IsSet = true; }
    }
    public DateTimeOffset Requested
    {
        get { return _requested; }
        set { _requested = value; _props[PropertyName.Requested].IsSet = true; }
    }

    public class Property
    {
        public PropertyName Name { get; init; }
        public bool IsSet = false;
        public bool IsRequired = false;
        public bool IsValid => (IsRequired && IsSet) || !IsRequired;
        public string DisplayName { get; set; }

        public Property(PropertyName name)
        {
            Name = name;
            DisplayName = name.ToString();
        }
    }

    internal Dictionary<PropertyName, Property> _props { get; init; }

    public CarbonAwareParameters(Dictionary<string, string>? displayNameMap = null)
    {
        _props = InitProperties();
        if (displayNameMap is not null) { _props.UpdateDisplayNames(displayNameMap); }
    }

    /// <summary>
    /// Validates the properties and relationships between properties. Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply returns void. 
    /// </summary>
    /// <remarks> Validation includes two checks.
    ///  - Check that required properties are set
    ///  - Check that specified relationships between properties (like start < end) are true
    ///  If any validation errors are found during property validation, with throw without validating property relationships
    /// </remarks>
    public void Validate()
    {
        // Validate Properties
        var errors = new Dictionary<string, List<string>>(); 
        foreach(var propertyName in GetPropertyNames()) 
        {
            var property = _props[propertyName];
            if (!property.IsValid) { errors.AppendValue(property.DisplayName, $"{property.DisplayName} is not set"); }
        }

        // Assert no property validation errors before validating relationships. Throws if any errors.
        AssertNoErrors(errors);
        
        // Validate Relationships
        var start = _props[PropertyName.Start];
        var end = _props[PropertyName.End];
        if (Start >= End) { errors.AppendValue(start.DisplayName, $"{start.DisplayName} must be before {end.DisplayName}"); }

        // Assert no relationship validation errors. Throws if any errors.
        AssertNoErrors(errors);
    }

    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property as required for validation.
    /// </summary>
    /// <param name="requiredProperties"></param>
    public void SetRequiredProperties(params PropertyName[] requiredProperties) {
        foreach (var propertyName in requiredProperties) {
            _props[propertyName].IsRequired = true;
        }
    }

    /// <summary>
    /// Get the Start property or default if not set
    /// </summary>
    /// <param name="defaultValue"> Default start value if not set</param>
    public DateTimeOffset GetStartOrDefault(DateTimeOffset defaultValue)
    {
        return _props[PropertyName.Start].IsSet ? Start : defaultValue;
    }

    /// <summary>
    /// Get the End property or default if not set
    /// </summary>
    /// <param name="defaultValue"> Default start value if not set</param>
    public DateTimeOffset GetEndOrDefault(DateTimeOffset defaultValue)
    {
        return _props[PropertyName.End].IsSet ? End : defaultValue;
    }

    /// <summary>
    /// Implicit converter between a CarbonAwareParametersBaseDTO instance and a CarbonAwareParameters instance
    /// </summary>
    /// <param name="p"></param>
    public static implicit operator CarbonAwareParameters(CarbonAwareParametersBaseDTO p)
    {
        var parameters = new CarbonAwareParameters(p.GetDisplayNameMap());

        var multipleLocations = MultipleLocationsFromStrings(p.MultipleLocations);
        var singleLocation = SingleLocationFromString(p.SingleLocation);

        if (multipleLocations is not null) { parameters.MultipleLocations = multipleLocations; }
        if (singleLocation is not null) { parameters.SingleLocation = singleLocation; }
        if (p.Start.HasValue) { parameters.Start = p.Start.Value; }
        if (p.End.HasValue) { parameters.End = p.End.Value; }
        if (p.Duration.HasValue) { parameters.Duration = TimeSpan.FromMinutes(p.Duration.Value); }
        if (p.Requested.HasValue) { parameters.Requested = p.Requested.Value; }

        return parameters;
    }

    /// <summary>
    /// Get an enumerable of all the PropertyNames defined in the CarbonAwareParameters class
    /// </summary>
    public static IEnumerable<PropertyName> GetPropertyNames()
    {
        return Enum.GetValues<PropertyName>().Cast<PropertyName>();
    }

    /// <summary>
    /// Convert an array of string locations into an enumerable of Location objects. 
    /// </summary>
    /// <param name="locations">Array of string locations.</param>
    /// <remarks>Skips conversion for any values that are empty/null.</remarks>
    private static IEnumerable<Location>? MultipleLocationsFromStrings(string[]? locations)
    {
        if (locations is null) { return null; }
        return locations.Where(location => !String.IsNullOrEmpty(location)).Select(location => new Location() { RegionName = location });
    }

    /// <summary>
    /// Converts a string location into a Location object. 
    /// </summary>
    /// <param name="locations">String location.</param>
    /// <remarks>Skips conversion for empty/null strings.</remarks>
    private static Location? SingleLocationFromString(string? location)
    {
        if (String.IsNullOrEmpty(location)) { return null; }
        return new Location() { RegionName = location };
    }

    /// <summary>
    /// Asserts there are no errors or throws ArgumentException.
    /// </summary>
    /// <param name="errors"> Dictionary of errors mapping the name of the parameter that caused the error to any associated error messages.</param>
    /// <remarks>All errors packed into a single ArgumentException with corresponding Data entries.</remarks>
    private static void AssertNoErrors(Dictionary<string, List<string>> errors)
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

    /// <summary>
    /// Creates a new internal properties dictionary with entries for each PropertyName and its Property.
    /// </summary>
    private static Dictionary<PropertyName, Property> InitProperties()
    {
        var properties = new Dictionary<PropertyName, Property>();
        foreach (PropertyName name in GetPropertyNames())
        {
            properties[name] = new Property(name);
        }
        return properties;
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

    public static void UpdateDisplayNames(this Dictionary<CarbonAwareParameters.PropertyName, CarbonAwareParameters.Property> props, Dictionary<string, string> displayNameMap)
    {
        foreach ((var propertyName, var property) in props)
        {
            string? newDisplayName;
            if (displayNameMap.TryGetValue(propertyName.ToString(), out newDisplayName))
            {
                property.DisplayName = newDisplayName;
            }
        }
    }
}