using CarbonAware.Aggregators.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CarbonAware.WebApi.Models;

public class EmissionsDataForLocationsParametersDTO : CarbonAwareParametersBaseDTO
{
    /// <summary>String array of named locations</summary>
    /// <example>eastus</example>
    [BindProperty(Name = "location"), BindRequired] override public string[]? MultipleLocations { get; set; }
    /// <summary>[Optional] Start time for the data query.</summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [BindProperty(Name = "time")] override public DateTimeOffset? Start { get; set; }
    /// <summary>[Optional] End time for the data query.</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [BindProperty(Name = "toTime")] override public DateTimeOffset? End { get; set; }
}