using System.Collections;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;

namespace CarbonAware.Plugins.JsonReaderPlugin;

public class CarbonAwareJsonReaderPlugin : ICarbonAware
{
    public string Name => "CarbonAwareJsonReaderPlugin";

    public string Description => "Example plugin to read data from a json for Carbon Aware SDK";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    private readonly ILogger<CarbonAwareJsonReaderPlugin> _logger;

    private List<EmissionsData>? emissionsData;


    public CarbonAwareJsonReaderPlugin(ILogger<CarbonAwareJsonReaderPlugin> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        List<EmissionsData>? emissionsData = GetSampleJson();
        if (emissionsData == null) {
            return Enumerable.Empty<EmissionsData>();
        }
        _logger.LogDebug($"Total emission records retrieved {emissionsData.Count}");
        
        return await Task.FromResult(GetFilteredData(emissionsData, props));
    }

    private IEnumerable<EmissionsData> GetFilteredData(IEnumerable<EmissionsData> data, IDictionary props)
    {
        var locations = GetLocationsFromProps(props);
        var startDate = GetStartDateFromProps(props);
        var endDate = GetEndDateFromProps(props);
        
        data = FilterByLocation(data, locations);

        if (endDate != null)
        {
            data = FilterByDateRange(data, startDate, endDate);
        }
        else
        {
            data  = data.Where(ed => ed.Time >= startDate);
        }

        if (data.Count() != 0)
        {
            data.MaxBy(ed => ed.Time);
        }

        return data;
    }

    private IEnumerable<EmissionsData> FilterByDateRange(IEnumerable<EmissionsData> data, DateTime startDate, DateTime? endDate)
    {
        data = data.Where(ed => ed.TimeBetween(startDate, endDate));
        return data;
    }

    private IEnumerable<EmissionsData> FilterByLocation(IEnumerable<EmissionsData> data, IEnumerable<string>? locations)
    {
        if (locations!.Any()) 
        {
            data = data.Where(ed => locations!.Contains(ed.Location));
        }
        return data;
    }

    private DateTime GetStartDateFromProps(IDictionary props)
    {
        var start = props[CarbonAwareConstants.Start];
        var startDate = DateTime.Now;
        if (start != null && !DateTime.TryParse(start.ToString(), out startDate))
        {
            startDate = DateTime.Now;
        }
        return startDate;
    }

    private DateTime? GetEndDateFromProps(IDictionary props)
    {
        DateTime value;
        var end = props[CarbonAwareConstants.End];
        if (end == null || !DateTime.TryParse(end.ToString(), out value))
        {
            return null;
        }
        return value;
    }

    private IEnumerable<string>? GetLocationsFromProps(IDictionary props)
    {
        if (!props.Contains(CarbonAwareConstants.Locations) ||
            props[CarbonAwareConstants.Locations] is null)
        {
            return Enumerable.Empty<string>();
        }
        var locValue = props[CarbonAwareConstants.Locations];
        var type = locValue?.GetType();
        if (type?.GetInterface(nameof(IEnumerable)) != null)
        {
            return locValue as IEnumerable<string>;
        }
        throw new ArgumentException("Invalid location data type. Must be an IEnumerable");
    }

    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }
 
    protected virtual List<EmissionsData>? GetSampleJson()
    {
        if(emissionsData == null || !emissionsData.Any()) {
            var data = ReadFromResource("CarbonAware.Plugins.JsonReaderPlugin.test-data-azure-emissions.json");
            var jsonObject = JsonConvert.DeserializeObject<EmissionsJsonFile>(data);
            emissionsData = jsonObject.Emissions;
        }
        return emissionsData;
    }
}
