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
    public async Task SuccessfulCallReturnsOk_sciscore()
    {
        double data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregatorWithData(data).Object);
        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput calc = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };
        IActionResult ar1 = await controller.GetCarbonIntensityAsync(calc);
        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that exception thrown by plugin results in action with BadRequest status
    /// </summary> location 
    [Test]
    public async Task ExceptionReturnsBadRequest_sciscore()
    {
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregatorWithException().Object);

        Location location = new Location() { LocationType = LocationType.Geoposition, Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput calc = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };

        IActionResult ar1 = await controller.GetCarbonIntensityAsync(calc);

        // Assert
        TestHelpers.AssertStatusCode(ar1, HttpStatusCode.BadRequest);
    }
}
