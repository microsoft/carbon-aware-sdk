using System.Text.Json;

namespace CarbonAware.Model;

/// <summary>
/// Represents a location.  Note that at least one value must be set.
/// </summary>
public class Location
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the Azure region name.
    /// </summary>
    public string AzureRegionName { get; set; }

    /// <summary>
    /// Gets or sets the AWS Region name.
    /// </summary>
    public string AwsRegionName { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }
}
