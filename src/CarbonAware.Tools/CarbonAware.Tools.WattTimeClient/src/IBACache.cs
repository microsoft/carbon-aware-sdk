namespace CarbonAware.Tools.WattTimeClient;

public interface IBACache
{
    bool TryGetValue<T>(Tuple<string, string> key, out T? value);
    void SetValue<T>(Tuple<string, string> key, T value);
    void Remove(Tuple<string, string> key);
}