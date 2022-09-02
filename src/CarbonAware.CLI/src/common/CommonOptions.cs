using CarbonAware.CLI.common;
using System.CommandLine;


namespace CarbonAware.CLI.Common
{
    internal class CommonOptions
    {
        public static readonly Option<string> RequiredLocationOption = new Option<string>(
            new string[] { "--location", "-l" }, 
            CommonLocalizableStrings.locationDescription)
            {
                IsRequired = true
            };
        public static readonly Option<DateTimeOffset> startTimeOption = new Option<DateTimeOffset>("--startTime", CommonLocalizableStrings.startTimeDescription);
        public static readonly Option<DateTimeOffset> endTimeOption = new Option<DateTimeOffset>("--endTime", CommonLocalizableStrings.endTimeDescription);
    }
}
