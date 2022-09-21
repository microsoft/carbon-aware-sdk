using System.Reflection;
using System.Text.RegularExpressions;

namespace CarbonAware.LocationSources.Configuration;

/// <summary>
/// A configuration class for holding Location Data config values.
/// </summary>
public class LocationDataSourcesConfiguration
{

    public const string Key = "LocationDataSourcesConfiguration";

    public List<LocationDataSource>? LocationDataSources { get; set; }

}

public class LocationDataSource
{

    private const string DefaultAzureLocationDataFile = "azure-regions.json";
    private const string BaseDirectory = "location-sources/json";
    private const string DirectoryRegExPattern = @"^[-\\/a-zA-Z_\d ]*$";
    private const char DefaultDelimiter = '-';

    private string assemblyDirectory;
    private string? dataFileLocation;

    /// <summary>
    /// Location data file location
    /// </summary>
    public string DataFileLocation
    {
        get => dataFileLocation!;
        set
        {
            if (!IsValidDirPath(value))
            {
                throw new ArgumentException($"File path '{value}' contains not supported characters.");
            }
            dataFileLocation = Path.Combine(assemblyDirectory, BaseDirectory, value);
        }
    }

    public string? Prefix { get ; set; }
    public char? Delimiter { get;  set; }

    public LocationDataSource()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
        Delimiter = DefaultDelimiter;
    }

    private static bool IsValidDirPath(string fileName)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            return false;
        }
        var dirName = Path.GetDirectoryName(fileName);
        if (dirName is null)
        {
            return false;
        }
        var match = Regex.Match(dirName, DirectoryRegExPattern);
        return match.Success;
    }

    public static LocationDataSource GetDefaultAzureDataSourceLocation()
    {
        return new LocationDataSource() {
            DataFileLocation = DefaultAzureLocationDataFile
        };
    }
}
