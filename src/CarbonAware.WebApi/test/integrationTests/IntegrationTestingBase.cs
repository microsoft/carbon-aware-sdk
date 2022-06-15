using CarbonAware.DataSources.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace CarbonAware.WebApi.IntegrationTests;

public abstract class IntegrationTestingBase
{
    internal DataSourceType _dataSource;
	internal WebApplicationFactory<Program> _factory;
    protected HttpClient _client;
    protected IDataSourceMocker _dataSourceMocker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public IntegrationTestingBase(DataSourceType dataSource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
        _dataSource = dataSource;
        _factory = new WebApplicationFactory<Program>();
        }


    [OneTimeSetUp]
    public void Setup()
    {
        switch (_dataSource)
        {
            case DataSourceType.JSON:
            {
                _dataSourceMocker = new JsonDataSourceMocker();
                break;
            }
		    case DataSourceType.WattTime:
            {
                _dataSourceMocker = new WattTimeDataSourceMocker();
                break;
            }
            case DataSourceType.None:
            {
                throw new NotSupportedException($"DataSourceType {_dataSource.ToString()} not supported");
            }
        }

        _factory = _dataSourceMocker.overrideWebAppFactory(_factory);
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}