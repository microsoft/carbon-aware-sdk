using static CarbonAware.Parameters.CarbonAwareParameters;

namespace CarbonAware.Parameters;

public partial class Validator {

    ///////////////////////////////
    // Public Validator Instances
    ///////////////////////////////
    public static Validator EmissionsValidator() => new Validator()
        .SetRequiredProperties(PropertyName.MultipleLocations)
        .SetValidations(ValidationName.StartRequiredIfEnd, ValidationName.StartBeforeEnd);

    public static Validator CarbonIntensityValidator() => new Validator()
        .SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End)
        .SetValidations(ValidationName.StartBeforeEnd);

    public static Validator CurrentForecastValidator() => new Validator()
        .SetRequiredProperties(PropertyName.MultipleLocations)
        .SetValidations(ValidationName.StartBeforeEnd);

    public static Validator ForecastValidator() => new Validator()
        .SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested)
        .SetValidations(ValidationName.StartBeforeEnd);

    //////////////////////////////////////////
    // Private Parameters Validator Instances
    //////////////////////////////////////////
    private ParametersValidator StartBeforeEnd(CarbonAwareParameters parameters)
    {
        return new ParametersValidator(
            ValidationName.StartBeforeEnd,
            () => parameters.Start < parameters.End,
            parameters._props[PropertyName.Start].DisplayName,
            $"{parameters._props[PropertyName.Start].DisplayName} must be before {parameters._props[PropertyName.End].DisplayName}"
        );
    }

    private ParametersValidator StartRequiredIfEnd(CarbonAwareParameters parameters)
    {
        return new ParametersValidator(
            ValidationName.StartRequiredIfEnd,
            () => !(parameters._props[PropertyName.End].IsSet && !parameters._props[PropertyName.Start].IsSet),
            parameters._props[PropertyName.Start].DisplayName,
            $"{parameters._props[PropertyName.Start].DisplayName} must be defined if {parameters._props[PropertyName.End].DisplayName} is defined"
        );
    }
}