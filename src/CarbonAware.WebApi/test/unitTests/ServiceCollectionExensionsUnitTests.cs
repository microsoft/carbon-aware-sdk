using CarbonAware.WebApi.Configuration;
using CarbonAware.WepApi.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CarbonAware.WebApi.UnitTests
{
    /// <summary>
    /// Testing for Service Collection Extension Configurations
    /// </summary>
    [TestFixture]
    public class ServiceCollectionExtensionsUnitTests : TestsBase
    {

        /// <summary>
        /// Tests that the App Insights is set up correctly when the right configuration is given
        /// </summary>
        [Test]
        public async Task TestAppInsightsInstrumentationKey()
        {
            // Arrange
            //    IConfiguration config = new CarbonAwareVariablesConfiguration()
            //     {
            //         VerboseApi = false
            //     }; Mock.

            CarbonAwareVariablesConfiguration carbonAwareVariables = new()
            {
                TelemetryProvider = "ApplicationInsights"
            };

            Mock<IConfiguration> config = new Mock<IConfiguration>();

            config.Setup(c => c.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>()).Returns(carbonAwareVariables);
            Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Key).Returns(CarbonAwareVariablesConfiguration.Key);
            mockSection.Setup(x => x.Get<CarbonAwareVariablesConfiguration>()).Returns(carbonAwareVariables);
            config.Setup(c => c.GetSection(CarbonAwareVariablesConfiguration.Key)).Returns(mockSection.Object);


            // mockSection.SetupGet(x => x[It.Is<string>(s => s == "TelemetryProvider")]).Returns("ApplicationInsights");


            // mockConfig.Setup(x=>x.GetSection(It.Is<string>(k=>k==CarbonAwareVariablesConfiguration.Key))).Returns(mockSection.Object);
            // config.SetupGet(x => x[It.Is<string>(s => s == "CarbonAwareVars__TelemetryProvider")]).Returns("ApplicationInsights");
            // config.SetupGet(x => x[It.Is<string>(s => s == "Connection_String")]).Returns("On");
            // Act
            ServiceCollectionExtensions.AddMonitoringAndTelemetry(new Mock<IServiceCollection>().Object, config.Object);
            // Assert
        }
    }
}