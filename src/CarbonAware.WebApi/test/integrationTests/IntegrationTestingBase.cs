using CarbonAware.DataSources.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// A base class that does all the common setup for the Integration Testing
/// Overrides WebAPI factory by switching out different configurations via _datasource
/// </summary>
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
		//Switch between different data sources as needed
		//Each datasource should have an accompanying DataSourceMocker that will perform setup activities
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

		//Setup the WebAppFactory with custom settings as required by the datasource
		//For instance, overriding specific clients with new URLs.
		_factory = _dataSourceMocker.overrideWebAppFactory(_factory);
		_client = _factory.CreateClient();
		}

	[OneTimeTearDown]
	public void TearDown()
		{
		_client.Dispose();
		_factory.Dispose();
		_dataSourceMocker.Dispose();
		}
	}