using System.Net;
using System.Text;
using CarbonAware.Tools.WattTimeClient.Constants;
using CarbonAware.Tools.WattTimeClient.Model;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Text.Json.Nodes;

namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// A utilities static class for Watt Time
    /// </summary>
    public static class WattTimeServerMocks
    {
        /// <summary>
        /// Setup the mock server for watttime calls
        /// </summary>
        public static void SetupWattTimeServerMocks(this WireMockServer server)
        {
            SetupDataMock(server);
            SetupForecastMock(server);
            SetupBaMock(server);
            SetupLoginMock(server);
            SetupHistoricalMock(server);
        }
        /// <summary>
        /// Setup data calls on mock server
        /// </summary>
        public static void SetupDataMock(WireMockServer server)
        {
            var json = new JsonArray(
              new JsonObject
              {
                  ["ba"] = "ba",
                  ["datatype"] = "dt",
                  ["frequency"] = 300,
                  ["market"] = "mkt",
                  ["point_time"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                  ["value"] = 999.99,
                  ["version"] = "1.0"
              }
            );
            server
                .Given(Request.Create().WithPath("/" + Paths.Data).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(json.ToString())
                    );
        }

        /// <summary>
        /// Setup forecast calls on mock server
        /// </summary>
        private static void SetupForecastMock(WireMockServer server)
        {
            var json = new JsonArray
            {
                new JsonObject
                {
                    ["generated_at"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    ["forecast"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["ba"] = "ba",
                            ["point_time"] = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero),
                            ["value"] = 999.99,
                            ["version"] = "1.0"
                        }
                    }
                }
            };

            server
                .Given(Request.Create().WithPath("/" + Paths.Forecast).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(json.ToString())
                    );
        }
        /// <summary>
        /// Setup balancing authority calls on mock server
        /// </summary>
        private static void SetupBaMock(WireMockServer server)
        {
            var json = new JsonObject
            {
                ["id"] = "12345",
                ["abbrev"] = "TEST_BA",
                ["name"] = "Test Balancing Authority"
            };
            server
                .Given(Request.Create().WithPath("/" + Paths.BalancingAuthorityFromLocation).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(json.ToString())
                    );
        }

        /// <summary>
        /// Setup logins calls on mock server
        /// </summary>
        private static void SetupLoginMock(WireMockServer server)
        {
            server
                .Given(Request.Create().WithPath("/" + Paths.Login).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(JsonSerializer.Serialize(new LoginResult { Token = "myDefaultToken123" }))
                    );
        }

        /// <summary>
        /// Setup historical calls on mock server
        /// </summary>
        private static void SetupHistoricalMock(WireMockServer server)
        {
            server
                .Given(Request.Create().WithPath("/" + Paths.Historical).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/zip")
                        .WithBody(new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")).ToString() ?? string.Empty)
                    );
        }
    }
}
