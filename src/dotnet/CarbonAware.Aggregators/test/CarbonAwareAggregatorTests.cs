using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using CarbonAware.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CarbonAware.Aggregators.Tests;

public class CarbonAwareAggregatorTests
{

    [Test]
    public async Task Test_Emissions_Averagage()
    {
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockPlugin = new Mock<ICarbonAware>();

        mockPlugin.Setup(x => x.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(FakeEmissionData());
        
        var aggregator = new CarbonAwareAggregator(logger, mockPlugin.Object);

        var average = await aggregator.GetEmissionsAveragageAsync(new Dictionary<string, object?>());
        Assert.NotZero(average);
    }

    private List<EmissionsData> FakeEmissionData() {
        return new List<EmissionsData>() {
                new EmissionsData {
                    Location = "eastus",
                    Time = DateTime.Parse("2021-09-01"),
                    Rating = 23
                },
                new EmissionsData {
                    Location = "westus",
                    Time = DateTime.Parse("2021-12-01"),
                    Rating = 15
                },
                new EmissionsData {
                    Location = "eastus",
                    Time = DateTime.Parse("2022-02-01"),
                    Rating = 20
                },
                new EmissionsData {
                    Location = "midwest",
                    Time = DateTime.Parse("2021-05-01"),
                    Rating = 25
                }
            };
    }
}
