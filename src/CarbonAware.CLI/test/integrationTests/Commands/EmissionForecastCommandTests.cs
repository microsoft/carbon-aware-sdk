using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Text.Json.Nodes;

namespace CarbonAware.CLI.IntegrationTests;

/// <summary>
/// Tests that the CLI handles and packages various responses from aggregators 
/// and data sources properly, including empty responses and exceptions.
/// </summary>
//[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class EmissionForecastCommandTests : IntegrationTestingBase
{
    public EmissionForecastCommandTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task Emissions_Help_ReturnsHelpText()
    {
        // Arrange
        var expectedAliases = new[]
        {
            "-l", "--location",
            "-s", "--data-start-time",
            "-e", "--data-end-time",
            "-r", "--data-requested-at",
            "-w", "--window-size"
        };

        // Act
        var exitCode = await InvokeCliAsync("emissions-forecast -h");
        var output = _console.Out.ToString()!;

        // Assert
        Assert.AreEqual(0, exitCode);
        foreach (var expectedAlias in expectedAliases)
        {
            StringAssert.Contains(expectedAlias, output);
        }
    }

    [Test]
    public async Task EmissionsForecast_OnlyRequiredOptions_ReturnsExpectedData()
    {
        // Arrange
        var location = "eastus";

        _dataSourceMocker.SetupForecastMock();

        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecast -l {location}");
        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!;
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
    }

    [Test]
    public async Task EmissionsForecast_StartAndEndOptions_ReturnsExpectedData()
    {
        // Arrange
        var location = "eastus";
        var end = DateTimeOffset.UtcNow;
        var start = end - TimeSpan.FromHours(5.0);

        _dataSourceMocker.SetupForecastMock();

        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecast -l {location} ");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
    }

    [Test]
    public async Task EmissionsForecast_RequestedAtOptions_ReturnsExpectedData()
    {
        // Arrange
        _dataSourceMocker.SetupBatchForecastMock();

        // Act
        var exitCode = await InvokeCliAsync($"emissions-forecast -l eastus -r 2022-09-01");

        // Assert
        Assert.AreEqual(0, exitCode);

        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!.AsObject();
        Assert.AreEqual(1, jsonResults.Count);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["GeneratedAt"]);
        Assert.IsNotNull(firstResult["DataStartAt"]);
        Assert.IsNotNull(firstResult["DataEndAt"]);
        Assert.IsNotNull(firstResult["RequestedAt"]);
    }
}
