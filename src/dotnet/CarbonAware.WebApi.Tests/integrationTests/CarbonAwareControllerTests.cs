namespace CarbonAware.WepApi.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public void Setup()
    {
        _factory = new APIWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task HealthCheck_ReturnsOK()
    {
        //Use client to get endpoint
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
        var stringUri = "/emissions/bylocations/best?locations=eastus&locations=westus&time=2022-01-01&toTime=2022-05-17";

        var result = await _client.GetAsync(stringUri);
        //Get actual response content
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resultContent, Is.Not.Null);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
