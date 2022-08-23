using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Text.Json;

namespace CarbonAware.CLI.CommandKeywords.Emissions;

public static class EmissionsRootCommand
{ 
    public static void AddEmissionsCommands(ref RootCommand rootCommandBase, ICarbonAwareAggregator aggregator)
    {
        var emissionsCommand = TokenBuilder.Instance.CreateEmissionsRootCommand();
        rootCommandBase.Add(emissionsCommand);

        emissionsCommand.AddEmissionsObservedCommand(aggregator);
        emissionsCommand.AddBestEmissionsCommand(aggregator);
    }

}