using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace CarbonAware.LocationSources;

internal class IpStackClient : IIpStackClient
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    // private readonly IOptionsMonitor<ElectricityMapsClientConfiguration> _configurationMonitor;
    // private ElectricityMapsClientConfiguration _configuration => this._configurationMonitor.CurrentValue;
    private readonly ILogger<IpStackClient> _log;
    // private readonly Lazy<Task<Dictionary<string, ZoneData>>> _zonesAllowed;

    public IpStackClient(IHttpClientFactory factory, ILogger<IpStackClient> log)
    {
        _client = factory.CreateClient(IIpStackClient.NamedClient);
        // _configurationMonitor = monitor;
        _log = log;
        // _configuration.Validate();
        _client.BaseAddress = new Uri("https://ipstack.com/");
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }


    public async Task<string> GetData()
    {
        var data = await _client.GetStringAsync("/documentation");
        return "";
        // throw new NotImplementedException();
    }
}
