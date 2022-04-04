namespace CarbonAware.WepApi.UnitTests
{
    using CarbonAware.Model;
    using CarbonAware.WebApi.Controllers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// TestsBase for all WebAPI specific tests.
    /// </summary>
    public abstract class TestsBase
    {
        protected TestsBase()
        {
            this.MockLogger = new Mock<ILogger<CarbonAwareController>>();
            this.MockPlugin = new Mock<ICarbonAware>();
        }

        protected Mock<ILogger<CarbonAwareController>> MockLogger { get; }
        protected Mock<ICarbonAware> MockPlugin { get; }

        protected void SetupPluginWithData(List<EmissionsData> data) =>
            this.MockPlugin.Setup(x =>
                x.GetEmissionsDataAsync(
                    It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);

        protected void SetupPluginWithException() =>
            this.MockPlugin.Setup(x =>
                x.GetEmissionsDataAsync(
                    It.IsAny<Dictionary<string, object>>())).Throws<Exception>();
    }
}