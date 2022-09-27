# Aggregators
Aggregators live in the business logic tier and are responsible for taking a consumer request and fulfilling it. They perform any necessary validation on the inputs from the consumer tier, call the necessary data source(s), and do any post-processing required before returning the result to the consumer.

## Validating Inputs
Part of the aggregator's responsibility is validating the input it receives from the consumer. This includes enforcing required fields and any validations (i.e., a list field cannot be empty, a time field cannot be in the past etc.) 

The tricky part is that each aggregator can support a wide variety of consumer requests that could potentially require access to various data sources. Therefore the input must be generic enough to support various requests, but specific enough to allow enforcement of requirements. To handle this, we have create the `Carbon Aware Parameters` class. 

## Carbon Aware Parameters
The `CarbonAwareParameters` class allows the user to pass in a unique parameter instance to an aggregator function with the specific parameters needed by that call. 
The list of allowed parameters is defined in the class and includes
- SingleLocation
- MultipleLocations
- Start
- End
- RequestedAt
- Duration

### Parameter Display Names
The display name of each parameter can be overriden using the public setter. By default, each parameter display name is set to the variable name (ex: `Start = "start"`). The parameter display names are used when creating the validation error messages. Overriding them is useful in situations where the variables the user is using for input don't exactly match the default display name of the parameter (e.g. the user variable in the controller is `periodStartTime` instead of `startTime`). That way, when the error is thrown to the user, the parameter names will match the users' expectation

### Required Properties
The first core validation the parameters class does is validating that required parameters are defined. By default, all parameters are considered optional. Calling the `SetRequiredProperties(...)` function with the desired arguments sets the required parameters for the instance.
```
    /// <summary>
    /// Set the required properties in this parameters instance. Default to not required
    /// </summary>
    public void SetRequiredProperties(
        bool singleLocation = false,
        bool multipleLocations = false,
        bool start = false,
        bool end = false,
        bool requested = false,
        bool duration = false)
```

### Validate
Calling the `Validate(...)` function will validate (1) required parameters and (2) specified relationships between parameters. Currently, the only relationship we check is whether `start` is before `end`, which can be triggered by setting that flag in the `Validate` function to `true` (defaults to `false`). 

If no errors are thrown, the funciton simply returns. If any validation errors are found, they are packaged into a single  `ArgumentException` error with each being part of the `data` dictionary.
```
    /// <summary>
    /// Validates this instance by checking the required properties are defined. Optionally validated that start property occurs before end, but that check defaults to false. Returns if validation succeeds, or throws if any validation errors are found.
    /// </summary>
    /// <param name="startBeforeEndRequired"></param>
    public void Validate(bool startBeforeEndRequired = false)
 ```

 ### Getters With Default Fallbacks
 Certain parameters have special getters that allow you to define a fallback default value if the parameter is null. This can be useful in cases where a parameter is optional, so you want to get it if it was defined by the user, or otherwise fallback to a specific default. These include `Start`, `End`, `Requested`,and `Duration`
 ```
    DateTimeOffset StartOrDefault(DateTimeOffset defaultStart)
    DateTimeOffset EndOrDefault(DateTimeOffset defaultEnd)
    DateTimeOffset RequestedOrDefault(DateTimeOffset defaultRequested)
    TimeSpan DurationOrDefault

 ```

 ### Open Questions/Ideas
 1. Process of adding new fields: is there a way to refactor the code such that the "requried" check of validation can automatically be done for all fields (insted of having to manually add the repeated code each time).
 2. Nullable parameters: Is there a way to write the class cleanly such that the parameters aren't nullable? This is because once we validate the parameters and query them, in most cases we need non-nullable instances (i.e., `DateTimeOffset` no `DateTimeOffset?`). So right now we'd need more null checking or casting.
3. Defaulting fields: going hand in hand with the above, is there a way to batch default all the fields at once? That way, they are known to either have a value (whether that be the user value or the default we perscribe for this instance) or they throw when we try to get.