﻿using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonAware.Tools.WatTimeClient.Tests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class WattTimeClientTests
    {
        private MockHttpMessageHandler MessageHandler { get; set; }

        private HttpClient HttpClient { get; set; }

        private WattTimeClientConfiguration Configuration { get; set; }

        private Mock<IOptionsMonitor<WattTimeClientConfiguration>> Options { get; set; }

        private Mock<ILogger<WattTimeClient.WattTimeClient>> Log { get; set; }

        private ActivitySource ActivitySource { get; set; }

        private string BasicAuthValue { get; set; }

        private readonly string DefaultTokenValue = "myDefaultToken123";

        [SetUp]
        public void Initialize()
        {
            this.Configuration = new WattTimeClientConfiguration() { Username = "username", Password = "password" };

            this.Options = new Mock<IOptionsMonitor<WattTimeClientConfiguration>>();
            this.Log = new Mock<ILogger<WattTimeClient.WattTimeClient>>();

            this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

            this.ActivitySource = new ActivitySource("CarbonAware.Tools.WattTimeClient.Tests");

            this.BasicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}"));
        }

        [Test]
        public void GetDataAsync_ThrowsWhenBadJsonIsReturned()
        {
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync("ba", "start", "end"));
        }

        [Test]
        public async Task GetDataAsync_DeserializesExpectedResponse()
        {
            this.CreateHttpClient(m =>
            {
                Assert.AreEqual("https://api2.watttime.org/v2/data?ba=balauth&starttime=start&endtime=end", m.RequestUri?.ToString());
                Assert.AreEqual(HttpMethod.Get, m.Method);
                var response = this.MockWattTimeAuthResponse(m, new StringContent("[" +
                    "{\"ba\":\"ba\",\"datatype\":\"dt\",\"frequency\": 300,\"market\":\"mkt\"," +
                    "\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]"));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var data = await client.GetDataAsync("balauth", "start", "end");

            Assert.IsTrue(data.Count() > 0);
            var gridDataPoint = data.ToList().First();
            Assert.AreEqual("ba", gridDataPoint.BalancingAuthority);
            Assert.AreEqual("dt", gridDataPoint.Datatype);
            Assert.AreEqual(300, gridDataPoint.Frequency);
            Assert.AreEqual("mkt", gridDataPoint.Market);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), gridDataPoint.PointTime);
            Assert.AreEqual("999.99", gridDataPoint.Value.ToString("0.00")); //Format float to avoid precision issues
            Assert.AreEqual("1.0", gridDataPoint.Version);
        }

        [Test]
        public async Task GetDataAsync_RefreshesTokenWhenExpired()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("[" +
                        "{\"ba\":\"ba\",\"datatype\":\"dt\",\"frequency\": 300,\"market\":\"mkt\"," +
                        "\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                        "]");

                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var data = await client.GetDataAsync("balauth", "start", "end");
           
            Assert.IsTrue(data.Count() > 0);
            var gridDataPoint = data.ToList().First();
            Assert.AreEqual("ba", gridDataPoint.BalancingAuthority);
        }

        [Test]
        public async Task GetDataAsync_RefreshesTokenWhenNoneSet()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("[" +
                        "{\"ba\":\"ba\",\"datatype\":\"dt\",\"frequency\": 300,\"market\":\"mkt\"," +
                        "\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                        "]");

                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);

            var data = await client.GetDataAsync("balauth", "start", "end");

            Assert.IsTrue(data.Count() > 0);
            var gridDataPoint = data.ToList().First();
            Assert.AreEqual("ba", gridDataPoint.BalancingAuthority);
        }

        [Test]
        public void GetCurrentForecastAsync_ThrowsWhenBadJsonIsReturned()
        {
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync("balauth"));
        }

        [Test]
        public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
        {
            this.CreateHttpClient(m =>
            {
                Assert.AreEqual("https://api2.watttime.org/v2/forecast?ba=balauth", m.RequestUri?.ToString());
                Assert.AreEqual(HttpMethod.Get, m.Method);
                var response = this.MockWattTimeAuthResponse(m, new StringContent("{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}"));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var forecast = await client.GetCurrentForecastAsync("balauth");

            Assert.IsNotNull(forecast);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast?.GeneratedAt);
            var forecastDataPoint = forecast?.ForecastData.First();
            Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthority);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecastDataPoint?.PointTime);
            Assert.AreEqual("999.99", forecastDataPoint?.Value.ToString("0.00")); //Format float to avoid precision issues
            Assert.AreEqual("1.0", forecastDataPoint?.Version);
        }

        [Test]
        public async Task GetCurrentForecastAsync_RefreshesTokenWhenExpired()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var forecast = await client.GetCurrentForecastAsync("balauth");

            Assert.IsNotNull(forecast);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast?.GeneratedAt);
            var forecastDataPoint = forecast?.ForecastData.First();
            Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthority);
        }

        [Test]
        public async Task GetCurrentForecastAsync_RefreshesTokenWhenNoneSet()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);

            this.HttpClient.DefaultRequestHeaders.Authorization = null;

            var forecast = await client.GetCurrentForecastAsync("balauth");

            Assert.IsNotNull(forecast);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast?.GeneratedAt);
            var forecastDataPoint = forecast?.ForecastData.First();
            Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthority);
        }

        [Test]
        public void GetForecastByDateAsync_ThrowsWhenBadJsonIsReturned()
        {
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastByDateAsync("balauth","start","end"));
        }

        [Test]
        public async Task GetForecastByDateAsync_DeserializesExpectedResponse()
        {
            this.CreateHttpClient(m =>
            {
                Assert.AreEqual("https://api2.watttime.org/v2/forecast?ba=balauth&starttime=start&endtime=end", m.RequestUri?.ToString());
                Assert.AreEqual(HttpMethod.Get, m.Method);
                var response = this.MockWattTimeAuthResponse(m, new StringContent("[{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}]"));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var forecasts = await client.GetForecastByDateAsync("balauth", "start", "end");

            Assert.IsTrue(forecasts.Count() > 0);
            var forecast = forecasts.ToList().First();
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast.GeneratedAt);
            var forecastDataPoint = forecast.ForecastData.ToList().First();
            Assert.AreEqual("ba", forecastDataPoint.BalancingAuthority);
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecastDataPoint.PointTime);
            Assert.AreEqual("999.99", forecastDataPoint.Value.ToString("0.00")); //Format float to avoid precision issues
            Assert.AreEqual("1.0", forecastDataPoint.Version);
        }

        [Test]
        public async Task GetForecastByDateAsync_RefreshesTokenWhenExpired()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("[{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}]");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var forecasts = await client.GetForecastByDateAsync("balauth", "start", "end");

            Assert.IsTrue(forecasts.Count() > 0);
            var forecast = forecasts.ToList().First();
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast.GeneratedAt);
        }

        [Test]
        public async Task GetForecastByDateAsync_RefreshesTokenWhenNoneSet()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent("[{\"generated_at\":\"2099-01-01T00:00:00Z\",\"forecast\":[" +
                    "{\"ba\":\"ba\",\"point_time\":\"2099-01-01T00:00:00Z\",\"value\":999.99,\"version\":\"1.0\"}" +
                    "]}]");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);

            this.HttpClient.DefaultRequestHeaders.Authorization = null;

            var forecasts = await client.GetForecastByDateAsync("balauth", "start", "end");
            
            Assert.IsTrue(forecasts.Count() > 0);
            var forecast = forecasts.ToList().First();
            Assert.AreEqual(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc), forecast.GeneratedAt);
        }

        [Test]
        public void GetBalancingAuthorityAsync_ThrowsWhenBadJsonIsReturned()
        {
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            Assert.ThrowsAsync<JsonException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
        }

        [Test]
        public async Task GetBalancingAuthorityAsync_DeserializesExpectedResponse()
        {
            this.CreateHttpClient(m =>
            {
                Assert.AreEqual("https://api2.watttime.org/v2/ba-from-loc?latitude=lat&longitude=long", m.RequestUri?.ToString());
                Assert.AreEqual(HttpMethod.Get, m.Method);
                var response = this.MockWattTimeAuthResponse(m, new StringContent(
                    "{\"id\":\"12345\",\"abbrev\":\"TEST_BA\",\"name\":\"Test Balancing Authority\"}"));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var ba = await client.GetBalancingAuthorityAsync("lat", "long");

            Assert.IsNotNull(ba);
            Assert.AreEqual(12345, ba?.Id);
            Assert.AreEqual("TEST_BA", ba?.Abbreviation);
            Assert.AreEqual("Test Balancing Authority", ba?.Name);
        }

        [Test]
        public async Task GetBalancingAuthorityAsync_RefreshesTokenWhenExpired()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent(
                    "{\"id\":\"12345\",\"abbrev\":\"TEST_BA\",\"name\":\"Test Balancing Authority\"}");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var ba = await client.GetBalancingAuthorityAsync("lat", "long");

            Assert.IsNotNull(ba);
            Assert.AreEqual(12345, ba?.Id);
        }

        [Test]
        public async Task GetBalancingAuthorityAsync_RefreshesTokenWhenNoneSet()
        {
            this.CreateHttpClient(m =>
            {
                var content = new StringContent(
                    "{\"id\":\"12345\",\"abbrev\":\"TEST_BA\",\"name\":\"Test Balancing Authority\"}");
                var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);

            this.HttpClient.DefaultRequestHeaders.Authorization = null;

            var ba = await client.GetBalancingAuthorityAsync("lat", "long");

            Assert.IsNotNull(ba);
            Assert.AreEqual(12345, ba?.Id);
        }

        [Test]
        public async Task GetHistoricalDataAsync_StreamsExpectedContent()
        {
            using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
            {
                this.CreateHttpClient(m =>
                {
                    var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream));
                    return Task.FromResult(response);
                });

                var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
                client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

                var result = await client.GetHistoricalDataAsync("ba");
                var sr = new StreamReader(result);
                string streamResult = sr.ReadToEnd();

                Assert.AreEqual("myStreamResults", streamResult);
            }
        }

        [Test]
        public async Task GetHistoricalDataAsync_RefreshesTokenWhenExpired()
        {
            using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
            {
                this.CreateHttpClient(m =>
                {
                    var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream), "REFRESHTOKEN");
                    return Task.FromResult(response);
                });

                this.HttpClient.DefaultRequestHeaders.Authorization = null;
                var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);

                var result = await client.GetHistoricalDataAsync("ba");
                var sr = new StreamReader(result);
                string streamResult = sr.ReadToEnd();

                Assert.AreEqual("myStreamResults", streamResult);
            }
        }

        [Test]
        public async Task GetHistoricalDataAsync_RefreshesTokenWhenNoneSet()
        {
            using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
            {
                this.CreateHttpClient(m =>
                {
                    var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream), "REFRESHTOKEN");
                    return Task.FromResult(response);
                });

                var client = new WattTimeClient.WattTimeClient(this.HttpClient, this.Options.Object, this.Log.Object, this.ActivitySource);
                client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

                var result = await client.GetHistoricalDataAsync("ba");
                var sr = new StreamReader(result);
                string streamResult = sr.ReadToEnd();

                Assert.AreEqual("myStreamResults", streamResult);
            }
        }

        private void CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
        {
            this.MessageHandler = new MockHttpMessageHandler(requestDelegate);
            this.HttpClient = new HttpClient(this.MessageHandler);
            this.HttpClient.BaseAddress = new Uri("https://api2.watttime.org/v2/");
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.DefaultTokenValue);
        }

        private HttpResponseMessage MockWattTimeAuthResponse(HttpRequestMessage request, HttpContent reponseContent, string? validToken = null)
        {
            if (validToken == null)
            {
                validToken = this.DefaultTokenValue;
            }
            var auth = this.HttpClient.DefaultRequestHeaders.Authorization;
            if (auth == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            var authHeader = auth.ToString();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            if (request.RequestUri == new Uri("https://api2.watttime.org/v2/login"))
            {
                Assert.AreEqual($"Basic {this.BasicAuthValue}", authHeader);
                response.Content = new StringContent("{\"token\":\""+validToken+"\"}");
            }
            else if (authHeader == $"Bearer {validToken}")
            {
                response.Content = reponseContent;
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.Forbidden;
            }
            return response;
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private Func<HttpRequestMessage, Task<HttpResponseMessage>> RequestDelegate { get; set; }

            public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
            {
                this.RequestDelegate = requestDelegate;
            }
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await this.RequestDelegate.Invoke(request);
            }
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}