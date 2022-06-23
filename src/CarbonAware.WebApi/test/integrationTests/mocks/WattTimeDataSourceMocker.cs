using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;
using System.Net;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net.Mime;
using CarbonAware.Tools.WattTimeClient.Model;
using CarbonAware.Tools.WattTimeClient.Constants;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;
    private static readonly DateTimeOffset testDataPointOffset = new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly string testBA = "TEST_BA";
    private static readonly GridEmissionDataPoint defaultDataPoint = new()
    {
        BalancingAuthorityAbbreviation = testBA,
        Datatype = "dt",
        Frequency = 300,
        Market = "mkt",
        PointTime = testDataPointOffset,
        Value = 999.99F,
        Version = "1.0"
    };

    private static readonly List<GridEmissionDataPoint> defaultDataList = new() { defaultDataPoint };

    private static readonly List<Forecast> defaultForecastList = new()
    {
        new Forecast()
        {
            GeneratedAt = testDataPointOffset,
            ForecastData = new List<GridEmissionDataPoint>()
                    {
                        new GridEmissionDataPoint()
                        {
                            BalancingAuthorityAbbreviation = testBA,
                            PointTime = testDataPointOffset,
                            Value = 999.99F,
                            Version = "1.0"
                        }
                    }
        }
    };

    private static readonly BalancingAuthority defaultBalancingAuthority = new()
    {
        Id = 12345,
        Abbreviation = testBA,
        Name = "Test Balancing Authority"
    };

    private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

    internal WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Initialize();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new List<GridEmissionDataPoint>();
        DateTimeOffset pointTime = start;
        TimeSpan duration = TimeSpan.FromSeconds(300);

        while (pointTime < end)
        {
            var newDataPoint = new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = testBA,
                PointTime = pointTime,
                Value = 999.99F,
                Version = "1.0",
                Datatype = "dt",
                Frequency = 300,
                Market = "mkt",
            };

            data.Add(newDataPoint);
            pointTime = newDataPoint.PointTime + duration;
        }

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(data));
    }

    public void SetupForecastMock()
    {
        var start = new DateTimeOffset(2022, 6, 23, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2022, 6, 24, 0, 0, 0, TimeSpan.Zero);
        var pointTime = start;
        var ForecastData = new List<GridEmissionDataPoint>();
        var currValue = 200.0F;
        var newForecastPoint =
            new GridEmissionDataPoint();
        while (pointTime < end)
        {
            newForecastPoint = new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = "testBA",
                Datatype = "dt",
                Frequency = 300,
                Market = "mkt",
                PointTime = start,
                Value = currValue,
                Version = "1.0"
            };
            newForecastPoint.PointTime = pointTime;
            newForecastPoint.Value = currValue;
            ForecastData.Add(newForecastPoint);
            pointTime = pointTime + TimeSpan.FromMinutes(5);
            currValue = currValue + 5.0F;
        }

        var forecast = new Forecast()
        {
            ForecastData = ForecastData,
            GeneratedAt = testDataPointOffset
        };
        SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(forecast));
    }

    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<WattTimeClientConfiguration>(configOpt =>
                {
                    configOpt.BaseUrl = _server.Url!;
                });
            });
        });
    }

    public void Initialize()
    {
        SetupBaMock();
        SetupLoginMock();
    }

    public void Reset()
    {
        _server.Reset();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private void SetupResponseGivenGetRequest(string path, string body, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = MediaTypeNames.Application.Json)
    {
        _server
            .Given(Request.Create().WithPath("/" + path).UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", contentType)
                    .WithBody(body)
        );
    }

    private void SetupBaMock(BalancingAuthority? content = null) =>
    SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, JsonSerializer.Serialize(content ?? defaultBalancingAuthority));

    private void SetupLoginMock(LoginResult? content = null) =>
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));


}