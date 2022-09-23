using System.Reflection;
using System.Text.RegularExpressions;

namespace CarbonAware.LocationSources.Configuration;

public class LocationSourceFile
{

    private const string DefaultLocationDataFile = "azure-regions.json";
    private const string BaseDirectory = "location-sources/json";
    private const string DirectoryRegExPattern = @"^[-\\/a-zA-Z_\d ]*$";

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

    public string Prefix { get ; set; } = String.Empty;
    public string Delimiter { get;  set; } = String.Empty;

    public LocationSourceFile()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
        DataFileLocation = DefaultLocationDataFile;
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
}
