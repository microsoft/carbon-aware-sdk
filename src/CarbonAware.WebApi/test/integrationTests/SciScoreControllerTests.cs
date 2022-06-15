namespace CarbonAware.WepApi.IntegrationTests;

using System.Net;
using NUnit.Framework;
using System.Text.Json;
using System.Net.Http.Headers;
using WireMock.Server;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.WebApi.IntegrationTests;
using System.Net.Mime;
using CarbonAware.DataSources.Configuration;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class SciScoreControllerTests : IntegrationTestingBase
{
    private string marginalCarbonIntensityURI = "/sci-scores/marginal-carbon-intensity";

	public SciScoreControllerTests(DataSourceType dataSource) : base(dataSource) {}

    [TestCase("2022-1-1", 1, "eastus")]
    public async Task SCI_WithValidData_ReturnsContent(DateTime start, int offset, string location)
    {
        var end = start.AddDays(offset);

        _dataSourceMocker.SetupDataMock(start, end, location);

        string timeInterval = start.ToUniversalTime().ToString("O") + "/" + end.ToUniversalTime().ToString("O");

        //Construct body object and then serialize it with JSONSerializer
        object body = new
        {
            location = new
            {
                locationType = "CloudProvider",
                providerName = "Azure",
                regionName = location
            },
            timeInterval = timeInterval
        };

        var jsonBody = JsonSerializer.Serialize(body);
        StringContent _content = new StringContent(jsonBody);

        var mediaType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
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
}
