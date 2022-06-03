namespace CarbonAware;

/// <summary>
/// Helper for intervals within Carbon Aware.
/// </summary>
public static class IntervalHelper
{ 

    /// <summary>
    /// Static helper that filters the data from the startDate.
    /// </summary>
    /// <param name="data">Data to filter</param>
    /// <param name="startDate">Original start date provided by user</param>
    /// <returns>Filtered emissions data.</returns>
    /// <remarks> If the user provided interval is already >= minimum sampling window, we just return the passed data. </remarks>
    public static IEnumerable<EmissionsData> GetFilteredData(IEnumerable<EmissionsData> newData, DateTimeOffset startDate, DateTimeOffset endDate, double minSamplingWindow)
    {
        TimeSpan ts = endDate - startDate;
        if ((ts.TotalMinutes >= minSamplingWindow) || !newData.Any())
        {
            return newData;
        }

        var arrData = newData.ToArray();
        // sort data since different sources might have populated the data differently.
        Array.Sort(arrData, new CompareEmissionDataSort());
        var filteredData = new EmissionsData[1];
        // copy only the last element since it is sorted by time.
        Array.Copy(arrData, arrData.Length - 1, filteredData, 0, 1);
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

class CompareEmissionDataSort : IComparer<EmissionsData>
{
    public int Compare(EmissionsData x, EmissionsData y)
    {
        if (x.Time == y.Time) return 0;
        return x.Time < y.Time ? -1 : 1;
    }
}
