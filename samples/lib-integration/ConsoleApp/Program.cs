﻿using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using GSF.CarbonIntensity.Configuration;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, ConsoleAppSample!");

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

var param = new CarbonAwareParameters {
    SingleLocation = new Location {
        RegionName = location    
    },
    Start = DateTimeOffset.Parse(startDate),
    End = DateTimeOffset.Parse(endDate)
};

var result = await handler.GetAverageCarbonIntensity(param);
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Average Emissions Rating is: {result}.");
