namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Models;
using NUnit.Framework;

public class SerializableEmissionsDataTests
{
    [Test]
    public void FromEmissionsData()
    {
        var expectedLocationName = "test location";
        var expectedTimestamp = new DateTimeOffset(2022,1,1,0,0,0, TimeSpan.Zero);
        var expectedDuration = 120;
        var expectedValue = 123.45;
        var emissionsData = new EmissionsData()
        {
            Location = expectedLocationName,
            Time = expectedTimestamp,
            Duration = TimeSpan.FromMinutes(expectedDuration),
            Rating = expectedValue
        };

        var serializableEmissionsData = SerializableEmissionsData.FromEmissionsData(emissionsData);

        Assert.AreEqual(expectedLocationName, serializableEmissionsData.Location);
        Assert.AreEqual(expectedTimestamp, serializableEmissionsData.Timestamp);
        Assert.AreEqual(expectedDuration, serializableEmissionsData.Duration);
        Assert.AreEqual(expectedValue, serializableEmissionsData.Value);
    }
}