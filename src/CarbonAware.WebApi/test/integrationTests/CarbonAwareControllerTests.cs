namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.Tools.WattTimeClient;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net;
using WireMock.Server;

public class APIWebApplicationFactory : WebApplicationFactory<Program> {

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
    private WireMockServer _server;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private bool resetEnvironment = false;

    [OneTimeSetUp]
    public void Setup()
    {
		//Defaults to JSON Integration Testing
		//WattTime can be tested by uncommenting the line below or setting an env variable
		//Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "WattTime");

		if (Environment.GetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource") == "WattTime")
        {
            //Start WireMock server and point the integration tests to it for mocking outbound calls
            //Only for WattTime Integration Testing
            _server = WireMockServer.Start();
            _server.SetupWattTimeServerMocks();
            string serverUrl = _server.Url!;

            Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", serverUrl);
            resetEnvironment = true;
        }

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
        //Getting static test data, does not need server initialization
        var start = WattTimeServerMocks.GetTestDataPointOffset().DateTime;
        var end = WattTimeServerMocks.GetTestDataPointOffset().DateTime.AddDays(1);
		var stringUri = $"/emissions/bylocations/best?locations=eastus&time={start:yyyy-MM-dd}&toTime={end:yyyy-MM-dd}";

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
        if (resetEnvironment)
        {
            Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", "");
            Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "");
            _server.Stop();
        }

        _client.Dispose();
        _factory.Dispose();
    }
}