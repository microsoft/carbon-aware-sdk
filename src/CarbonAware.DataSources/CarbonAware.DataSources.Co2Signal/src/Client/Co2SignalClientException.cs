using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.Co2Signal.Client;

public class Co2SignalClientException : CarbonAwareException
{
    public Co2SignalClientException(string message) : base(message)
    {
    }
}