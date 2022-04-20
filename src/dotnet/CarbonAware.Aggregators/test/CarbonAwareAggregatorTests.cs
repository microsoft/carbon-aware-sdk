using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using CarbonAware.Plugins;
using Microsoft.Extensions.Logging;
using System.Linq;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Aggregators.Configuration;

namespace CarbonAware.Aggregators.Tests;

[TestFixture]
public class CarbonAwareAggregatorTests
{

    [TestCase("westus", "2021-11-17", "2021-11-20", ExpectedResult = 25)]
    [TestCase("eastus", "2021-11-17", "2021-12-20", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17", "2021-11-18", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19", "2021-12-30", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18", "2021-12-30", ExpectedResult = 0)]
    public async Task<double> Test_Emissions_Average_FakeData(string location, string startTime, string endTime)
    {
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockPlugin = new Mock<ICarbonAware>();

        DateTime sTime, eTime;
        Assert.True(DateTime.TryParse(startTime, out sTime));
        Assert.True(DateTime.TryParse(endTime, out eTime));

        mockPlugin.Setup(x => x.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(FilterRawFakeData(location, sTime, eTime));
        
        var aggregator = new CarbonAwareAggregator(logger, mockPlugin.Object);
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<string>() { location }},
            { CarbonAwareConstants.Start, sTime },
            { CarbonAwareConstants.End, eTime }
        };
        return await aggregator.CalcEmissionsAverageAsync(props);
    }

    [TestCase("westus", "2021-11-17", "2021-11-20", 20)]
    [TestCase("eastus", "2021-12-19", "2021-12-30", 20)]
    [TestCase("fake", "2021-12-19", "2021-12-30", 0)]
    public async Task Test_Emissions_Average_With_Plugin_Association(string location, string startTime, string endTime, int expected)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); 
        serviceCollection.AddCarbonAwareEmissionServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var aggregators = serviceProvider.GetServices<ICarbonAwareAggregator>();
        Assert.NotNull(aggregators);
        Assert.IsNotEmpty(aggregators);

        DateTime sTime, eTime;
        Assert.True(DateTime.TryParse(startTime, out sTime));
        Assert.True(DateTime.TryParse(endTime, out eTime));
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<string>() { location } },
            { CarbonAwareConstants.Start, sTime },
            { CarbonAwareConstants.End, eTime}
        };
        var aggregator = aggregators.First();

        var average = await aggregator.CalcEmissionsAverageAsync(props);
        Assert.GreaterOrEqual(average, expected);
    }

    [TestCase(null, null, null)]
    [TestCase("westus", null, null)]
    [TestCase(null, "2021-12-19", null)]
    [TestCase(null, null, "2021-12-20")]
    [TestCase(null, "2021-12-19", "2021-12-20")]
    public void Test_Emissions_Average_Missing_Properties(string location, string startTime, string endTime)
    {
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockPlugin = new Mock<ICarbonAware>();

        mockPlugin.Setup(x => x.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(It.IsAny<IEnumerable<EmissionsData>>);
        
        var aggregator = new CarbonAwareAggregator(logger, mockPlugin.Object);
        var props = new Dictionary<string, object>();
        if (location != null)
        {
            props[CarbonAwareConstants.Locations] =  new List<string>() { location };
        }
        if (startTime != null)
        {
            DateTime sTime;
            Assert.True(DateTime.TryParse(startTime, out sTime));
            props[CarbonAwareConstants.Start] = sTime;
        }
        if (endTime != null)
        {
            DateTime eTime;
            Assert.True(DateTime.TryParse(endTime, out eTime));
            props[CarbonAwareConstants.End] = eTime;
        }
        Assert.ThrowsAsync<ArgumentException>(async () => await aggregator.CalcEmissionsAverageAsync(props));
    }

    private IEnumerable<EmissionsData> FilterRawFakeData(string location, DateTime startTime, DateTime endTime)
    {
        return RawFakeEmissionData.Where(x => x.Location == location && x.Time >= startTime && x.Time <= endTime);
    }

    static IEnumerable<EmissionsData> RawFakeEmissionData =  new List<EmissionsData>()
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 10
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 20
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17"),
                Rating = 30
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-19"),
                Rating = 40
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-11-18"),
                Rating = 60
            }
        };
}
