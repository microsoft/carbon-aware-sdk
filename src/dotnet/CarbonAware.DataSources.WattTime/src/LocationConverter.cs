using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient.Model;

namespace CarbonAware.DataSources.WattTime;

/// <inheritdoc />
public class LocationConverter : ILocationConverter
{
    /// <inheritdoc />
    public Task<BalancingAuthority> ConvertLocationToBalancingAuthorityAsync(Location location)
    {
        // validate the location object to make sure something is set.
        // Based on what is set, determine which lat/long converter will need to be used.
        // convert to lat/long
        // call watttime client with lat/long to get ba
        // if no ba, throw LocationConvertsionException
        // return BA.
        var ba = new BalancingAuthority {
            Name = "California ISO Northern",
            Abbreviation = "CAISO_NORTH",
            Id = 1
        };
        return Task.FromResult(ba);
    }
}
