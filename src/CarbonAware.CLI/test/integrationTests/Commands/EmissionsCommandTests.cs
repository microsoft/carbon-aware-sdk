using CarbonAware.DataSources.Configuration;
using NUnit.Framework;
using System.Text.Json.Nodes;

namespace CarbonAware.CLI.IntegrationTests;

/// <summary>
/// Tests that the CLI handles and packages various responses from aggregators 
/// and data sources properly, including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class EmissionsCommandTests : IntegrationTestingBase
{
    public EmissionsCommandTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public void Emissions_Help_ReturnsHelpText()
    {
        // Arrange
        var expectedAliases = new[]
        { 
            "-l", "--location",
            "-s", "--start-time",
            "-e", "--end-time",
        };
        
        // Act
        var exitCode = InvokeCLI("emissions -h");
        var output = _console.Out.ToString()!;

        // Assert
        Assert.AreEqual(0, exitCode);
        foreach (var expectedAlias in expectedAliases)
        {
            StringAssert.Contains(expectedAlias, output);
        }
    }

    [Test]
    public void Emissions_OnlyRequiredOptions_ReturnsExpectedData()
    {
        // Arrange
        var end = DateTimeOffset.UtcNow;
        var start = end.AddMinutes(-30);
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);

        // Act
        var exitCode = InvokeCLI($"emissions -l {location}");

        // Assert
        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.First()!;
        
        Assert.AreEqual(0, exitCode);
        Assert.IsNotNull(firstResult["Location"]);
        Assert.IsNotNull(firstResult["Time"]);
        Assert.IsNotNull(firstResult["Rating"]);
        Assert.IsNotNull(firstResult["Duration"]);
    }

    [Test]
    public void Emissions_StartAndEndOptions_ReturnsExpectedData()
    {
        // Arrange
        var start = DateTimeOffset.Parse("2022-09-01T00:00:00Z");
        var end = DateTimeOffset.Parse("2022-09-01T03:00:00Z");
        var location = "eastus";
        _dataSourceMocker.SetupDataMock(start, end, location);
        var expectedTime = DateTimeOffset.Parse("2022-09-01T02:00:00Z");

        // Act
        var exitCode = InvokeCLI($"emissions -l {location} -s 2022-09-01T02:00:00Z -e 2022-09-01T02:04:00Z");

        // Assert
        var jsonResults = JsonNode.Parse(_console.Out.ToString()!)!.AsArray()!;
        var firstResult = jsonResults.Last()!.AsObject();

        Assert.AreEqual(0, exitCode);
        Assert.AreEqual(expectedTime, (DateTimeOffset?)firstResult["Time"]);
    }
}
