namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using CarbonAware.WebApi.Models;
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
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetEmissions simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetEmissions simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetEmissions simulates 'location=Sydney'")]
    public async Task GetEmissionsByMultipleLocations_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(data).Object);

        IActionResult result = await controller.GetEmissionsDataForLocationsByTime(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task GetEmissionsBySingleLocation_SuccessfulCallReturnsOk()
    {
        var location = "Sydney";
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

        IActionResult result = await controller.GetEmissionsDataForLocationByTime(location);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful best emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetBestEmissions simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetBestEmissions simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetBestEmissions simulates 'location=Sydney'")]
    public async Task GetBestEmissions_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new EmissionsData()
        {
            Location = "Sydney",
            Rating = 0.9,
            Time = DateTime.Now
        };

        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithBestEmissionsData(data).Object);

        var result = await controller.GetBestEmissionsDataForLocationsByTime(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful forecast call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetForecast simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetForecast simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetForecast simulates 'location=Sydney'")]
    public async Task GetForecast_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var aggregator = CreateAggregatorWithForecastData(data);
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, aggregator.Object);

        IActionResult result = await controller.GetCurrentForecastData(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
        aggregator.Verify(a => a.GetCurrentForecastDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        // Act
        var carbonIntensityOutput = (await controller.GetAverageCarbonIntensity(location, start, end)) as ObjectResult;

        // Assert
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);
        var expectedContent = new CarbonIntensityDTO { Location = location, StartTime = start, EndTime = end, CarbonIntensity = data };
        var actualContent = (carbonIntensityOutput == null) ? string.Empty : carbonIntensityOutput.Value;
        Assert.AreEqual(expectedContent, actualContent);
    }

    /// <summary> 
    /// batch should throw error with multiple locations
    ///</summary>
    [TestCase("Sydney", "Melbourne", "2022-03-07T03:30:00", "2022-03-07T07:30:00")]
    public async Task CalculateAverageCarbonIntensityBatch_MultipleDifferentLocations(string location1, string location2, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        var request1 = new CarbonIntensityBatchDTO { Location = location1, StartTime = start, EndTime = end };
        var request2 = new CarbonIntensityBatchDTO { Location = location2, StartTime = start, EndTime = end };
        double data = 0.7;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCIBatch(data).Object);
        // Act
        var requestList = new List<CarbonIntensityBatchDTO> { request1, request2 };
        var result = (await controller.GetAverageCarbonIntensityBatch(requestList) as ObjectResult;
        // Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.BadRequest);
    }

    // batch should have values with valid input for a single or multiple locations in the batch
    [Test]
    public async Task CalculateAverageCarbonIntensityBatch_ValidInput(string location, DateTimeOffset start1, DateTimeOffset start2, DateTimeOffset end1, DateTimeOffset end2)
    {
        // Arrange
        double data1 = 0.7;
        double data2 = 0.3;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCIBatch(data1, data2).Object);

        var request1 = new CarbonIntensityBatchDTO { Location = location, StartTime = start1, EndTime = end1 };
        var request2 = new CarbonIntensityBatchDTO { Location = location, StartTime = start2, EndTime = end2 };
        var requestList = new List<CarbonIntensityBatchDTO> { request1, request2 };
        // Act
        var carbonIntensityOutput = (await controller.GetAverageCarbonIntensityBatch(requestList)) as ObjectResult;

        // Assert
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);
        var expectedContent1 = new CarbonIntensityDTO { Location = location, StartTime = start1, EndTime = end1, CarbonIntensity = data1 };
        var expectedContent2 = new CarbonIntensityDTO { Location = location, StartTime = start2, EndTime = end2, CarbonIntensity = data2 };
        var actualContent = (carbonIntensityOutput == null) ? string.Empty : carbonIntensityOutput.Value;
        var expectedContent = new List<CarbonIntensityDTO> { expectedContent1, expectedContent2 };
        Assert.AreEqual(expectedContent, actualContent);
    }


    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        string location = "Sydney";
        IActionResult singleLocationResult = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult multipleLocationsResult = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        //Assert
        TestHelpers.AssertStatusCode(singleLocationResult, HttpStatusCode.NoContent);
        TestHelpers.AssertStatusCode(multipleLocationsResult, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetBestEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        IActionResult result = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { "Sydney" });

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
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
