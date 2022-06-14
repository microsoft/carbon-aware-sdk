namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net;
using System.Web;
using WireMock.Server;
using CarbonAware.WebApi.IntegrationTests;

internal class APIWebApplicationFactory : WebApplicationFactory<Program> {

}

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class CarbonAwareControllerTests : IntegrationTestingBase
{
    private bool resetEnvironment = false;
    private string healthURI = "/health";
    private string fakeURI = "/fake-endpoint";
    private string bestLocationsURI = "/emissions/bylocations/best";

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
            _server.WattTimeServerSetupMocks();
            string serverUrl = _server.Url!;

            Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", serverUrl);
            resetEnvironment = true;
       }

        _factory = new CarbonAwareWebAppFactory();
        _client = _factory.CreateClient();
        }

    [Test]
    public async Task HealthCheck_ReturnsOK()
    {
        //Use client to get endpoint
        var result = await _client.GetAsync(healthURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

    [Test]
    public async Task FakeEndPoint_ReturnsNotFound()
    {
        var result = await _client.GetAsync(fakeURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task BestLocations_ReturnsOK()
    {
        // Setup with default data
        WattTimeServerMocks.SetupDataMock(_server);
        var start = WattTimeServerMocks.GetTestDataPointOffset().DateTime;
        var end = WattTimeServerMocks.GetTestDataPointOffset().DateTime.AddDays(1);
        var location = "eastus";
        
        //Call the private method to construct with parameters
        var endpointURI = ConstructBestLocationsURI(location, start, end);

        //Get response and response content
        var result = await _client.GetAsync(endpointURI);
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resultContent, Is.Not.Null);

        // Setup with specific data point
        GridEmissionDataPoint christmasDataPoint = WattTimeServerMocks.GetDefaultEmissionsDataPoint();
        DateTimeOffset christmasTime = new(2021, 12, 25, 0, 0, 0, TimeSpan.Zero);
        christmasDataPoint.PointTime = christmasTime;

        WattTimeServerMocks.SetupDataMock(_server, new List<GridEmissionDataPoint> { christmasDataPoint });
        var uri2 = $"/emissions/bylocations/best?locations=eastus&time={christmasTime.DateTime:yyyy-MM-dd}&toTime={christmasTime.DateTime.AddDays(1):yyyy-MM-dd}";
        var result2 = await _client.GetAsync(uri2);
        var content2 = await result2.Content.ReadAsStringAsync();

        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content2, Is.Not.Null);
    }

    private string ConstructBestLocationsURI(string location, DateTime start, DateTime end)
    {
        // Use HTTP Query builder
        var builder = new UriBuilder();

        //Add all query parameters
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["locations"] = location;
        query["time"] = $"{start:yyyy-MM-dd}";
        query["toTime"] = $"{end:yyyy-MM-dd}";

        //Generate final query string
        builder.Query = query.ToString();
        builder.Path = bestLocationsURI;

        //These values are blank as they are set by the SDK
        builder.Scheme = "";
        builder.Port = -1;
        builder.Host = "";

        return builder.ToString();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (resetEnvironment)
        {
            Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", "");
            _server.Stop();
        }

        _client.Dispose();
        _factory.Dispose();
    }
}