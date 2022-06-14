using System.Net;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net.Mime;
using CarbonAware.Tools.WattTimeClient.Model;
using CarbonAware.Tools.WattTimeClient.Constants;

namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// A utilities static class for Watt Time
    /// </summary>
    public static class WattTimeServerMocks
    {
        private static readonly DateTimeOffset testDataPointOffset = new (2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly string testBA = "TEST_BA";

        /// <summary>
        /// Setup the mock server for watttime calls
        /// </summary>
        public static void SetupWattTimeServerMocks(this WireMockServer server, List<GridEmissionDataPoint>? dataMock = null, List<Forecast>? forecastMock = null, BalancingAuthority? baMock = null, LoginResult? loginMock = null)
        {
            SetupDataMock(server, dataMock);
            SetupForecastMock(server, forecastMock);
            SetupBaMock(server, baMock);
            SetupLoginMock(server, loginMock);
        }

        public static DateTimeOffset GetTestDataPointOffset() => testDataPointOffset;

        public static void SetupResponseGivenGetRequest(this WireMockServer server, string path, HttpStatusCode statusCode, string contentType, string body) =>
            server
                .Given(Request.Create().WithPath("/" + path).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );

        /// <summary>
        /// Setup data calls on mock server
        /// </summary>
        public static void SetupDataMock(WireMockServer server, List<GridEmissionDataPoint>? content = null)
        {
            var data = content ?? new List<GridEmissionDataPoint>() {
                new GridEmissionDataPoint(){
                  BalancingAuthorityAbbreviation= testBA,
                  Datatype = "dt",
                  Frequency = 300,
                  Market = "mkt",
                  PointTime = testDataPointOffset,
                  Value = 999.99F,
                  Version = "1.0"
                }
            };
            server.SetupResponseGivenGetRequest(Paths.Data, HttpStatusCode.OK, MediaTypeNames.Application.Json, JsonSerializer.Serialize(data));
        }

        /// <summary>
        /// Setup forecast calls on mock server
        /// </summary>
        private static void SetupForecastMock(WireMockServer server, List<Forecast>? content = null)
        {
            var forecastList = content ?? new List<Forecast>()
            {
                new Forecast()
                {
                    GeneratedAt = testDataPointOffset,
                    ForecastData = new List<GridEmissionDataPoint>()
                    {
                        new GridEmissionDataPoint()
                        {
                            BalancingAuthorityAbbreviation = testBA,
                            PointTime = testDataPointOffset,
                            Value = 999.99F,
                            Version = "1.0"
                        }
                    }
                }
            };
            server.SetupResponseGivenGetRequest(Paths.Forecast, HttpStatusCode.OK, MediaTypeNames.Application.Json, JsonSerializer.Serialize(forecastList));
        }
        /// <summary>
        /// Setup balancing authority calls on mock server
        /// </summary>
        private static void SetupBaMock(WireMockServer server, BalancingAuthority? content = null)
        {
            var ba = content ?? new BalancingAuthority()
            {
                Id = 12345,
                Abbreviation = testBA,
                Name = "Test Balancing Authority"
            };
            server.SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, HttpStatusCode.OK, MediaTypeNames.Application.Json, JsonSerializer.Serialize(ba));
        }

        /// <summary>
        /// Setup logins calls on mock server
        /// </summary>
        private static void SetupLoginMock(WireMockServer server, LoginResult? content = null)
        {
            var login = content ?? new LoginResult { Token = "myDefaultToken123" };
            server.SetupResponseGivenGetRequest(Paths.Login, HttpStatusCode.OK, MediaTypeNames.Application.Json, JsonSerializer.Serialize(login));
        }
    }
}
