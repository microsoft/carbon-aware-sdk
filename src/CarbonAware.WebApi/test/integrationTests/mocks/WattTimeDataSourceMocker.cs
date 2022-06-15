using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WireMock.Server;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;

    internal WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        WattTimeServerMocks.WattTimeServerSetupMocks(_server);
    }

    public void SetupDataMock(DateTime start, DateTime end, string location)
    {
        GridEmissionDataPoint newDataPoint = WattTimeServerMocks.GetDefaultEmissionsDataPoint();
        DateTimeOffset newTime = new(start);
        newDataPoint.PointTime = newTime;

        WattTimeServerMocks.SetupDataMock(_server, new List<GridEmissionDataPoint> { newDataPoint });
    }

    public WebApplicationFactory<Program> overrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<CarbonAwareVariablesConfiguration>(configOpt =>
                {
                    configOpt.CarbonIntensityDataSource = _dataSource.ToString();
                });

                services.Configure<WattTimeClientConfiguration>(configOpt =>
                {
                    configOpt.BaseUrl = _server.Url;
                });
            });
        });
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}