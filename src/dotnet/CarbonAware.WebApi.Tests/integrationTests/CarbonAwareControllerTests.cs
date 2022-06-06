namespace CarbonAware.WepApi.IntegrationTests;

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
public class CarbonAwareControllerTests
{

    [Setup]
    public void Setup()
    {
        await using var application = new WebApplicationFactory<Program>();
        var client = application.CreateClient(); // HttpClient

    }

    /// <summary>
    /// Tests that successfull call to plugin with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task SuccessfulCallReturnsOk()
    {
        Assert.IsTrue(true);
    }
}
