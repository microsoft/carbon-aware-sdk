namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using CarbonAware.Model;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class CarbonAwareControllerTests : IntegrationTestingBase
{
    private string healthURI = "/health";
    private string fakeURI = "/fake-endpoint";
    private string bestLocationsURI = "/emissions/bylocations/best";
    private string currentForecastURI = "/emissions/forecasts/current";
    private JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public CarbonAwareControllerTests(DataSourceType dataSource) : base(dataSource) { }

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

    //ISO8601: YYYY-MM-DD
    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus")]
    [TestCase("2021-12-25", "2021-12-26", "westus")]
    public async Task BestLocations_ReturnsOK(DateTimeOffset start, DateTimeOffset end, string location)
    {
        //Sets up any data endpoints needed for mocking purposes
        _dataSourceMocker.SetupDataMock(start, end, location);

        //Call the private method to construct with parameters
        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;
        queryStrings["time"] = $"{start:O}";
        queryStrings["toTime"] = $"{end:O}";

        var endpointURI = ConstructUriWithQueryString(bestLocationsURI,queryStrings);

        //Get response and response content
        var result = await _client.GetAsync(endpointURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task EmissionsForecastsCurrent_ReturnsNotImplemented()
    {
        var ignoredDataSources = new List<DataSourceType>() { DataSourceType.WattTime };
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore("Ignore test for data sources that implement '/emissions/forecasts/current'.");
        }

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = "fakeLocation";

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
    }

    [TestCase("westus")]
    public async Task EmissionsForecastsCurrent_ReturnsOk(string location)
    {
        
        var ignoredDataSources = new List<DataSourceType>() { DataSourceType.JSON };
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore("Ignore test for data sources that don't implement '/emissions/forecasts/current'.");
        }
        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    [Test]
    public async Task EmissionsForecastsCurrent_ReturnsOkWithNullLocation()
    {

        var ignoredDataSources = new List<DataSourceType>() { DataSourceType.JSON };
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore("Ignore test for data sources that don't implement '/emissions/forecasts/current'.");
        }
        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = null;

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }



    [Test]
    public async Task EmissionsForecastsCurrent_ReturnsBadResult()
    {

        var ignoredDataSources = new List<DataSourceType>() { DataSourceType.JSON };
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore("Ignore test for data sources that don't implement '/emissions/forecasts/current'.");
        }
        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}