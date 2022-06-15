using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// This interface is used by the Integration Tests to set up data for different data sources
/// Each method corresponds to the dataset needed by a different integration test
/// </summary>
public interface IDataSourceMocker
{
	public abstract void InitializeMocks();
	public WebApplicationFactory<Program> overrideWebAppFactory(WebApplicationFactory<Program> factory);  
	public abstract void SetupDataMock(DateTime start, DateTime end, string location);
};