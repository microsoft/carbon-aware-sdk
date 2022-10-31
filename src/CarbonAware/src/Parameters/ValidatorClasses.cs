using PropertyName = CarbonAware.Parameters.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Parameters;

public partial class Validator
{
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public enum ValidationName { 
        // Start < End
        StartBeforeEnd, 
        // if End, End && Start
        StartRequiredIfEnd };

    private readonly List<PropertyName> _requiredProperties;

    private readonly List<Func<CarbonAwareParameters, ParametersValidator>> _parameterValidations;

    public Validator()
    {
        _requiredProperties = new List<PropertyName>();
        _parameterValidations = new List<Func<CarbonAwareParameters, ParametersValidator>>();
    }

    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property as required for validation.
    /// </summary>
    /// <param name="requiredProperties"></param>
    public Validator SetRequiredProperties(params PropertyName[] requiredProperties)
    {
        _requiredProperties.AddRange(requiredProperties);
        return this;
    }

    /// <summary>
    /// Accepts any ValidationName as arguments and sets the associated validation to check.
    /// </summary>
    /// <param name="validationName"></param>
    public Validator SetValidations(params ValidationName[] validationNames)
    {
        foreach (var validationName in validationNames)
        {
            switch (validationName)
            {
                case ValidationName.StartBeforeEnd:
                    _parameterValidations.Add(StartBeforeEnd);
                    break;
                case ValidationName.StartRequiredIfEnd:
                    _parameterValidations.Add(StartRequiredIfEnd);
                    break;
            }
        }
        return this;
    }

    /// <summary>
    /// Takes in a CarbonAwarePArameters object and validates the properties and relationships between properties. Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply returns void. 
    /// </summary>
    /// <remarks> Validation includes two checks.
    ///  - Check that required properties are set
    ///  - Check that specified validations (like start < end) are true
    ///  If any validation errors are found during property validation, with throw without validating property relationships
    /// </remarks>
    public void Validate(CarbonAwareParameters parameters)
    {
        // Validate Properties
        var errors = new Dictionary<string, List<string>>();
        foreach (var propertyName in CarbonAwareParameters.GetPropertyNames())
        {
            var property = parameters._props[propertyName];
            if (_requiredProperties.Contains(propertyName)) property.IsRequired = true;
            if (!property.IsValid) { errors.AppendValue(property.DisplayName, $"{property.DisplayName} is not set"); }
        }

        // Assert no property validation errors before validating relationships. Throws if any errors.
        AssertNoErrors(errors);

        // Check parameter validations
        foreach (var parameterValidation in _parameterValidations)
        {
            var parameterValidator = parameterValidation(parameters);
            if (!parameterValidator.IsValid()) errors.AppendValue(parameterValidator.ErrorKey!, parameterValidator.ErrorMessage!);
        }

        // Assert no validation errors. Throws if any errors.
        AssertNoErrors(errors);
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
            var error = new ArgumentException("Invalid _parameters");
            foreach (KeyValuePair<string, List<string>> message in errors)
            {
                error.Data[message.Key] = message.Value.ToArray();
            }
            throw error;
        }
    }

    private class ParametersValidator
    {
        private Func<bool> _predicate { get; init; }
        private string _errorKey { get; init; }
        private string _errorMessage { get; init; }
        public ValidationName Name { get; init; }

        // Contains a value if isValid() evaluates to false
        public string? ErrorKey { get; private set; }

        // Contains a value if isValid() evaluates to false
        public string? ErrorMessage { get; private set; }

        public ParametersValidator(ValidationName name, Func<bool> predicate, string errorKey, string errorMessage)
        {
            Name = name;
            _predicate = predicate;
            _errorKey = errorKey;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Checks if the validator is valid
        /// </summary>
        /// <remarks> If result is false, will set ErrorKey and ErrorMessage property of the object. </remarks>
        public bool IsValid()
        {
            var result = _predicate();
            if (!result)
            {
                ErrorKey = _errorKey;
                ErrorMessage = _errorMessage;
            }
            return result;
        }
    }
}


