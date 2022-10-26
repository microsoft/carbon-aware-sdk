using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Tests.Configuration;
class Dummy
{
    public string? Name { get; set; } 
}
class DataSourcesConfigurationTests
{
    [Test]
    public void AssertValid_NoDataSourceConfigured_ThrowsException()
    {
        DataSourcesConfiguration configuration = new DataSourcesConfiguration();
        var ex = Assert.Throws<ArgumentException>(() => configuration.AssertValid());
        Assert.That(ex!.Message, Contains.Substring("At least one data source"));
    }

    [TestCase("WattTime", "", TestName = "AssertValid: Emissions valid, Forecast empty")]
    [TestCase("WattTime", null, TestName = "AssertValid: Emissions valid, Forecast null")]
    [TestCase("WattTime", "WattTime", TestName = "AssertValid: Emissions valid, Forecast valid")]
    [TestCase("", "WattTime", TestName = "AssertValid: Forecast valid, Emissions empty")]
    [TestCase(null, "WattTime", TestName = "AssertValid: Forecast valid, Emissions null")]

    public void AssertValid_WithAtleastOneDataSourceConfigured_DoesNotThrowException(string? emissionsDataSource, string? forecastDataSource)
    {
        //Arrange
        var inMemorySettings = new Dictionary<string, string>() {{
            "Configurations:WattTime", "" 
        }};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = emissionsDataSource,
            ForecastDataSource = forecastDataSource,
            ConfigurationSection = configuration.GetSection("Configurations")
        };
        
        Assert.DoesNotThrow(() => dataSourceConfig.AssertValid());
    }

    [TestCase("WattTime","badkey","Emissions data source", TestName = "AssertValid: EmissionsDataSourceNotInConfigurationSection") ]
    [TestCase("badkey", "WattTime", "Forecast data source", TestName = "AssertValid: ForecastDataSourceNotInConfigurationSection")]

    public void AssertValid_DataSourceNotInConfigurationSection_ThrowsException(string forecastDataSource, string emissionsDataSource, string errorMessage)
    {
        //Arrange
        var inMemorySettings = new Dictionary<string, string>() {{
            "Configurations:WattTime", ""
        }};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = emissionsDataSource,
            ForecastDataSource = forecastDataSource,
            ConfigurationSection = configuration.GetSection("Configurations")
        };
        var ex = Assert.Throws<ArgumentException>(() => dataSourceConfig.AssertValid());
        Assert.That(ex!.Message, Contains.Substring(errorMessage));
    }

    [Test]
    public void EmissionsConfiguration_ReturnsGenericClassWithExpectedValues()
    {
        //Arrange
        var expectedResult = "abcd";
        var inMemorySettings = new Dictionary<string, string>() {
            {"Configurations:Emissions:Name", expectedResult },
            {"Configurations:Forecast:Name", "xyz" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = "Emissions",
            ForecastDataSource = "Forecast",
            ConfigurationSection = configuration.GetSection("Configurations")
        };
        var result = dataSourceConfig.EmissionsConfiguration<Dummy>();
        
        Assert.AreEqual(expectedResult, result.Name);
    }

    [Test]
    public void ForecastConfiguration_ReturnsGenericClassWithExpectedValues()
    {
        //Arrange
        var expectedResult = "abcd";
        var inMemorySettings = new Dictionary<string, string>() {
            {"Configurations:Emissions:Name", "xyz" },
            {"Configurations:Forecast:Name", expectedResult }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        DataSourcesConfiguration dataSourceConfig = new()
        {
            EmissionsDataSource = "Emissions",
            ForecastDataSource = "Forecast",
            ConfigurationSection = configuration.GetSection("Configurations")
        };
        var result = dataSourceConfig.ForecastConfiguration<Dummy>();

        Assert.AreEqual(expectedResult, result.Name);
    }
}
