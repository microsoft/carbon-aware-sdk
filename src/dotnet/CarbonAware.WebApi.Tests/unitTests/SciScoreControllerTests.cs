namespace CarbonAware.WepApi.UnitTests;

using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using CarbonAware.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

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
    [Test]
    public async Task SuccessfulCallReturnsOk_MarginalCarbonIntensity()
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
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);

        var expected = new SciScore() { MarginalCarbonEmissionsValue = 0.7 };
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }

    /// <summary>
    /// Tests that without location, ends up having a badRequest error
    /// </summary> location 
    [Test]
    public async Task ExceptionReturnsBadRequest_MarginalCarbonIntensity()
    {
        var data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object, this.ActivitySource);

        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "";
        SciScoreInput input = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };

        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;
        var expected = new CarbonAwareWebApiError() { Message = "TimeInterval is required" };
        // Assert
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.BadRequest);
        Assert.AreEqual(expected, carbonIntensityOutput.Value);
    }

}
