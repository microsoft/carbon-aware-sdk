using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.Model;
using Moq;
using NUnit.Framework;
using System.Text.Json;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
public class EmissionsCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsAggregatorAndWritesResults()
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, "emissions");
        var expectedLocationJSON = "\"Location\":\"eastus\"";
        var expectedTimeJSON = "\"Time\":\"2022-01-01T00:00:00+00:00\"";
        var expectedDurationJSON = "\"Duration\":\"01:00:00\"";
        var expectedRatingJSON = "\"Rating\":100.7";

        var emissionsData = JsonSerializer.Deserialize<EmissionsData>(
            "{" + $"{expectedLocationJSON},{expectedTimeJSON},{expectedDurationJSON},{expectedRatingJSON}" + "}")!;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync(new List<EmissionsData>() { emissionsData });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        StringAssert.Contains(expectedLocationJSON, _console.Out.ToString());
        StringAssert.Contains(expectedTimeJSON, _console.Out.ToString());
        StringAssert.Contains(expectedDurationJSON, _console.Out.ToString());
        StringAssert.Contains(expectedRatingJSON, _console.Out.ToString());
        _mockCarbonAwareAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
    }

    [Test]
    public async Task Run_CallsAggregatorWithLocationOptions()
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var longAliasLocation = "eastus";
        var shortAliasLocation = "westus";
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions --location {longAliasLocation} -l {shortAliasLocation}");
        var expectedLocations = new List<string>() { longAliasLocation, shortAliasLocation };
        IEnumerable<string> actualLocations = Array.Empty<string>();

        _mockCarbonAwareAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualLocations = _parameters.MultipleLocations.Select(l => l.DisplayName);
             });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        CollectionAssert.AreEquivalent(expectedLocations, actualLocations);
    }

    [TestCase("--start-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: long alias")]
    [TestCase("-s", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run StartTimeOption: short alias")]
    public async Task Run_CallsAggregatorWithStartTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualStartTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualStartTime = _parameters.Start;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualStartTime);
    }

    [TestCase("--end-time", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: long alias")]
    [TestCase("-e", "2022-01-02T03:04:05Z", TestName = "EmissionsCommandTests.Run EndTimeOption: short alias")]
    public async Task Run_CallsAggregatorWithEndTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsCommand = new EmissionsCommand();
        var invocationContext = SetupInvocationContext(emissionsCommand, $"emissions {alias} {optionValue}");
        var expectedEndTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualEndTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualEndTime = _parameters.End;
            });

        // Act
        await emissionsCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedEndTime, actualEndTime);
    }
}