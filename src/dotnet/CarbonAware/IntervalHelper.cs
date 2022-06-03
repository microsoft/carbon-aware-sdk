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
        if (ts.TotalMinutes >= minSamplingWindow) return newData;

        if (!newData.Any()) return newData;

        int indexStart = Find_Nearest_Index(newData, startDate.DateTime);
        var filteredData = new EmissionsData[newData.Count() - indexStart];
        Array.Copy(newData.ToArray(), indexStart, filteredData, 0, newData.Count() - indexStart);
        return filteredData;
    }

    public static DateTimeOffset GetShiftedDate(DateTimeOffset original, double minSamplingWindow)
    {
        return original.AddMinutes(-minSamplingWindow);
    }

    private static int Find_Nearest_Index(IEnumerable<EmissionsData> data, DateTime findTime)
    {
        var searchValue = new EmissionsData {
            Time = findTime
        };
        int index = Array.BinarySearch(data.ToArray(), searchValue, new CompareEmissionDataTime());
        if (index >= 0) return index;

        var complement = ~index;
        // return last index or the one which time is less than the time provided.
        return complement == data.Count() ? (data.Count() - 1) : (complement - 1);
    }
}

class CompareEmissionDataTime : IComparer<EmissionsData>
{
    public int Compare(EmissionsData x, EmissionsData y)
    {
        if (x.Time == y.Time) return 0;
        return x.Time < y.Time ? -1 : 1;
    }
}
