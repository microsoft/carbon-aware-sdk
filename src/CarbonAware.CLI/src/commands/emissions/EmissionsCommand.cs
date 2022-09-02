using CarbonAware.CLI.Common;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace CarbonAware.CLI.Commands;

class EmissionsCommand : Command
{
    private static readonly Option _startTimeOption = new Option<string>("--start-time", "The start time of the emissions query");
    private static readonly Command _observedCommand = new ObservedCommand();

    public EmissionsCommand() : base("emissions", "emissions keyword")
    {
        Add(_observedCommand);
        Add(_startTimeOption);
        this.SetHandler(this.Run);
    }

    private void Run(InvocationContext context)
    {
        var startTime = context.ParseResult.GetValueForOption(_startTimeOption);
        var isVerbose = context.ParseResult.GetValueForOption(CommonOptions.VerbosityOption);
        context.Console.Out.Write($"Emissions: {startTime}");
        if (isVerbose)
        {
            context.Console.WriteLine($"VERBOSITY IS ON!");

        }
        context.ExitCode = 0;
    }
}