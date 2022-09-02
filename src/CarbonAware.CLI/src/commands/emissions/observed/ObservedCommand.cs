using System.CommandLine;
using System.CommandLine.Invocation;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;

namespace CarbonAware.CLI.Commands;

class ObservedCommand : Command
{
    public static readonly Option<string> LocationOption = new Option<string>(
        new string [] { "--location", "-l"}, "The location to get the observed emissions for.");

    public ObservedCommand() : base("observed", "observed command")
    {
        Add(LocationOption);
        this.SetHandler(this.Run);
        
    }

    private async Task Run(InvocationContext context)
    {
      // Get aggregator via DI.
      var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
      var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");
      
      // Get the arguments and options to build the parameters.
      var location = context.ParseResult.GetValueForOption<string>(LocationOption) ?? "";
      var parameters = new CarbonAwareParametersBaseDTO(){ MultipleLocations = new string[]{ location }};
      
      // Call the aggregator.
      var results = await aggregator.GetEmissionsDataAsync(parameters);
      foreach (var result in results)
      {
        context.Console.WriteLine(result.ToString());
      }
      context.ExitCode = 0;
    }
}