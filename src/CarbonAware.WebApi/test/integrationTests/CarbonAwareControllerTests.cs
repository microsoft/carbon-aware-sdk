namespace CarbonAware.WepApi.IntegrationTests;

using System.Net;
using CarbonAware.Tools.WattTimeClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using WireMock.Server;

public class APIWebApplicationFactory : WebApplicationFactory<Program>
{
    //private readonly string _environment;

    //public APIWebApplicationFactory(string environment = "Development")
    //{
    //    _environment = environment;
    //}

    //protected override IHost CreateHost(IHostBuilder builder)
    //{
    //    builder.UseEnvironment(_environment);

    //    builder.ConfigureServices(services =>
    //    {
    //        services.

    //    })


    //}

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

    [OneTimeSetUp]
    public void Setup()
    {
        _factory = new APIWebApplicationFactory();
        _client = _factory.CreateClient();

        _server = WireMockServer.Start();
        _server.SetupWattTimeServerMocks();

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

    [TearDown]
    public void ResetMockServer()
    {
        _server.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _server.Stop();
    }
}
