using CarbonAware.Interfaces;
using CarbonAware.Model;

namespace CarbonAware.DataSources.NullForecast;

public class NullForecastDataSource : IForecastDataSource
{
    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        return Task.FromResult(new EmissionsForecast());
    }

    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        return Task.FromResult(new EmissionsForecast());
    }
}