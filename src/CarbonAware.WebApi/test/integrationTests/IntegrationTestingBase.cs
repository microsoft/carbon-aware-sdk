using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireMock.Server;

namespace CarbonAware.WebApi.IntegrationTests;

public abstract class IntegrationTestingBase
{
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	protected CarbonAwareWebAppFactory _factory;
	protected HttpClient _client;
	protected WireMockServer _server;
	#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}