using CarbonAware.DataSources.Co2Signal.Client;
using CarbonAware.DataSources.Co2Signal.Configuration;
using CarbonAware.DataSources.Co2Signal.Constants;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Moq.Contrib.HttpClient;
using System.Text.Json;

namespace CarbonAware.DataSources.Co2Signal.Tests;

[TestFixture]
public class Co2SignalClientTests
{
    private readonly string AuthHeader = "auth-token";
    private readonly string DefaultTokenValue = "myDefaultToken123";
    private readonly string TestLatitude = "36.6681";
    private readonly string TestLongitude = "-78.3889";
    private readonly string TestCountryCode = "NL";

    private Mock<HttpMessageHandler> Handler { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private Co2SignalClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<Co2SignalClientConfiguration>> Options { get; set; }

    private Mock<ILogger<Co2SignalClient>> Log { get; set; }

    [SetUp]
    public void Setup()
    {
        this.Configuration = new Co2SignalClientConfiguration() { APITokenHeader = AuthHeader, APIToken = DefaultTokenValue };

        this.Options = new Mock<IOptionsMonitor<Co2SignalClientConfiguration>>();
        this.Log = new Mock<ILogger<Co2SignalClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.Handler = new Mock<HttpMessageHandler>();
        this.HttpClientFactory = Handler.CreateClientFactory();
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(ICo2SignalClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                return client;
            });
    }

    [TestCase("ww.ca.u", "mytoken", TestName = "ClientInstantiation_FailsForInvalidConfig: url")]
    [TestCase("www.example.com", "", TestName = "ClientInstantiation_FailsForInvalidConfig: Token")]
    public void ClientInstantiation_FailsForInvalidConfig(string baseUrl, string token)
    {
        // Arrange
        this.Configuration = new Co2SignalClientConfiguration()
        {
            APITokenHeader = "auth-token",
            APIToken = token,
            BaseUrl = baseUrl,
        };

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => new Co2SignalClient(this.HttpClientFactory, this.Options.Object, this.Log.Object));
    }

    [Test]
    public void AllPublicMethods_DoNotSwallowBadProxyExceptions()
    {
        // Arrange
        var mockHttpClientFactory = Mock.Of<IHttpClientFactory>();
        var mockHandler = new Mock<HttpClientHandler>();

        // A bad proxy will throw HttpRequestException when used so we mock that here.
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException());

        Mock.Get(mockHttpClientFactory)
            .Setup(h => h.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(mockHandler.Object));

        var client = new Co2SignalClient(mockHttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetLatestCarbonIntensityAsync(TestCountryCode));
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetLatestCarbonIntensityAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void AllPublicMethods_ThrowJsonException_WhenBadJsonIsReturned()
    {
        // Arrange
        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.Latest) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "This is bad json");

        var client = new Co2SignalClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetLatestCarbonIntensityAsync(TestLatitude, TestLongitude));
    }

     public async Task GetLatestCarbonIntensityAsync_DeserializesExpectedResponse()
    {
        // Arrange
        AddHandler_RequestResponse(r =>
            {
                return r.RequestUri!.ToString().Contains(Paths.Latest) && r.Method == HttpMethod.Get;
            }, System.Net.HttpStatusCode.OK, TestData.GetHistoryCarbonIntensityDataJsonString());

        var client = new Co2SignalClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var latestData = await client.GetLatestCarbonIntensityAsync(TestLatitude, TestLongitude);
        var dataPoint = latestData?.Data;

        // Assert
        Assert.That(latestData, Is.Not.Null);
        Assert.That(latestData?.CountryCode, Is.EqualTo(TestCountryCode));
        Assert.Multiple(() =>
        {
            Assert.That(dataPoint?.DateTime, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(dataPoint?.Value, Is.EqualTo(999));
        });
    }

    /**
     * Helper to add client handler for request predicate and corresponding status code and response content
     */
    private void AddHandler_RequestResponse(Predicate<HttpRequestMessage> requestPredicate, System.Net.HttpStatusCode statusCode, string? responseContent = null) {
        if (responseContent != null) {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode, new StringContent(responseContent));
        } else {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode);
        }    
    }
}
