using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using CarbonAware.WebApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

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

    // protected static Mock<IEmissionsAggregator> CreateEmissionsAggregator(List<EmissionsData> data)
    // {
    //     var aggregator = new Mock<IEmissionsAggregator>();
    //     aggregator.Setup(x => x.GetEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
    //         .Callback((CarbonAwareParameters parameters) =>
    //         {
    //             parameters.SetRequiredProperties(PropertyName.MultipleLocations);
    //             parameters.Validate();
    //         })
    //         .ReturnsAsync(data);
    //     return aggregator;
    // }

    protected static Mock<IEmissionsHandler> CreateEmissionsHandler(List<EmissionsData> data)
    {
        var handler = new Mock<IEmissionsHandler>();
        handler.Setup(x => x.GetEmissionsDataAsync(It.IsAny<string>(), null, null))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return handler;
    }

    // protected static Mock<IEmissionsAggregator> CreateAggregatorWithBestEmissionsData(List<EmissionsData> data)
    // {
    //     var aggregator = new Mock<IEmissionsAggregator>();
    //     aggregator.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<CarbonAwareParameters>()))
    //         .Callback((CarbonAwareParameters parameters) =>
    //         {
    //             parameters.SetRequiredProperties(PropertyName.MultipleLocations);
    //             parameters.Validate();
    //         })
    //         .ReturnsAsync(data);
    //     return aggregator;
    // }

    protected static Mock<IEmissionsHandler> CreateHandlerWithBestEmissionsData(List<EmissionsData> data)
    {
        var handler = new Mock<IEmissionsHandler>();
        handler.Setup(x => x.GetBestEmissionsDataAsync(It.IsAny<string>(), null, null))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);
        return handler;
    }

    // protected static Mock<IForecastAggregator> CreateForecastAggregator(List<EmissionsData> data)
    // {
    //     var forecasts = new List<EmissionsForecast>()
    //     {
    //         new EmissionsForecast(){ ForecastData = data }
    //     };
    //     var aggregator = new Mock<IForecastAggregator>();
    //     aggregator.Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
    //         .Callback((CarbonAwareParameters parameters) =>
    //         {
    //             parameters.SetRequiredProperties(PropertyName.MultipleLocations);
    //             parameters.Validate();
    //         })
    //         .ReturnsAsync(forecasts);
    //     return aggregator;
    // }

    protected static Mock<IForecastHandler> CreateForecastHandler(List<EmissionsData> data)
    {
        var forecasts = new List<EmissionsForecast>()
        {
            new EmissionsForecast(){ EmissionsDataPoints = data }
        };
        var handler = new Mock<IForecastHandler>();
        handler.Setup(x => x.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(forecasts);
        return handler;
    }

    // protected static Mock<IEmissionsAggregator> CreateCarbonAwareAggregatorWithAverageCI(double data)
    // {
    //     var aggregator = new Mock<IEmissionsAggregator>();
    //     aggregator.Setup(x => x.CalculateAverageCarbonIntensityAsync(It.IsAny<CarbonAwareParameters>()))
    //         .Callback((CarbonAwareParameters parameters) =>
    //         {
    //             parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
    //             parameters.Validate();
    //         })
    //         .ReturnsAsync(data);

    //     return aggregator;
    // }

    protected static Mock<IEmissionsHandler> CreateCarbonAwareHandlerWithAverageCI(double data)
    {
        var handler = new Mock<IEmissionsHandler>();
        handler.Setup(x => x.GetAverageCarbonIntensityAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return handler;
    }
}
