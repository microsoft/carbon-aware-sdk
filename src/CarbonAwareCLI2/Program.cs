using System.CommandLine;
using System.CommandLine.Invocation;


namespace CarbonAware.CL2;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand()
        {
            Name = "carbonaware",
            Description = "Root command for retrieving data using Carbonaware SDK"
        };
        var emissionsCommand = new Command("emissions")
        {
            new Option<string>("--locations") 
            { 
                Description = "List of Locations",
                IsRequired = true 
            },
            new Option<string>(
                "--startTime",
                description: "startTime")
        };

        emissionsCommand.SetHandler(() =>
        {
            Console.WriteLine("test command");
        });

        rootCommand.AddCommand(emissionsCommand);
        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("Success");
            });

        return rootCommand.Invoke(args);
    }

    static void ReadFile(String text)
    {
        Console.WriteLine("here");
    }
}