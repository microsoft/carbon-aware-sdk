using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Text.Json;

namespace CarbonAware.CLI.CommandKeywords.Emissions;

public static class EmissionsRootCommand
{ 
    public static void AddEmissionsCommands(ref RootCommand rootCommandBase, ICarbonAwareAggregator aggregator)
    {
        var emissionsCommand = new Command("emissions", "Main Command for all Emissions related subcommands.");
        rootCommandBase.Add(emissionsCommand);


        emissionsCommand.AddEmissionsObservedCommand(aggregator);

        // Just a placeholder to show extensibility
        //emissionsCommand.AddEmissionsForecastCommand(aggregator);
    }

}