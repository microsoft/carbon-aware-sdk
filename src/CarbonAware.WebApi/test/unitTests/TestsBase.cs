using CarbonAware.Model;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using CarbonAware.Parameters;
using static CarbonAware.Parameters.CarbonAwareParameters;

namespace CarbonAware.WepApi.UnitTests;

/// <summary>
/// TestsBase for all WebAPI specific tests.
/// </summary>
public abstract class TestsBase
{
    protected Mock<ILogger<CarbonAwareController>> MockCarbonAwareLogger { get; }
    protected TestsBase()
    {
        this.MockCarbonAwareLogger = new Mock<ILogger<CarbonAwareController>>();
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithEmissionsData(List<EmissionsData> data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x => x.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                new Validator()
                    .SetRequiredProperties(PropertyName.MultipleLocations)
                    .Validate(parameters);
            })
            .ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithBestEmissionsData(List<EmissionsData> data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                new Validator()
                    .SetRequiredProperties(PropertyName.MultipleLocations)
                    .Validate(parameters);
            })
            .ReturnsAsync(data);
        return aggregator;
    }

    protected static Mock<ICarbonAwareAggregator> CreateAggregatorWithForecastData(List<EmissionsData> data)
    {
        var forecasts = new List<EmissionsForecast>()
        {
            new EmissionsForecast(){ ForecastData = data }
        };
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                new Validator()
                    .SetRequiredProperties(PropertyName.MultipleLocations)
                    .Validate(parameters);
            })
            .ReturnsAsync(forecasts);
        return aggregator;
    }

    protected static Mock<ICarbonAwareAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                new Validator()
                    .SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End)
                    .Validate(parameters);
            })
            .ReturnsAsync(data);

        return aggregator;
    }
}
