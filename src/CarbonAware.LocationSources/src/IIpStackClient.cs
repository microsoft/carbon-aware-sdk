namespace CarbonAware.LocationSources;

/// <summary>
/// An interface for interacting with ipstack.com
/// </summary>
public interface IIpStackClient
{
    public const string NamedClient = "IpStackClient";

    public Task<string> GetData();

}
