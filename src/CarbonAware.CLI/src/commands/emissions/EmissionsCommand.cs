using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace CarbonAware.CLI.Commands;

class EmissionsCommand : Command
{
    public EmissionsCommand() : base("emissions", "emissions keyword")
    {
        CommonOptions.AddAllOptionsToCommand(this);
        this.SetHandler(this.Run);
    }

    private async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var location = context.ParseResult.GetValueForOption<string>(CommonOptions.RequiredLocationOption) ?? "";
        var parameters = new CarbonAwareParametersBaseDTO() { MultipleLocations = new string[] { location } };

        // Call the aggregator.
        var results = await aggregator.GetEmissionsDataAsync(parameters);
        foreach (var result in results)
        {
            context.Console.WriteLine(result.ToString());
        }
        context.ExitCode = 0;
    }
}