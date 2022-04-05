using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CarbonAware.Tools.WattTimeClient.Model;
using System.Web;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CarbonAware.Tools.WattTimeClient
{
    public class WattTimeClient : IWattTimeClient
    {
        private const string BaseUrl = "https://api2.watttime.org/v2/";
        private HttpClient client;
        internal string? authToken = null;

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private IOptionsMonitor<WattTimeClientConfiguration> ConfigurationMonitor { get; }

        private WattTimeClientConfiguration Configuration => this.ConfigurationMonitor.CurrentValue;

        private ActivitySource ActivitySource { get; }

        private ILogger<WattTimeClient> Log { get; }

        public WattTimeClient(HttpClient httpClient, IOptionsMonitor<WattTimeClientConfiguration> configurationMonitor, ILogger<WattTimeClient> log, ActivitySource source)
        {
            this.client = httpClient;
            this.client.BaseAddress = new Uri(BaseUrl);
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.ConfigurationMonitor = configurationMonitor;
            this.ActivitySource = source;
            this.Log = log;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string balancingAuthority, string startTime, string endTime){
            var url = BuildUrlWithQueryString(
                "data",
                new Dictionary<string, string>() {
                    { "ba", balancingAuthority },
                    { "starttime", startTime },
                    { "endtime", endTime }
                });

            Log.LogInformation("Requesting grid emission data using url {url}", url);

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.AddTag("ba", balancingAuthority);

                var result = await this.GetAsyncWithAuthRetries(url);

                Log.LogDebug("For query {url}, received data {result}", url, result);

                var data = JsonSerializer.Deserialize<List<GridEmissionDataPoint>>(result, options);

                if (data is not null)
                {
                    return data;
                } else {
                    throw new System.Exception("Serialized response from WattTime was null.");
                }
            }
        }

        /// <inheritdoc/>
        public async Task<Forecast> GetCurrentForecastAsync(string balancingAuthority){
            var url = BuildUrlWithQueryString(
                "forecast",
                new Dictionary<string, string>() {
                    { "ba", balancingAuthority }
                });

            Log.LogInformation("Requesting current forecast using url {url}", url);

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.AddTag("ba", balancingAuthority);

                var result = await this.GetAsyncWithAuthRetries(url);

                Log.LogDebug("For query {url}, received data {result}", url, result);

                var data = JsonSerializer.Deserialize<Forecast>(result);

                if (data is not null)
                {
                    return data;
                } else {
                    throw new System.Exception("Serialized response from WattTime was null.");
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Forecast>> GetForecastByDateAsync(string balancingAuthority, string startTime, string endTime){
            var url = BuildUrlWithQueryString(
                "forecast",
                new Dictionary<string, string>() {
                    { "ba", balancingAuthority },
                    { "starttime", startTime },
                    { "endtime", endTime }
                });

            Log.LogInformation("Getting forecast by date using url {url}", url);

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.AddTag("ba", balancingAuthority);

                var result = await this.GetAsyncWithAuthRetries(url);

                Log.LogDebug("For query {url}, received data {result}", url, result);

                var data = JsonSerializer.Deserialize<List<Forecast>>(result, options);
                
                if (data is not null)
                {
                    return data;
                } else {
                    throw new System.Exception("Serialized response from WattTime was null.");
                }
            }
        }

        /// <inheritdoc/>
        public async Task<BalancingAuthority> GetBalancingAuthorityAsync(string latitude, string longitude){
            var url = BuildUrlWithQueryString(
                "ba-from-loc",
                new Dictionary<string, string>() {
                    { "latitude", latitude },
                    { "longitude", longitude }
                });

            Log.LogInformation("Getting balancing authority using url {url}", url);

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.AddTag("latitude", latitude);
                activity?.AddTag("longitude", longitude);

                var result = await this.GetAsyncWithAuthRetries(url);

                Log.LogDebug("For query {url}, received data {result}", url, result);

                var data = JsonSerializer.Deserialize<BalancingAuthority>(result, options);

                if (data is not null)
                {
                    return data;
                } else {
                    throw new System.Exception("Serialized response from WattTime was null.");
                }
            }
        }

        private async Task<string> GetAsyncWithAuthRetries(string uriPath, int retries = 1)
        {
            var retriableStatusCodes = new List<System.Net.HttpStatusCode>()
                {
                    System.Net.HttpStatusCode.Unauthorized, System.Net.HttpStatusCode.Forbidden
                };
            if( this.authToken == null )
            {
                Log.LogDebug("No authtoken provided.  Authenticating.");
                await this.UpdateAuthTokenAsync();
            }

            var response = await this.client.GetAsync(uriPath);
            if (response.IsSuccessStatusCode)
            {
                Log.LogDebug("Successfully retrieved {url}", uriPath);

                var data = response.Content.ReadAsStringAsync();
                return data.Result ?? string.Empty; 
            }
            else if (retries > 0 && retriableStatusCodes.Contains(response.StatusCode))
            {
                Log.LogDebug("Failed to get url {url} with status code {statusCode}.  {retries} retries remaining.", uriPath, response.StatusCode, retries);
                retries--;
                await this.UpdateAuthTokenAsync();
                return await this.GetAsyncWithAuthRetries(uriPath, retries);
            }
            else
            {
                Log.LogError("Error getting data from WattTime.  StatusCode: {statusCode}", response.StatusCode);

                throw new System.Exception($"Error getting data from WattTime: {response.StatusCode}");
            }
        }

        private async Task UpdateAuthTokenAsync(){
            // Request auth token from WattTime API
            var authToken = Encoding.ASCII.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}");
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.SetTag("username", this.Configuration.Username);

                Log.LogInformation("Attempting to log in user {username}", this.Configuration.Username);

                var result = await this.client.GetStringAsync("login");

                // Store token for use with API requests
                var data = JsonSerializer.Deserialize<LoginResult>(result, options);

                if(data is not null)
                {
                    this.authToken = data.Token;
                } else {
                    throw new System.Exception("Serialized response from WattTime was null.");
                }
                
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.authToken);
            }
        }

        private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, String.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
            }

            var query = HttpUtility.ParseQueryString("");
            foreach(var kvp in queryStringParams)
            {
                query[kvp.Key] = kvp.Value;
            }

            var result = $"{url}?{query.ToString()}";

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Built url {result} from url {url} and query string parameters {parameters}", result, url, String.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
            }

            return result;
        }
    }
}
