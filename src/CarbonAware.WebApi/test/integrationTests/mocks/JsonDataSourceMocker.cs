using Microsoft.AspNetCore.Mvc.Testing;

namespace CarbonAware.WebApi.IntegrationTests;
public class JsonDataSourceMocker : IDataSourceMocker
{
    internal JsonDataSourceMocker() { }

    public void SetupDataMock(DateTime start, DateTime end, string location) { }

    public WebApplicationFactory<Program> overrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory;
    }

    public void Initialize() { }
    public void Reset() { }
    public void Dispose() { }
}