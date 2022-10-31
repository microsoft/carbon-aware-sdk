using static CarbonAware.Aggregators.CarbonAware.Parameters.Validator;

namespace CarbonAware.Aggregators.CarbonAware.Parameters;
/// <summary>
/// Single class builder that does field validation real-time as users try to set it based on instantiated ParameterType
/// </summary>
public class Builder
{
    public enum ParameterType { GenericParameters, EmissionsParameters, CurrentForecastParameters, ForecastParameters, CarbonIntensityParameters }
    private readonly CarbonAwareParametersBaseDTO baseParameters;
    private readonly ParameterType parameterType;

    public Builder(ParameterType? type = null, CarbonAwareParametersBaseDTO? parameters = null)
    {
        baseParameters = parameters ?? new CarbonAwareParametersBaseDTO();
        parameterType = type ?? ParameterType.GenericParameters;
    }

    public CarbonAwareParameters Build()
    {
        GetValidator(parameterType)
            .Validate(baseParameters);
        return baseParameters;
    }

    public Builder AddStartTime(DateTimeOffset start)
    {
        baseParameters.Start = start;
        return this;
    }
    public Builder AddEndTime(DateTimeOffset end)
    {
        baseParameters.End = end;
        return this;
    }
    public Builder AddLocation(string location)
    {
        AddLocations(new string[] { location });
        return this;
    }

    public Builder AddLocations(string[] locations)
    {
        switch (parameterType)
        {
            case ParameterType.EmissionsParameters:
            case ParameterType.CurrentForecastParameters:
            case ParameterType.GenericParameters:
                {
                    if (locations.Any())
                    {
                        baseParameters.MultipleLocations = locations;
                    }
                    break;
                }
            case ParameterType.ForecastParameters:
            case ParameterType.CarbonIntensityParameters:
                {
                    if (locations.Any() && locations.Length == 1)
                        {
                            baseParameters.SingleLocation = locations[0];
                        }
                    break;
                }
        }
        return this;
    }

    public Builder AddDuration(int duration)
    {
        baseParameters.Duration = duration;
        return this;
    }

    public static Validator GetValidator(ParameterType type)
    {
        return type switch
        {
            ParameterType.EmissionsParameters => EmissionsValidator(),
            ParameterType.CurrentForecastParameters => CurrentForecastValidator(),
            ParameterType.ForecastParameters => ForecastValidator(),
            ParameterType.CarbonIntensityParameters => CarbonIntensityValidator(),
            ParameterType.GenericParameters => new Validator(),
            _ => new Validator(),
        };
    }
}