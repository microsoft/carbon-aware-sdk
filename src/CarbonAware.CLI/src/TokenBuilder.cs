using System.CommandLine;
using System.Reflection;
using System.Resources;

namespace CarbonAware.CLI;

public sealed class TokenBuilder
{
    private ResourceManager resourceManager;
    private TokenBuilder()
    {
        resourceManager = new ResourceManager("CarbonAware.CLI.CommandOptions", Assembly.GetExecutingAssembly());
    }
    private static readonly Lazy<TokenBuilder> _instance = new Lazy<TokenBuilder> (
        () => new TokenBuilder());
    public static TokenBuilder Instance
    {
        get { return _instance.Value; }
    }

    #region Commands
    public Command CreateEmissionsRootCommand()
    {
        return new Command(
            name: GetStringValueFromLibrary("CommandEmissionsRootName"),
            description: GetStringValueFromLibrary("CommandEmissionsRootDescription")
        );
    }

    public Command CreateEmissionsBestCommand()
    {
        return new Command(
            name: GetStringValueFromLibrary("CommandEmissionsBestName"),
            description: GetStringValueFromLibrary("CommandEmissionsBestDescription")
        );
    }

    public Command CreateEmissionsObservedCommand()
    {
        return new Command(
            name: GetStringValueFromLibrary("CommandEmissionsObservedName"),
            description: GetStringValueFromLibrary("CommandEmissionsObservedDescription")
        );
    }
    #endregion

    #region Options
    public Option<DateTime?> CreateStartTimeOption()
    {
        var option = new Option<DateTime?>
            (
                name: GetStringValueFromLibrary("OptionStartTimeName"),
                description: GetStringValueFromLibrary("OptionStartTimeDescription")
            );
        option.IsRequired = false;
        return option;
    }

    public Option<DateTime?> CreateEndTimeOption()
    {
        var option = new Option<DateTime?>
            (
                name: GetStringValueFromLibrary("OptionEndTimeName"),
                description: GetStringValueFromLibrary("OptionEndTimeDescription")
            );
        option.IsRequired = false;
        return option;
    }
    #endregion

    #region Arguments
    public Argument<string[]> CreateLocationsArgument()
    {
        var argument = new Argument<string[]>
            (
                name: GetStringValueFromLibrary("ArgLocationsName"),
                description: GetStringValueFromLibrary("ArgLocationsDescription")
            );
        argument.Arity = ArgumentArity.OneOrMore;

        return argument;
    }
    #endregion

    /// <summary>
    /// Loads the specified string value from the resource library.
    /// </summary>
    /// <param name="key">The key into the resource library corresponding to the desired string.</param>
    /// <returns>The corresponding string. If no string is found for the specified string, an exception will be thrown.</returns>
    private String GetStringValueFromLibrary(string key)
    {
        return resourceManager.GetString(key) ?? key;
    }

}
