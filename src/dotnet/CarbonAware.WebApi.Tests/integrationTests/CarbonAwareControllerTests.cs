namespace CarbonAware.WepApi.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Json;
using Moq;
using System.Net.Http.Headers;

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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private APIWebApplicationFactory _factory;
	private HttpClient _client;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

    [Test]
    public async Task FakeEndPoint_ReturnsNotFound()
    {
        var result = await _client.GetAsync("/fake-endpoint");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task BestLocations_ReturnsOK()
    {
        var result = await _client.GetAsync("/emissions/bylocations/best?locations=eastus&locations=westus&time=2022-01-01&toTime=2022-05-17");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_SCI()
    {
        var body = @"{
                " + "\n" +
                 @"    ""location"": {
                " + "\n" +
                 @"        ""locationType"": ""CloudProvider"",
                " + "\n" +
                 @"        ""cloudProvider"": ""Azure"",
                " + "\n" +
                 @"        ""regionName"": ""westeurope""
                " + "\n" +
                 @"    },
                " + "\n" +
                 @"    ""timeInterval"": ""2022-05-08T13:00:00Z/2022-05-08T15:30:00Z""
                " + "\n" +
                 @"}";

        StringContent _content = new StringContent(body);
        var mediaType = new MediaTypeHeaderValue("application/json");
        _content.Headers.ContentType = mediaType;


        var result = await _client.PostAsync("/sci-scores/marginal-carbon-intensity", _content);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

}
