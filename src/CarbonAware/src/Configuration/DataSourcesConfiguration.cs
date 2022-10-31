using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;
public class DataSourcesConfiguration
{
    public const string Key = "DataSources";

    #nullable enable
    public string? EmissionsDataSource { get; set; }
    public string? ForecastDataSource { get; set; }
    public IConfigurationSection? ConfigurationSection { get; set; }
    #nullable disable

    public string EmissionsConfigurationType()
    {
        return GetConfigurationType(EmissionsDataSource);
    }

    public string ForecastConfigurationType()
    {
        return GetConfigurationType(ForecastDataSource);
    }

    public IConfigurationSection EmissionsConfigurationSection()
    {
        return GetConfigurationSection(EmissionsDataSource);
    }

    public IConfigurationSection ForecastConfiguration()
    {
        return GetConfigurationSection(ForecastDataSource);
    }

    public void AssertValid()
    {
        if (string.IsNullOrEmpty(EmissionsDataSource) && string.IsNullOrEmpty(ForecastDataSource))
        {
            throw new ArgumentException("At least one data source must be specified in configuration");
        }

        if (!string.IsNullOrEmpty(EmissionsDataSource) && !ConfigurationSectionContainsKey(EmissionsDataSource))
        {
            throw new ArgumentException($"Emissions data source value '{EmissionsDataSource}' was not found in 'Configurations'");
        }

        if (!string.IsNullOrEmpty(ForecastDataSource) && !ConfigurationSectionContainsKey(ForecastDataSource))
        {
            throw new ArgumentException($"Forecast data source value '{ForecastDataSource}' was not found in 'Configurations'");
        }
    }

    private string GetConfigurationType(string dataSourceName)
    {
        return ConfigurationSection.GetValue<string>($"{dataSourceName}:Type");   
    }

    private IConfigurationSection GetConfigurationSection(string dataSourceName)
    {
        return ConfigurationSection.GetSection(dataSourceName);
    }

    private bool ConfigurationSectionContainsKey(string key)
    {
        foreach (var subsection in ConfigurationSection.GetChildren())
        {
            // TODO what string comparison do we want?  ignore case?
            if (subsection.Key == key)
            {
                return true;
            }
        }
        return false;
    }
}
