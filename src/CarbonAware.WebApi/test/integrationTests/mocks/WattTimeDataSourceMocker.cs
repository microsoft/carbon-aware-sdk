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

    public class WireMockWattTimeClient : WattTimeClient
    {
        public WireMockWattTimeClient(IHttpClientFactory factory, IOptionsMonitor<WattTimeClientConfiguration> configurationMonitor, ILogger<WattTimeClient> log, ActivitySource source, string address) : base(factory, configurationMonitor, log, source)
        {
            this.client.BaseAddress = new Uri(address);
        }
    }

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

                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    services.AddSingleton<IWattTimeClient>(clientServices =>
                    {
                        return new WireMockWattTimeClient(
                            clientServices.GetRequiredService<IHttpClientFactory>(),
                            clientServices.GetRequiredService<IOptionsMonitor<WattTimeClientConfiguration>>(),
                            clientServices.GetRequiredService<ILogger<WattTimeClient>>(),
                            clientServices.GetRequiredService<ActivitySource>(),
                            _server.Url);
                    });
                }
            });
        });
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}