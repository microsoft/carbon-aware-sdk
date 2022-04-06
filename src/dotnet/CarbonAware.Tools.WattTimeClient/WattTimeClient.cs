using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CarbonAware.Tools.WattTimeClient.Model;
using System.Web;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Net;

namespace CarbonAware.Tools.WattTimeClient
{
    public class WattTimeClient : IWattTimeClient
    {
        private const string BaseUrl = "https://api2.watttime.org/v2/";
        private HttpClient client;
        internal string? authToken;

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private static readonly HttpStatusCode[] RetriableStatusCodes = new HttpStatusCode[]
        {
            HttpStatusCode.Unauthorized, 
            HttpStatusCode.Forbidden
        };

        private IOptionsMonitor<WattTimeClientConfiguration> ConfigurationMonitor { get; }

        private WattTimeClientConfiguration Configuration => this.ConfigurationMonitor.CurrentValue;

        private ActivitySource ActivitySource { get; }

        private ILogger<WattTimeClient> Log { get; }



        public WattTimeClient(HttpClient httpClient, IOptionsMonitor<WattTimeClientConfiguration> configurationMonitor, ILogger<WattTimeClient> log, ActivitySource source)
        {
            this.client = httpClient;
            this.client.BaseAddress = new Uri(BaseUrl);
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            this.ConfigurationMonitor = configurationMonitor;
            this.ActivitySource = source;
            this.Log = log;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(string balancingAuthorityAbbreviation, string startTime, string endTime)
        {
            Log.LogInformation("Requesting grid emission data using start time {startTime} and endTime {endTime}", startTime, endTime);

            var parameters = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthorityAbbreviation },
                { Constants.StartTime, startTime },
                { Constants.EndTime, endTime }
            };

            var tags = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthorityAbbreviation }
            };

            var result = await this.MakeRequestAsync(Paths.Data, parameters, tags);

            return JsonSerializer.Deserialize<List<GridEmissionDataPoint>>(result, options) ?? new List<GridEmissionDataPoint>();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<GridEmissionDataPoint>> GetDataAsync(BalancingAuthority balancingAuthority, string startTime, string endTime)
        {
            return this.GetDataAsync(balancingAuthority.Abbreviation, startTime, endTime);
        }

        /// <inheritdoc/>
        public async Task<Forecast?> GetCurrentForecastAsync(string balancingAuthorityAbbreviation)
        {

            Log.LogInformation("Requesting current forecast from balancing authority {balancingAuthority}", balancingAuthorityAbbreviation);

            var parameters = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthorityAbbreviation }
            };

            var tags = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthorityAbbreviation }
            };

            var result = await this.MakeRequestAsync(Paths.Forecast, parameters, tags);

            return JsonSerializer.Deserialize<Forecast?>(result);
        }

        /// <inheritdoc/>
        public Task<Forecast?> GetCurrentForecastAsync(BalancingAuthority balancingAuthority)
        {
            return this.GetCurrentForecastAsync(balancingAuthority.Abbreviation);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Forecast>> GetForecastByDateAsync(string balancingAuthority, string startTime, string endTime)
        {
            Log.LogInformation("Requesting forecast from balancingAuthority {balancingAuthority} using start time {startTime} and endTime {endTime}", balancingAuthority, startTime, endTime);

            var parameters = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthority },
                { Constants.StartTime, startTime },
                { Constants.EndTime, endTime }
            };

            var tags = new Dictionary<string, string>()
            {
                { Constants.BalancingAuthorityAbbreviation, balancingAuthority }
            };

            var result = await this.MakeRequestAsync(Paths.Forecast, parameters, tags);

            return JsonSerializer.Deserialize<List<Forecast>>(result, options) ?? new List<Forecast>();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Forecast>> GetForecastByDateAsync(BalancingAuthority balancingAuthority, string startTime, string endTime)
        {
            return this.GetForecastByDateAsync(balancingAuthority.Abbreviation, startTime, endTime);
        }

        /// <inheritdoc/>
        public async Task<BalancingAuthority?> GetBalancingAuthorityAsync(string latitude, string longitude)
        {
            Log.LogInformation("Requesting balancing authority for lattitude {lattitude} and longitude {longitude}", latitude, longitude);

            var parameters = new Dictionary<string, string>()
            {
                { Constants.Latitude, latitude },
                { Constants.Longitude, longitude }
            };

            var tags = new Dictionary<string, string>()
            {
                { Constants.Latitude, latitude },
                { Constants.Longitude, longitude }
            };

            var result = await this.MakeRequestAsync(Paths.BalancingAuthorityFromLocation, parameters, tags);

            return JsonSerializer.Deserialize<BalancingAuthority>(result, options);
        }

        /// <inheritdoc/>
        public async Task<string?> GetBalancingAuthorityAbbreviationAsync(string latitude, string longitude)
        {
            return (await this.GetBalancingAuthorityAsync(latitude, longitude))?.Abbreviation;
        }

        private async Task<string> GetAsyncWithAuthRetries(string uriPath, int retries = 1)
        {
            await this.EnsureTokenAsync();

            var response = await this.client.GetAsync(uriPath);

            if (response.IsSuccessStatusCode)
            {
                Log.LogDebug("Successfully retrieved {url}", uriPath);

                var data = response.Content.ReadAsStringAsync();
                return data.Result ?? string.Empty; 
            }
            else if (retries > 0 && RetriableStatusCodes.Contains(response.StatusCode))
            {
                Log.LogDebug("Failed to get url {url} with status code {statusCode}.  {retries} retries remaining.", uriPath, response.StatusCode, retries);
                retries--;
                await this.UpdateAuthTokenAsync();
                return await this.GetAsyncWithAuthRetries(uriPath, retries);
            }
            else
            {
                Log.LogError("Error getting data from WattTime.  StatusCode: {statusCode}. Response: {response}", response.StatusCode, response);

                throw new System.Exception($"Error getting data from WattTime: {response.StatusCode}");
            }
        }

        private async Task EnsureTokenAsync()
        {
            if (this.authToken == null)
            {
                await this.UpdateAuthTokenAsync();
            }
        }

        private async Task UpdateAuthTokenAsync()
        {
            // Request auth token from WattTime API
            var authToken = Encoding.ASCII.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}");
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BasicAuthentication, Convert.ToBase64String(authToken));

            using (var activity = ActivitySource.StartActivity())
            {
                activity?.SetTag(Constants.Username, this.Configuration.Username);

                Log.LogInformation("Attempting to log in user {username}", this.Configuration.Username);

                var result = await this.client.GetStringAsync(Paths.Login);

                // Store token for use with API requests
                var data = JsonSerializer.Deserialize<LoginResult>(result, options);

                if (data == null)
                {
                    Log.LogError("Login failed for user {username}", this.Configuration.Username);
                    throw new AuthenticationException("Login failed.");
                }

                this.authToken = data.Token;
                
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BearerAuthentication, this.authToken);
            }
        }

        private async Task<string> MakeRequestAsync(string path, Dictionary<string, string> parameters, Dictionary<string, string>? tags = null, [CallerMemberName] string activityName = "N/A")
        {
            using (var activity = ActivitySource.StartActivity(activityName))
            {
                var url = BuildUrlWithQueryString(path, parameters);

                Log.LogInformation("Requesting data using url {url}", url);

                if (tags != null)
                {
                    foreach (var kvp in tags)
                    {
                        activity?.AddTag(kvp.Key, kvp.Value);
                    }
                }

                var result = await this.GetAsyncWithAuthRetries(url);

                Log.LogDebug("For query {url}, received data {result}", url, result);

                return result;
            }
        }

        private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
            }

            // this will get a specialized namevalue collection for formatting query strings.
            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach(var kvp in queryStringParams)
            {
                query[kvp.Key] = kvp.Value;
            }

            var result = $"{url}?{query}";

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Built url {result} from url {url} and query string parameters {parameters}", result, url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
            }

            return result;
        }
    }
}
