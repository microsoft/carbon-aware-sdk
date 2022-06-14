namespace CarbonAware.WepApi.IntegrationTests;

using System.Net;
using NUnit.Framework;
using System.Text.Json;
using System.Net.Http.Headers;
using WireMock.Server;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class SciScoreControllerTests : IntegrationTestingBase
{
    private bool resetEnvironment = false;

    private string marginalCarbonIntensityURI = "/sci-scores/marginal-carbon-intensity";

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

        _factory = new CarbonAwareWebAppFactory();
        _client = _factory.CreateClient();

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

        var result = await _client.PostAsync(marginalCarbonIntensityURI, _content);
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

        var result = await _client.PostAsync(marginalCarbonIntensityURI, _content);
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if(resetEnvironment)
        {
            Environment.SetEnvironmentVariable("WattTimeClient__baseUrl", "");
            _server.Stop();
        }

        _client.Dispose();
        _factory.Dispose();
    }
}
