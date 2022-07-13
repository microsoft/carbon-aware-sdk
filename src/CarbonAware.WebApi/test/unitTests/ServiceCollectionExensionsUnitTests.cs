using CarbonAware.WebApi.Configuration;
using CarbonAware.WepApi.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CarbonAware.WebApi.UnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Tests that the Web API controller handles and packages various responses from a plugin properly 
    /// including empty responses and exceptions.
    /// </summary>
    [TestFixture]
    public class ServiceCollectionExtensionsUnitTests : TestsBase
    {

        /// <summary>
        /// Tests that invalid locationType inputs respond with a badRequest error
        /// </summary>
        [Test]
        public async Task TestAppInsightsInstrumentationKey()
        {
            // Arrange
            //    IConfiguration config = new CarbonAwareVariablesConfiguration()
            //     {
            //         VerboseApi = false
            //     }; Mock.

            CarbonAwareVariablesConfiguration carbonAwareVariables = new CarbonAwareVariablesConfiguration()
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