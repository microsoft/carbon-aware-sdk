namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using NUnit.Framework;
using System.Net;
using System.Web;


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

	public CarbonAwareControllerTests(DataSourceType dataSource) : base(dataSource){}

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
    [TestCase("2022-1-1", 1, "eastus")]
    [TestCase("2021-12-25", 1, "westus")]
    public async Task BestLocations_ReturnsOK(DateTime start, int offset, string location)
        {
        var end = start.AddDays(offset);

        _dataSourceMocker.SetupDataMock(start, end, location);

        //Call the private method to construct with parameters
        var endpointURI = ConstructBestLocationsURI(location, start, end);

        //Get response and response content
        var result = await _client.GetAsync(endpointURI);
        var resultContent = await result.Content.ReadAsStringAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resultContent, Is.Not.Null);

        //// Setup with specific data point
        //GridEmissionDataPoint christmasDataPoint = WattTimeServerMocks.GetDefaultEmissionsDataPoint();
        //DateTimeOffset christmasTime = new(2021, 12, 25, 0, 0, 0, TimeSpan.Zero);
        //christmasDataPoint.PointTime = christmasTime;

        //WattTimeServerMocks.SetupDataMock(_server, new List<GridEmissionDataPoint> { christmasDataPoint });
        //var uri2 = $"/emissions/bylocations/best?locations=eastus&time={christmasTime.DateTime:yyyy-MM-dd}&toTime={christmasTime.DateTime.AddDays(1):yyyy-MM-dd}";
        //var result2 = await _client.GetAsync(uri2);
        //var content2 = await result2.Content.ReadAsStringAsync();

        //Assert.That(result2, Is.Not.Null);
        //Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        //Assert.That(content2, Is.Not.Null);
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
}