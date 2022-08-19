using System.Reflection;
using System.Resources;
using System.Globalization;
using System.CommandLine;
using CarbonAware.CLI.CommandKeywords;
using System.Text.Json;

namespace CarbonAware.CLI;

public class TokenBuilder
{
    static ResourceManager resourceManager = new ResourceManager("CarbonAware.CLI.CommandOptions", Assembly.GetExecutingAssembly());

    public static Option<DateTime?> CreateStartTimeOption()
    {
        String? name = resourceManager.GetString("startTime");

        var startTimeOption = new Option<DateTime?>(name!, getDefaultValue: () => null)
        {
            Description = resourceManager.GetString("startTimeDescription"),
            IsRequired = false
        };

        return startTimeOption;
    }

    public static Option<DateTime?> CreateEndTimeOption()
    {
        String? name = resourceManager.GetString("endTime");

        var endTimeOption = new Option<DateTime?>(name!, getDefaultValue: () => null)
        {
            Description = resourceManager.GetString("endTimeDescription"),
            IsRequired = false
        };

        return endTimeOption;
    }

    public static Argument<string[]> CreateLocationsArgument()
    {
        String? name = resourceManager.GetString("locations");
        String? description = resourceManager.GetString("locatiomsDescription");

        var argument = new Argument<string[]>(name: name, description);
        argument.Arity = ArgumentArity.OneOrMore;

        return argument;
    }

    public static Option<T?> GetOption<T>(string option)
    {
        var rm = new ResourceManager("CarbonAware.CLI.CommandOptions", Assembly.GetExecutingAssembly());

        String? jsonString = rm.GetString(option);
        CommandOption commandOption = JsonSerializer.Deserialize<CommandOption>(jsonString!) ?? new CommandOption();

        Option<T?> resOption = new Option<T?>(commandOption.name!, getDefaultValue: () => default(T))
        {
            Description = commandOption.description,
            IsRequired = commandOption.isRequired
        };

        return resOption;
    }

}
