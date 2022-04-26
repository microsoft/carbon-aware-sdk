namespace CarbonAware.WepApi.UnitTests;

using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
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
        var controller = new SciScoreController(this.MockLogger.Object, CreateAggregatorWithData(data).Object);

        // IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        // TestHelpers.AssertStatusCode(ar1, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that a success call to plugin with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task EmptyResultRetunsNoContent_sciscore()
    {
        var controller = new SciScoreController(this.MockLogger.Object, CreateAggregatorWithData(new List<EmissionsData>()).Object);
        
        // IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        //Assert
    //     TestHelpers.AssertStatusCode(ar1, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that exception thrown by plugin results in action with BadRequest status
    /// </summary> location 
    [Test]
    public async Task ExceptionReturnsBadRequest_sciscore()
    {
        var controller = new SciScoreController(this.MockLogger.Object, CreateAggregatorWithException().Object);
 
        // string location = "Sydney";
        // IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);

        // // Assert
        // TestHelpers.AssertStatusCode(ar1, HttpStatusCode.BadRequest);
    }
}
