using Microsoft.Extensions.Caching.Memory;

namespace CarbonAware.Tools.WattTimeClient;

public class BACache : IBACache
{
    private static readonly int SIZELIMIT = 1024;
    private static readonly int EXPIRATION = 90;
    private MemoryCache Cache { get; set; }

    public BACache()
    {
        Cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = SIZELIMIT
        });
    }

    public void Remove(Tuple<string, string> key)
    {
        Cache.Remove(key);
    }

    public void SetValue<T>(Tuple<string, string> key, T value)
    {
       Cache.Set(key, value, new MemoryCacheEntryOptions()
        .SetSize(1)
        .SetAbsoluteExpiration(TimeSpan.FromSeconds(EXPIRATION))
      );
    }

    public bool TryGetValue<T>(Tuple<string, string> key, out T? value)
    {
        value = default(T);
        if (Cache.TryGetValue(key, out T result))
        {
            value = result;
            return true;
        }
        return false;
    }
}
