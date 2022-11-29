using CarbonAware.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Constants;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Exceptions;
using Castle.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[TestFixture]
public class ElectricityMapsClientTests
{

    private readonly string ForecastDataSourceKey = $"DataSources:ForecastDataSource";
    private readonly string ForecastDataSourceValue = $"ElectricityMapsTest";
    private readonly string HeaderKey = $"DataSources:Configurations:ElectricityMapsTest:APITokenHeader";
    private readonly string TokenKey = $"DataSources:Configurations:ElectricityMapsTest:APIToken";
    private readonly string UrlKey = $"DataSources:Configurations:ElectricityMapsTest:BaseUrl";
    private readonly string UseProxyKey = $"DataSources:Configurations:ElectricityMapsTest:Proxy:UseProxy";
    private readonly string ProxyUrlKey = $"DataSources:Configurations:ElectricityMapsTest:Proxy:Url";

    private readonly string TestLatitude = "36.6681";
    private readonly string TestLongitude = "-78.3889";
    private readonly string TestZone = "NL";

    private MockHttpMessageHandler MessageHandler { get; set; }

    private HttpClient HttpClient { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private ElectricityMapsClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>> Options { get; set; }

    private Mock<ILogger<ElectricityMapsClient>> Log { get; set; }


    private readonly string AuthHeader = "auth-token";
    
    private readonly string DefaultTokenValue = "myDefaultToken123";

    [SetUp]
    public void Setup()
    {
        this.Configuration = new ElectricityMapsClientConfiguration() { APITokenHeader = AuthHeader, APIToken = DefaultTokenValue };

        this.Options = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        this.Log = new Mock<ILogger<ElectricityMapsClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);
    }

    [Test]
    public void AllPublicMethods_ThrowsWhenNoAuth()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "{}");

        // Token Auth
        var noAuthConfig_Token = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        noAuthConfig_Token.Setup(o => o.CurrentValue).Returns(() => new ElectricityMapsClientConfiguration());
        var tokenClient = new ElectricityMapsClient(this.HttpClientFactory, noAuthConfig_Token.Object, this.Log.Object);
        
        // Trial Auth
        var noAuthConfig_Trial = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        noAuthConfig_Trial.Setup(o => o.CurrentValue).Returns(() => new ElectricityMapsClientConfiguration() { BaseUrl = BaseUrls.TrialBaseUrl});
        var trialClient = new ElectricityMapsClient(this.HttpClientFactory, noAuthConfig_Trial.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await tokenClient.GetCurrentForecastAsync(TestZone));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await tokenClient.GetCurrentForecastAsync(TestLatitude, TestLongitude));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await tokenClient.GetHistoryCarbonIntensityDataAsync(TestZone));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await tokenClient.GetHistoryCarbonIntensityDataAsync(TestLatitude, TestLongitude));

        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await trialClient.GetCurrentForecastAsync(TestZone));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await trialClient.GetCurrentForecastAsync(TestLatitude, TestLongitude));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await trialClient.GetHistoryCarbonIntensityDataAsync(TestZone));
        Assert.ThrowsAsync<ElectricityMapsClientHttpException>(async () => await trialClient.GetHistoryCarbonIntensityDataAsync(TestLatitude, TestLongitude));
    }
    
    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenBadJsonIsReturned()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "This is bad json");
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenNull()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "null");
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetCurrentForecastAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoPathsSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetCurrentForecastAsync(TestZone));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoZonesSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetCurrentForecastAsync(TestZone));
    }


    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        // Arrange
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString() == BaseUrls.TokenBaseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetZonesAllowedJsonString()));
                return Task.FromResult(response);
            }
            else if (m.RequestUri!.ToString().Contains(BaseUrls.TokenBaseUrl + Paths.Forecast))
            {
                Assert.That(HttpMethod.Get, Is.EqualTo(m.Method));
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetCurrentForecastJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var forecast = await client.GetCurrentForecastAsync(TestLatitude, TestLongitude);

        // Assert
        Assert.IsNotNull(forecast);
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(forecast?.UpdatedAt));
        Assert.That(TestZone, Is.EqualTo(forecast?.Zone));
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(forecastDataPoint?.DateTime));
        Assert.That(forecastDataPoint?.CarbonIntensity.ToString(), Is.EqualTo("999"));
    }

    [Test]
    public async Task GetCurrentForecastAsync_FreeTrialToken_DeserializesExpectedResponse()
    {
        // Arrange
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString().Contains(BaseUrls.TrialBaseUrl + Paths.Forecast))
            {
                Assert.That(HttpMethod.Get, Is.EqualTo(m.Method));
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetCurrentForecastJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

        var freeTrialOptions = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        freeTrialOptions.Setup(o => o.CurrentValue).Returns(() => new ElectricityMapsClientConfiguration() { BaseUrl = BaseUrls.TrialBaseUrl });
        var client = new ElectricityMapsClient(this.HttpClientFactory, freeTrialOptions.Object, this.Log.Object);

        // Act
        var forecast = await client.GetCurrentForecastAsync(TestLatitude, TestLongitude);

        // Assert
        Assert.IsNotNull(forecast);
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(forecast?.UpdatedAt));
        Assert.That(TestZone, Is.EqualTo(forecast?.Zone));
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(forecastDataPoint?.DateTime));
        Assert.That(forecastDataPoint?.CarbonIntensity.ToString(), Is.EqualTo("999"));
    }

    [Test]
    public void GetHistoryCarbonIntensityDataAsync_ThrowsWhenBadJsonIsReturned()
    {
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "This is bad json");

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetHistoryCarbonIntensityDataAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetHistoryCarbonIntensityDataAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoPathsSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetHistoryCarbonIntensityDataAsync(TestZone));
    }

    [Test]
    public void GetHistoryCarbonIntensityDataAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoZonesSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetHistoryCarbonIntensityDataAsync(TestZone));
    }

    [Test]
    public async Task GetHistoryCarbonIntensityDataAsync_DeserializesExpectedResponse()
    {
        // Arrange
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString() == BaseUrls.TokenBaseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetZonesAllowedJsonString()));
                return Task.FromResult(response);
            }
            else if (m.RequestUri!.ToString().Contains(BaseUrls.TokenBaseUrl + Paths.History))
            {
                Assert.That(HttpMethod.Get, Is.EqualTo(m.Method));
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetHistoryCarbonIntensityDataJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var data = await client.GetHistoryCarbonIntensityDataAsync(TestLatitude, TestLongitude);

        // Assert
        Assert.IsNotNull(data);
        Assert.That(TestZone, Is.EqualTo(data?.Zone));
        Assert.IsTrue(data?.HistoryData.Count() > 0);
        var dataPoint = data?.HistoryData.First();
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.DateTime));
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.UpdatedAt));
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.CreatedAt));
        Assert.That(dataPoint?.CarbonIntensity.ToString(), Is.EqualTo("999"));
        Assert.That(dataPoint?.EmissionFactorType, Is.EqualTo(EmissionsFactor.Lifecycle));
        Assert.IsFalse(dataPoint?.IsEstimated);
        Assert.IsEmpty(dataPoint?.EstimationMethod);
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.CreatedAt));
    }

    [Test]
    public async Task GetHistoryAsync_FreeTrialToken_DeserializesExpectedResponse()
    {
        // Arrange
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString().Contains(BaseUrls.TrialBaseUrl + Paths.History))
            {
                Assert.That(HttpMethod.Get, Is.EqualTo(m.Method));
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetHistoryCarbonIntensityDataJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

        var freeTrialOptions = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        freeTrialOptions.Setup(o => o.CurrentValue).Returns(() => new ElectricityMapsClientConfiguration() { BaseUrl = BaseUrls.TrialBaseUrl });
        var client = new ElectricityMapsClient(this.HttpClientFactory, freeTrialOptions.Object, this.Log.Object);

        // Act
        var data = await client.GetHistoryCarbonIntensityDataAsync(TestLatitude, TestLongitude);

        // Assert
        Assert.IsNotNull(data);
        Assert.That(TestZone, Is.EqualTo(data?.Zone));
        Assert.IsTrue(data?.HistoryData.Count() > 0);
        var dataPoint = data?.HistoryData.First();
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.DateTime));
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.UpdatedAt));
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.CreatedAt));
        Assert.That(dataPoint?.CarbonIntensity.ToString(), Is.EqualTo("999"));
        Assert.That(dataPoint?.EmissionFactorType, Is.EqualTo(EmissionsFactor.Lifecycle));
        Assert.IsFalse(dataPoint?.IsEstimated);
        Assert.IsEmpty(dataPoint?.EstimationMethod);
        Assert.That(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), Is.EqualTo(dataPoint?.CreatedAt));
    }

    [TestCase("x-token-header", "faketoken", TestName = "Client config test, token header + token value valid")]
    [TestCase("", "", TestName = "Client config test, Empty token header + empty token value valid")]
    public void ClientTest_With_ServiceCollection_VariedConfiguration_DoesNotThrow(string tokenHeader, string tokenValue)
    {
        // Arrange
        var settings = new Dictionary<string, string>(){
            { ForecastDataSourceKey, ForecastDataSourceValue }
        };
        if (!HeaderKey.IsNullOrEmpty()) settings.Add(HeaderKey, tokenHeader);
        if (!TokenKey.IsNullOrEmpty()) settings.Add(TokenKey, tokenValue);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();

        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddLogging()
            .AddElectricityMapsForecastDataSource(configuration.DataSources())
            .BuildServiceProvider();

        // Act & Assert
        Assert.DoesNotThrow(() => serviceProvider.GetRequiredService<IElectricityMapsClient>());
    }

    [TestCase("", "faketoken", "example.com", TestName = "Client config test, Empty token header + token value throws")]
    [TestCase("x-token-header", "", "example.com", TestName = "Client config test, token header + empty token value throws")]
    [TestCase("", "", "example.c%om", TestName = "Client config test, malformed url throws")]
    public void ClientTest_With_ServiceCollection_InvalidConfiguration_Throws(string tokenHeader, string tokenValue, string url)
    {
        // Arrange
        var settings = new Dictionary<string, string>(){
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { UrlKey, url}
        };
        if (!HeaderKey.IsNullOrEmpty()) settings.Add(HeaderKey, tokenHeader);
        if (!TokenKey.IsNullOrEmpty()) settings.Add(TokenKey, tokenValue);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();

        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddLogging()
            .AddElectricityMapsForecastDataSource(configuration.DataSources())
            .BuildServiceProvider();

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => serviceProvider.GetRequiredService<IElectricityMapsClient>());
    }

    [Test]
    public void ClientProxyTest_With_BadProxy_ThrowsException()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, "true" },
            { ProxyUrlKey, "http://fakeproxy:8080" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.AddLogging()
            .AddElectricityMapsForecastDataSource(configuration.DataSources())
            .BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IElectricityMapsClient>();

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetCurrentForecastAsync("lat", "long"));
    }

    [Test]
    public void ClientProxyTest_With_Missing_ProxyURL_ThrowsException()
    {
        // Arrange
        var settings = new Dictionary<string, string> {
            { ForecastDataSourceKey, ForecastDataSourceValue },
            { HeaderKey, AuthHeader },
            { TokenKey, DefaultTokenValue },
            { UseProxyKey, "true" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => serviceCollection.AddElectricityMapsForecastDataSource(configuration.DataSources()));
    }

    private void CreateBasicClient(string zoneContent, string resultContent)
    {
        this.CreateHttpClient(m =>
        {
            var isTokenAuthValid = m.RequestUri!.ToString().Contains(BaseUrls.TokenBaseUrl) && !m.Headers.Where(x => x.Key == "auth-token").Any();
            var isTrialAuthValid = m.RequestUri!.ToString().Contains(BaseUrls.TrialBaseUrl) && !m.Headers.Where(x => x.Key == "X-BLOBR-KEY").Any();
            // If no auth and token setup, return unauthorized
            if (isTokenAuthValid || isTrialAuthValid)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));
            }

            if (m.RequestUri?.ToString() == BaseUrls.TokenBaseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(zoneContent));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent(resultContent)));
        });
    }

    private HttpResponseMessage MockElectricityMapsResponse(HttpRequestMessage request, HttpContent responseContent)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content = responseContent;
        return response;
    }

    private void CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
    {
        this.MessageHandler = new MockHttpMessageHandler(requestDelegate);
        this.HttpClient = new HttpClient(this.MessageHandler);
        this.HttpClientFactory = Mock.Of<IHttpClientFactory>();
        Mock.Get(this.HttpClientFactory).Setup(h => h.CreateClient(IElectricityMapsClient.NamedClient)).Returns(this.HttpClient);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> RequestDelegate { get; set; }

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
        {
            this.RequestDelegate = requestDelegate;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await this.RequestDelegate.Invoke(request);
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
