using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Common;
using System.CommandLine;

class EmissionsForecastCommand : Command
{
    private Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    private readonly Option<DateTimeOffset?> _dataStartAt = new Option<DateTimeOffset?>(
           "--data-start-at",
           LocalizableStrings.DataStartAtDescription)
           {
              IsRequired = false,
              Arity = ArgumentArity.ExactlyOne,
           };
    private readonly Option<DateTimeOffset?> _dataEndAt = new Option<DateTimeOffset?>(
           "--data-end-at",
           LocalizableStrings.DataStartAtDescription)
    {
        IsRequired = false,
        Arity = ArgumentArity.ExactlyOne,
    };
    public EmissionsForecastCommand() : base("forecasts", LocalizableStrings.EmissionsCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_dataStartAt);
        AddOption(_dataEndAt);
     //   this.SetHandler(this.Run);
    }
}