namespace CarbonAware.WepApi.IntegrationTests;

using System.Net;
using NUnit.Framework;
using System.Text.Json;
using System.Net.Http.Headers;
using WireMock.Server;
using CarbonAware.Tools.WattTimeClient;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class SciScoreControllerTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private APIWebApplicationFactory _factory;
	private HttpClient _client;
    private WireMockServer _server;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [OneTimeSetUp]
    public void Setup()
    {
        _server = WireMockServer.Start();
        _server.SetupWattTimeServerMocks();
        string serverUrl = _server.Url!;

        Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "WattTime");
        Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", serverUrl);

        _factory = new APIWebApplicationFactory();
        _client = _factory.CreateClient();

        // set wattime base url to server url in config
        Console.WriteLine(serverUrl);
    }

    [Test]
    public async Task SCI_WithValidData_ReturnsContent()
    {
        //Construct body object and then serialize it with JSONSerializer
        object body = new
        {
            location = new
            {
                locationType = "CloudProvider",
                providerName = "Azure",
                regionName = "westus2"
            },
            timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z"
        };

        var jsonBody = JsonSerializer.Serialize(body);
        StringContent _content = new StringContent(jsonBody);

        var mediaType = new MediaTypeHeaderValue("application/json");
        _content.Headers.ContentType = mediaType;

        var result = await _client.PostAsync("/sci-scores/marginal-carbon-intensity", _content);
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resultContent, Is.Not.Null);
    }

    [Test]
    public async Task SCI_WithInvalidData_ReturnsBadRequest()
    {
        object body = new
        {
            location = new { },
            timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z"
        };

        var jsonBody = JsonSerializer.Serialize(body);
        StringContent _content = new StringContent(jsonBody);

        var mediaType = new MediaTypeHeaderValue("application/json");
        _content.Headers.ContentType = mediaType;

        var result = await _client.PostAsync("/sci-scores/marginal-carbon-intensity", _content);
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TearDown]
    public void ResetMockServer()
    {
        _server.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", "");
        Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "");

        _client.Dispose();
        _factory.Dispose();
        _server.Stop();
    }
}
