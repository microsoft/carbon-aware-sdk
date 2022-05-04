
using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using CarbonAware.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CarbonAware.WepApi.UnitTests;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class SciScoreControllerTests : TestsBase
{

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(LocationType.Geoposition)]
    [TestCase(LocationType.CloudProvider)]
    public async Task SuccessfulCallReturnsOk_MarginalCarbonIntensity(LocationType locationType)
    {
        double data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object, this.ActivitySource);
        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput input = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };
        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;
        var expected = new SciScore() { MarginalCarbonEmissionsValue = data };

        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }

    /// <summary>
    /// Tests that invalid time inputs respond with a badRequest error
    /// </summary> location 
    [Test]
    public async Task InvalidTimeIntervalReturnsBadRequest_MarginalCarbonIntensity()
    {
        // Arrange
        var data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object, this.ActivitySource);

        LocationInput locationInput = new LocationInput()
        {
            LocationType = LocationType.Geoposition.ToString()
        };

        string timeInterval = "";

        SciScoreInput input = new SciScoreInput()
        {
            Location = locationInput,
            TimeInterval = timeInterval
        };

        // Act
        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;

        // Assert
        var expected = new CarbonAwareWebApiError() { Message = "timeInterval is required" };
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.BadRequest);
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }

    /// <summary>
    /// Tests that invalid location inputs respond with a badRequest error
    /// </summary> location 
    [Test]
    public async Task NullLocationReturnsBadRequest_MarginalCarbonIntensity()
    {
        // Arrange
        var data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object);

        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";

        SciScoreInput input = new SciScoreInput()
        {
            TimeInterval = timeInterval
        };

        // Act
        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;

        // Assert
        var expected = new CarbonAwareWebApiError() { Message = "location is required" };
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.BadRequest);
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }

    /// <summary>
    /// Tests that invalid locationType inputs respond with a badRequest error
    /// </summary> location 
    public async Task InvalidLocationTypeReturnsBadRequest_MarginalCarbonIntensity()
    {
        // Arrange
        var data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object);

        LocationInput locationInput = new LocationInput()
        {
            LocationType = "InvalidType"
        };

        string timeInterval = "";

        SciScoreInput input = new SciScoreInput()
        {
            Location = locationInput,
            TimeInterval = timeInterval
        };

        // Act
        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;

        // Assert
        var expected = new CarbonAwareWebApiError() { Message = "locationType 'InvalidType' is invalid" };
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.BadRequest);
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
