using CarbonAware.DataSources.Co2Signal.Configuration;
using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.Co2Signal.Tests;

[TestFixture]
public class Co2SignalClientConfigurationTests
{
    [TestCase("faketoken", "http://example.com", TestName = "Validate does not throw: token; url")]
    public void Validate_DoesNotThrow(string? tokenValue, string? url)
    {
        // Arrange
        var config = new Co2SignalClientConfiguration();
        if (tokenValue != null)
            config.APIToken = tokenValue;
        if (url != null)
            config.BaseUrl = url;

        // Act & Assert
        Assert.DoesNotThrow(() => config.Validate());
    }

    [TestCase("faketoken", "not a url", TestName = "Validate throws: value; bad url")]
    [TestCase(null, "http://example.com", TestName = "Validate throws: no value; url")]
    public void Validate_Throws(string? tokenValue, string? url)
    {
        // Arrange
        var config = new Co2SignalClientConfiguration();
        if (tokenValue != null)
            config.APIToken = tokenValue;
        if (url != null)
            config.BaseUrl = url;

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => config.Validate());
    }
}