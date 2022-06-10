namespace CarbonAware.WepApi.IntegrationTests;

using System.Net;
using NUnit.Framework;
using System.Text.Json;
using System.Net.Http.Headers;
using WireMock.Server;
using CarbonAware.Tools.WattTimeClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using Microsoft.Extensions.Options;
using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class SciScoreControllerTests2
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private WebApplicationFactory<Program> _factory;
	private HttpClient _client;
    //private WireMockServer _server;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class WireWattTimeClient : WattTimeClient
    {
        public WireMockServer _server;

        public WireWattTimeClient(IHttpClientFactory factory, IOptionsMonitor<WattTimeClientConfiguration> configurationMonitor, ILogger<WattTimeClient> log, ActivitySource source) : base(factory, configurationMonitor, log, source)
            {
                _server = WireMockServer.Start();
                _server.SetupWattTimeServerMocks();
                string serverUrl = _server.Url!;
                this.client.BaseAddress = new Uri(serverUrl);
                Console.WriteLine(serverUrl);
            }
        }

    [OneTimeSetUp]
    public void Setup()
    {

        Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "WattTime");
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        using (var scope = serviceProvider.CreateScope())
                            {
                            var scopedServices = scope.ServiceProvider;
                            services.AddSingleton <IWattTimeClient, WireWattTimeClient>();
                            }
                    });
                });

        _client = _factory.CreateClient();

        //_server = WireMockServer.Start();
        //_server.SetupWattTimeServerMocks();
        //string serverUrl = _server.Url!;

        // set wattime base url to server url in config
        //Console.WriteLine(serverUrl);
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
                regionName = "uswest"
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

    //[TearDown]
    //public void ResetMockServer()
    //{
    //    _server.Reset();
    //}

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        //_server.Stop();
    }
}
