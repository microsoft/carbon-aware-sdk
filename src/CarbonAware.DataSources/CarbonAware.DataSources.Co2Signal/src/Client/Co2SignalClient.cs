using CarbonAware.DataSources.Co2Signal.Configuration;
using CarbonAware.DataSources.Co2Signal.Constants;
using CarbonAware.DataSources.Co2Signal.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Web;

namespace CarbonAware.DataSources.Co2Signal.Client;

internal class Co2SignalClient : ICo2SignalClient
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    private readonly IOptionsMonitor<Co2SignalClientConfiguration> _configurationMonitor;
    private Co2SignalClientConfiguration _configuration => this._configurationMonitor.CurrentValue;
    private readonly ILogger<Co2SignalClient> _log;

    public Co2SignalClient(IHttpClientFactory factory, IOptionsMonitor<Co2SignalClientConfiguration> monitor, ILogger<Co2SignalClient> log)
    {
        _client = factory.CreateClient(ICo2SignalClient.NamedClient);
        _configurationMonitor = monitor;
        _log = log;
        _configuration.Validate();
        _client.BaseAddress = new Uri(this._configuration.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        if (!string.IsNullOrWhiteSpace(_configuration.APITokenHeader))
        {
            _client.DefaultRequestHeaders.Add(_configuration.APITokenHeader, _configuration.APIToken);
        }
    }

    /// <inheritdoc/>
    public async Task<LatestCarbonIntensityData> GetLatestCarbonIntensityAsync(string countryCode)
    {
        _log.LogDebug("Requesting latest carbon intensity using country code {countryCode}",
            countryCode);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.CountryCode, countryCode },
        };

        return await GetLatestCarbonIntensityDataAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<LatestCarbonIntensityData> GetLatestCarbonIntensityAsync(string latitude, string longitude)
    {
        _log.LogDebug("Requesting latest carbon intensity using latitude {latitude} longitude {longitude}",
            latitude, longitude);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Latitude, latitude },
            { QueryStrings.Longitude, longitude }
        };

        return await GetLatestCarbonIntensityDataAsync(parameters);
    }
    
    /// <summary>
    /// Async method to make GET request to Latest endpoint
    /// </summary>
    /// <param name="parameters">List of query params</param>
    /// <returns>A <see cref="Task{LatestCarbonIntensityData}"/> which contains the latest emissions data point given the query params.</returns>
    /// <exception cref="Co2SignalClientException">Can be thrown when errors occur connecting to Co2Signal client.  See the Co2SignalClientException class for documentation of expected status codes.</exception>
    private async Task<LatestCarbonIntensityData> GetLatestCarbonIntensityDataAsync(Dictionary<string, string> parameters)
    {
        using Stream result = await this.MakeRequestGetStreamAsync(Paths.Latest, parameters);
        return await JsonSerializer.DeserializeAsync<LatestCarbonIntensityData>(result, _options) ?? throw new Co2SignalClientException($"Error getting latest carbon intensity data");
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string uriPath)
    {
        HttpResponseMessage response = await _client.GetAsync(uriPath, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _log.LogError("Error getting data from Co2Signal {uriPath}. StatusCode: {statusCode}. Content: {content}", uriPath, response.StatusCode, content);
            throw new Co2SignalClientHttpException($"Error requesting {uriPath} - Content: {content}", response);
        }
        return response;
    }

    private async Task<Stream> MakeRequestGetStreamAsync(string path, Dictionary<string, string>? parameters = null)
    {
        var url = path;
        if (parameters is not null)
        {
            url = BuildUrlWithQueryString(path, parameters);
        }
        _log.LogDebug("Requesting data using url {url}", url);
        var response = await this.GetResponseAsync(url);
        return await response.Content.ReadAsStreamAsync();
    }

    private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
    {
        if (_log.IsEnabled(LogLevel.Debug))
        {
            _log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }
        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach(var kvp in queryStringParams)
        {
            query[kvp.Key] = kvp.Value;
        }
        var result = $"{url}?{query}";
        if (_log.IsEnabled(LogLevel.Debug))
        {
            _log.LogDebug("Built url {result} from url {url} and query string parameters {parameters}", result, url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }
        return result;
    }
}
