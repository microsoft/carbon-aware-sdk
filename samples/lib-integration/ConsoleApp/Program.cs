using GSF.CarbonIntensity.Configuration;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, ConsoleApp Emissions Sample!");

var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

var serviceCollection = new ServiceCollection();
var serviceProvider = serviceCollection.AddLogging()
    .AddEmissionsServices(configuration)
    .AddForecastServices(configuration)
    .BuildServiceProvider();
var handlerEmissions = serviceProvider.GetRequiredService<IEmissionsHandler>();
var handlerForecast = serviceProvider.GetRequiredService<IForecastHandler>();

const string startDate = "2022-03-01T15:30:00Z";
const string endDate = "2022-03-01T18:30:00Z";
const string location = "eastus";

var average = await handlerEmissions.GetAverageCarbonIntensityAsync(location, DateTimeOffset.Parse(startDate), DateTimeOffset.Parse(endDate));
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Average Emissions Rating is: {average}.");

try
{
    var forecasts = await handlerForecast.GetCurrentAsync(new string[] { location });
    foreach (var forecast in forecasts)
    {
        Console.WriteLine($"Forecast GeneratedAt: {forecast.GeneratedAt} ");
        Console.WriteLine($"Forecast RequestedAt: {forecast.RequestedAt} ");
        Console.WriteLine("EmissionsDataPoints");
        Array.ForEach(forecast.EmissionsDataPoints.ToArray(), Console.WriteLine);
        Console.WriteLine("OptimalDataPoints");
        Array.ForEach(forecast.OptimalDataPoints.ToArray(), Console.WriteLine);
    }
}
catch (NotImplementedException)
{
    // Ignore
}
