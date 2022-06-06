namespace CarbonAware;

/// <summary>
/// Helper for intervals within Carbon Aware.
/// </summary>
public static class IntervalHelper
{

    /// <summary>
    /// Ensures the data is valid by using the min sampling window. Uses the expanded data to add an extra
    /// data point before the startDate to ensure all expected data is included.
    /// </summary>
    /// <param name="data">Expanded data that includes the minimum sampling window</param>
    /// <param name="startDate">Original start date provided by user</param>
    /// <param name="endDate">Original end date provided by user</param>
    /// <returns>Filtered emissions data.</returns>
    /// <remarks> If the user provided interval is already >= minimum sampling window, or no expanded data, we just return the passed data. </remarks>
    public static IEnumerable<EmissionsData> MinSamplingFiltering(IEnumerable<EmissionsData> expandedData, DateTimeOffset startDate, DateTimeOffset endDate, double minSamplingWindow)
    {
        TimeSpan ts = endDate - startDate;
        if ((ts.TotalMinutes >= minSamplingWindow) || !expandedData.Any())
        {
            return expandedData;
        }

        var arrData = expandedData.ToArray();
        // sort data since different sources might have populated the data differently.
        Array.Sort(arrData, new CompareEmissionDataSort());

        var startDateTime = startDate.DateTime;
        var dataLength = arrData.Length;

        var splitIndex = dataLength - 1;
        while (!(arrData[splitIndex].Time < startDateTime)){
            if (splitIndex == 0) break;
            splitIndex --;
        }

        var filteredData = new EmissionsData[dataLength-splitIndex];

        // copy from the split index on
        Array.Copy(arrData, splitIndex, filteredData, 0, filteredData.Length);
        return filteredData;
    }

    /// <summary>
    /// Add or Substract minutes from date 
    /// </summary>
    /// <param name="date">Date to shift</param>
    /// <param name="minutesValue">Minutes to add or substract</param>
    /// <returns>Shifted date</returns>
    /// <remarks>To substract minutes, minutesValue should be negative.</remarks>
    public static DateTimeOffset ShiftDate(DateTimeOffset date, double minutesValue)
    {
        return date.AddMinutes(minutesValue);
    }
}

public class CompareEmissionDataSort : IComparer<EmissionsData>
{
    public int Compare(EmissionsData x, EmissionsData y)
    {
        if (x.Time == y.Time) return 0;
        return x.Time < y.Time ? -1 : 1;
    }
}
