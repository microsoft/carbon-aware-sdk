namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Models;
using Moq;
using NUnit.Framework;

public class SerializableEmissionsForecastTests
{
    [Test]
    public void FromEmissionsForecast()
    {
        var expectedGeneratedAt = new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero);
        var expectedStartTime = new DateTimeOffset(2022,1,1,0,1,0,TimeSpan.Zero);
        var expectedEndTime = new DateTimeOffset(2022,1,1,0,2,0,TimeSpan.Zero);
        var expectedLocationName = "test location";
        var expectedWindowSize = 10;
        var expectedOptimalValue = 98.76d;
        var expectedDataPointValue = 123.456d;

        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = expectedGeneratedAt,
            Location = new Location(){ LocationType = LocationType.CloudProvider, RegionName = expectedLocationName },
            StartTime =  expectedStartTime,
            EndTime =  expectedEndTime,
            WindowSize = TimeSpan.FromMinutes(expectedWindowSize),
            ForecastData = new List<EmissionsData>(){ new EmissionsData(){ Rating = expectedDataPointValue } },
            OptimalDataPoint = new EmissionsData(){ Rating = expectedOptimalValue }
        };

        var serializableEmissionsForecast = SerializableEmissionsForecast.FromEmissionsForecast(emissionsForecast);
        var serializedForecastData = serializableEmissionsForecast.ForecastData?.ToList();

        Assert.AreEqual(expectedGeneratedAt, serializableEmissionsForecast.GeneratedAt);
        Assert.AreEqual(expectedLocationName, serializableEmissionsForecast.Location);
        Assert.AreEqual(expectedStartTime, serializableEmissionsForecast.StartTime);
        Assert.AreEqual(expectedEndTime, serializableEmissionsForecast.EndTime);
        Assert.AreEqual(expectedWindowSize, serializableEmissionsForecast.WindowSize);
        Assert.AreEqual(expectedOptimalValue, serializableEmissionsForecast.OptimalDataPoint?.Value);
        Assert.AreEqual(1, serializedForecastData?.Count());
        Assert.AreEqual(expectedDataPointValue, serializedForecastData?.First().Value);
    }
}