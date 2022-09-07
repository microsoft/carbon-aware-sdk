using System.CommandLine;

namespace CarbonAware.CLI.Common
{
    internal class CommonOptions
    {
        public static readonly Option<string[]> RequiredLocationOption = new Option<string[]>(
            new string[] { "--location", "-l" }, 
            CommonLocalizableStrings.LocationDescription)
            {
                IsRequired = true,
                Arity = ArgumentArity.OneOrMore
            };
        public static readonly Option<DateTimeOffset?> StartTimeOption = new Option<DateTimeOffset?>(
            "--start-time", 
            CommonLocalizableStrings.StartTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
        public static readonly Option<DateTimeOffset?> EndTimeOption = new Option<DateTimeOffset?>(
            "--end-time",
            CommonLocalizableStrings.EndTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
    }
}
