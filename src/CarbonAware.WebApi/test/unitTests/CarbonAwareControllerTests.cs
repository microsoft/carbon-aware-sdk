namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using WireMock.Models;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class CarbonAwareControllerTests : TestsBase
{
    /// <summary>
    /// Tests that successful emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task GetEmissions_SuccessfulCallReturnsOk()
    {
        string location = "Sydney";
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = location,
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(data).Object);

        IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult ar2 = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.OK);
        TestHelpers.AssertStatusCode(ar2, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful best emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task GetBestEmissions_SuccessfulCallReturnsOk()
    {
        string location = "Sydney";
        var data = new EmissionsData()
        {
            Location = location,
            Rating = 0.9,
            Time = DateTime.Now
        };


        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithBestEmissionsData(data).Object);

        var ar = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { location });

        TestHelpers.AssertStatusCode(ar, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful forecast call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task GetForecast_SuccessfulCallReturnsOk()
    {
        string location = "Sydney";
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = location,
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var aggregator = CreateAggregatorWithForecastData(data);
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, aggregator.Object);

        IActionResult result = await controller.GetCurrentForecastData(new string[] { location });

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
        aggregator.Verify(a => a.GetCurrentForecastDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        string location = "Sydney";
        IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult ar2 = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { location });
        IActionResult ar3 = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        //Assert
        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.NoContent);
        TestHelpers.AssertStatusCode(ar2, HttpStatusCode.NoContent);
        TestHelpers.AssertStatusCode(ar3, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetBestEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        IActionResult ar = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { "Sydney" });

        //Assert
        TestHelpers.AssertStatusCode(ar, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "array of nulls: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "array of nulls: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "empty array: simulates no 'location' query string")]
    public void GetEmissions_NoLocations_ThrowsException(params string[] locations)
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetBestEmissionsDataForLocationsByTime(locations));
        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetEmissionsDataForLocationsByTime(locations));
        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetCurrentForecastData(locations));
    }
}
