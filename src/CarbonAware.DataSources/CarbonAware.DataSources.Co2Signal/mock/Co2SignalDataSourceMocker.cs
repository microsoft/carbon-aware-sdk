using CarbonAware.DataSources.Co2Signal.Model;
using CarbonAware.DataSources.Co2Signal.Constants;
using CarbonAware.DataSources.Mocks;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarbonAware.DataSources.Co2Signal.Mocks;

public class Co2SignalDataSourceMocker : IDataSourceMocker
{
    private readonly WireMockServer _server;
    private const string ZONE_NAME = "eastus";
    private const string ZONE_KEY = "US-NE-ISNE";
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    
    public Co2SignalDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Environment.SetEnvironmentVariable("DataSources__Configurations__Co2Signal__BaseURL", _server.Url!);
        Initialize();
    }

    public void SetupForecastMock()
    {
        throw new NotImplementedException();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new LatestCarbonIntensityData {
            CountryCode = location,
            Data = new CarbonIntensity() {
                Value = 100,
                DateTime = start,
                FossilFuelPercentage = 12.03d
            },
            Status = "ok",
            Units = new DataUnits {
                CarbonIntensity = "gCO2eq/kWh"
            }
        };

        SetupResponseGivenGetRequest(Paths.Latest, data);
    }

    public void SetupBatchForecastMock()
    {
        throw new NotImplementedException();
    }

    public void Initialize() {
        // No initialization needed
        return;
    }

    public void Reset() => _server.Reset();

    public void Dispose() => _server.Dispose();

    private void SetupResponseGivenGetRequest(string path, object body)
    {
        var jsonBody = JsonSerializer.Serialize(body, _options);
        _server
            .Given(Request.Create().WithPath("/" + path).UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                    .WithBody(jsonBody)
        );
    }
}