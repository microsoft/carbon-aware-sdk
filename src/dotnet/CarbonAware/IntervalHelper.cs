namespace CarbonAware;

/// <summary>
/// Helper for intervals within Carbon Aware.
/// </summary>
public static class IntervalHelper
{

    /// <summary>
    /// Ensures the data is available between the range of time considering the duration of the data
    /// </summary>
    /// <param name="expandedData">Expanded data that includes the minimum sampling window</param>
    /// <param name="startDate">Original start date provided by user</param>
    /// <param name="endDate">Original end date provided by user</param>
    /// <returns>Filtered emissions data.</returns>
    public static IEnumerable<EmissionsData> MinSamplingFiltering(IEnumerable<EmissionsData> expandedData, DateTimeOffset startDate, DateTimeOffset endDate, TimeSpan duration = default)
    {
        if (duration != default)
        {   // constant duration
            return expandedData.Where(d => (d.Time + duration) >= startDate && d.Time <= endDate);
        }
        return expandedData.Where(d => (d.Time + d.Duration) >= startDate && d.Time <= endDate);
    }

    /// <summary>
    /// Substract and Add minutes to start and end dates
    /// </summary>
    /// <param name="orgStartDate">Original Start Date to shift</param>
    /// <param name="orgEndDate">Original End Date to shift</param>
    /// <param name="minutesValue">Minutes to add and substract</param>
    /// <returns>Shifted dates</returns>
    public static (DateTimeOffset, DateTimeOffset) ShiftDate(DateTimeOffset orgStartDate, DateTimeOffset orgEndDate, double minutesValue)
    {
        if (orgEndDate.Subtract(orgStartDate) < TimeSpan.FromMinutes(minutesValue))
        {
            return (orgStartDate.AddMinutes(-minutesValue), orgEndDate.AddMinutes(minutesValue));
        }
        return (orgStartDate, orgEndDate);
    }
}
