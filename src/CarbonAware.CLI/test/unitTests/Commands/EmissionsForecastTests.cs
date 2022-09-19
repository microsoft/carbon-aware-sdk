using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.Model;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CarbonAware.CLI.UnitTests;

[TestFixture]
public class EmissionsForecastsCommandTests : TestBase
{
    [Test]
    public async Task Run_CallsAggregatorWithLocationOptions()
    {
        // Arrange
        var forecastCommand = new EmissionsForecastCommand();
        var longAliasLocation = "eastus";
        var shortAliasLocation = "westus";
        var invocationContext = SetupInvocationContext(forecastCommand, $"emissions-forecast --location {longAliasLocation} -l {shortAliasLocation}");
        var expectedLocations = new List<string>() { longAliasLocation, shortAliasLocation };
        IEnumerable<string> actualLocations = Array.Empty<string>();

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualLocations = _parameters.MultipleLocations.Select(l => l.DisplayName);
            });

        // Act
        await forecastCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        CollectionAssert.AreEquivalent(expectedLocations, actualLocations);
    }

    [TestCase("--data-start-time", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run DataStartTimeOption: long alias")]
    [TestCase("-s", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run DataStartTimeOption: short alias")]
    public async Task Run_CallsCurrentForecast_WithStartTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastCommand = new EmissionsForecastCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastCommand, $"emissions-forecast {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualStartTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualStartTime = _parameters.Start;
            });
        // Act
        await emissionsForecastCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualStartTime);
    }

    [TestCase("--data-end-time", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run DataEndTimeOption: long alias")]
    [TestCase("-e", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run DataEndTimeOption: short alias")]
    public async Task Run_CallsCurrentForecast_WithEndTimeOptions(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastCommand = new EmissionsForecastCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastCommand, $"emissions-forecast {alias} {optionValue}");
        var expectedStartTime = DateTimeOffset.Parse(optionValue);
        DateTimeOffset actualEndTime = default;

        _mockCarbonAwareAggregator.Setup(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters _parameters) => {
                actualEndTime = _parameters.End;
            });
        // Act
        await emissionsForecastCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
        Assert.AreEqual(expectedStartTime, actualEndTime);
    }

    [TestCase("--data-requested-at", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run RequestedAt: long alias")]
    [TestCase("-r", "2022-01-02T03:04:05Z", TestName = "EmissionsForecastCommandTests.Run RequestedAt: short alias")]
    public async Task Run_CallsHistoricForecast_WhenRequestedAtProvided(string alias, string optionValue)
    {
        // Arrange
        var emissionsForecastCommand = new EmissionsForecastCommand();
        var invocationContext = SetupInvocationContext(emissionsForecastCommand, $"emissions-forecast -l eastus {alias} {optionValue}");
        var emissionData = new EmissionsData()
        {
            Location = "useast",
            Rating = 0.9,
            Time = DateTime.Now
        };
        var emissions = new List<EmissionsData>()
        {
            emissionData
        };
        EmissionsForecast expectedForecast = new()
        {
            ForecastData = emissions,
            OptimalDataPoint = emissionData
        };
        _mockCarbonAwareAggregator.Setup(agg => agg.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ReturnsAsync(expectedForecast);

        // Act
        await emissionsForecastCommand.Run(invocationContext);

        // Assert
        _mockCarbonAwareAggregator.Verify(agg => agg.GetForecastDataAsync(It.IsAny<CarbonAwareParameters>()), Times.Once);
    }
}