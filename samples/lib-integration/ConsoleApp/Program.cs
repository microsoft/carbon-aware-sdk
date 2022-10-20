using GSF.CarbonIntensity.Configuration;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, ConsoleApp Emissions Sample!");

var configuration = new ConfigurationBuilder()
        .Build();

var serviceCollection = new ServiceCollection();
var serviceProvider = serviceCollection.AddLogging()
    .AddEmissionsServices(configuration)
    .BuildServiceProvider();
var handler = serviceProvider.GetRequiredService<IEmissionsHandler>();

const string startDate = "2022-03-01T15:30:00Z";
const string endDate = "2022-03-01T18:30:00Z";
const string location = "eastus";

var result = await handler.GetAverageCarbonIntensity(location, DateTimeOffset.Parse(startDate), DateTimeOffset.Parse(endDate));
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Average Emissions Rating is: {result}.");
