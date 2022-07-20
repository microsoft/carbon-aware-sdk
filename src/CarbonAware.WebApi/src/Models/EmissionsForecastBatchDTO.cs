namespace CarbonAware.WebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBatchDTO
{

  /// <summary>The location of the forecast</summary>
  /// <example>eastus</example>
  [JsonPropertyName("location")]
  [Required()]
  public string Location { get; set; } = string.Empty;

  /// <summary>The historical time used to fetch the most recent forecast as of that time.</summary>
  /// <example>2022-06-01T00:00:00Z</example>
  [JsonPropertyName("requestedAt")]
  [Required()]
  public DateTimeOffset RequestedAt { get; set; }

  /// <summary>
  /// Filter start time boundary of forecasted data points. Ignores forecast data points before this time.
  /// Defaults to the earliest time in the forecast data.
  /// </summary>
  /// <example>2022-06-01T14:40:00Z</example>
  [JsonPropertyName("dataStartAt")]
  public DateTimeOffset DataStartAt { get; set; }

  /// <summary>
  /// Filter end time boundary of forecasted data points. Ignores forecast data points after this time.
  /// Defaults to the latest time in the forecast data.
  /// </summary>
  /// <example>2022-06-01T14:50:00Z</example>
  [JsonPropertyName("dataEndAt")]
  public DateTimeOffset DataEndAt { get; set; }

  /// <summary>
  /// The estimated duration (in minutes) of the workload.
  /// Defaults to the duration of a single forecast data point.
  /// </summary>
  /// <example>30</example>
  [JsonPropertyName("windowSize")]
  public int WindowSize { get; set; }
}
