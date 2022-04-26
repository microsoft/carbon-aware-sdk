namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;

/// <summary>
/// TestsBase for all WebAPI specific tests.
/// </summary>
public abstract class TestsBase
{
    protected Mock<ILogger<CarbonAwareController>> MockLogger { get; }
    protected Mock<ILogger<SciScoreController>> MockSciScoreLogger { get; }
    protected TestsBase()
    {
        this.MockLogger = new Mock<ILogger<CarbonAwareController>>();
        this.MockSciScoreLogger = new Mock<ILogger<SciScoreController>>();
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithData(List<EmissionsData> data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithException()
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x =>
            x.GetEmissionsDataAsync(
                It.IsAny<Dictionary<string, object>>())).Throws<Exception>();
        return aggregator;
    }

    // Mocks for SciScoreAggregator
    protected static Mock<ISciScoreAggregator> CreateSciScoreAggregatorWithData(double data)
    {
        var aggregator = new Mock<ISciScoreAggregator>();
        aggregator.Setup(x =>
            x.CalculateAverageCarbonIntensityAsync(It.IsAny<Location>(), It.IsAny<string>())).ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<ISciScoreAggregator> CreateSciScoreAggregatorWithException()
    {
        var aggregator = new Mock<ISciScoreAggregator>();
        aggregator.Setup(x =>
            x.CalculateAverageCarbonIntensityAsync(It.IsAny<Location>(), It.IsAny<string>())).Throws<Exception>();
        return aggregator;
    }
}
