using System.CommandLine;
using LocalizableStrings = CarbonAware.CLI.options.LocalizableStrings;


namespace CarbonAware.CLI.options
{
    internal class CommonOptions
    {
        public static readonly Option<string> LocationOption = new Option<string>(new string[] { LocalizableStrings.locationName, LocalizableStrings.locationAlias }, LocalizableStrings.locationDescription)
        {
            IsRequired = true
        };
        public static readonly Option<DateTimeOffset> startTimeOption = new Option<DateTimeOffset>(LocalizableStrings.startTimeName, LocalizableStrings.startTimeDescription);
        public static readonly Option<DateTimeOffset> endTimeOption = new Option<DateTimeOffset>(LocalizableStrings.endTimeName, LocalizableStrings.endTimeDescription);
        
        public static void AddCommonOptionsToCommand(Command command)
        {
            command.Add(LocationOption);
            command.Add(startTimeOption);
        }
    }
}
