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
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregatorWithData(data).Object);
        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput input = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };
        var ar1 = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;
        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.OK);

        var expected = new SciScore() { MarginalCarbonEmissionsValue = 0.7 };
        Assert.AreEqual(ar1.Value, expected);
    }

    /// <summary>
    /// Tests that exception thrown by plugin results in action with BadRequest status
    /// </summary> location 
    [Test]
    public async Task ExceptionReturnsBadRequest_MarginalCarbonIntensity()
    {
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregatorWithException().Object);

        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput input = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };

        IActionResult ar1 = await controller.GetCarbonIntensityAsync(input);

        // Assert
        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.BadRequest);
    }

}
