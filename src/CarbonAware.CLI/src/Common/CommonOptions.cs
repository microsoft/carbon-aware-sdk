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
                Arity = ArgumentArity.OneOrMore,
                AllowMultipleArgumentsPerToken = true,
            };
        public static readonly Option<DateTimeOffset?> StartTimeOption = new Option<DateTimeOffset?>(
            "--startTime", 
            CommonLocalizableStrings.StartTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
        public static readonly Option<DateTimeOffset?> EndTimeOption = new Option<DateTimeOffset?>(
            "--endTime",
            CommonLocalizableStrings.EndTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
    }
}
