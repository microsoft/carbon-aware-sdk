namespace CarbonAware.WepApi.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Moq;


public class APIWebApplicationFactory : WebApplicationFactory<Program>
{

}

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class CarbonAwareControllerTests
{
    private APIWebApplicationFactory _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void GivenARequestToTheController()
    {
        _factory = new APIWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task HealthCheck_ReturnsOK()
    {
        var result = await _client.GetAsync("/health");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));

	}

    public async Task FakeEndPoint_ReturnsNotFound()
    {
        var result = await _client.GetAsync("/fake-endpoint");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

    }


    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

}
