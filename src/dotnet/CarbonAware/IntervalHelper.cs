namespace CarbonAware;

/// <summary>
/// Helper for intervals within Carbon Aware.
/// </summary>
public static class IntervalHelper
{ 
    /// <summary>
    /// Static helper that filters the data to the minimum sampling interval.
    /// </summary>
    /// <param name="data">Data to filter</param>
    /// <param name="startDate">Original start date provided by user</param>
    /// <param name="endDate">Original end date provided by user</param>
    /// <param name="minSamplingWindow">Minimum sampling window we want to enforce</param>
    /// <returns>Filtered emissions data.</returns>
    /// <remarks> If the user provided interval is already >= minimum sampling window, we just return the passed data. </remarks>
    public static IEnumerable<EmissionsData> FilterToMinInterval(IEnumerable<EmissionsData> data, DateTime startDate, DateTime endDate, double minSamplingWindow)
    {
        // If provided interval is at least the minimum sampling window, return data unfiltered
        TimeSpan ts = endDate - startDate;
        if (ts.TotalMinutes >= minSamplingWindow) return data;

        // Otherwise, push back startDate to hit minimum sampling window
        var shiftedStart = endDate.AddMinutes(-minSamplingWindow);
        var newData = data.Where(ed => ed.TimeBetween(shiftedStart, endDate));

        // No data found within minimum interval
        if (!newData.Any()) return newData;

        int indexStart = Find_Nearest(newData, startDate);
        var filteredData = new EmissionsData[newData.Count() - indexStart];
        Array.Copy(newData.ToArray(), indexStart, filteredData, 0, newData.Count() - indexStart);
        return filteredData;
    }

    private static int Find_Nearest(IEnumerable<EmissionsData> data, DateTime findTime)
    {
        var searchValue = new EmissionsData {
            Time = findTime
        };
        int index = Array.BinarySearch(data.ToArray(), searchValue, new CompareEmissionDataTime());
        if (index < 0) return index;
        var complement = ~index;
        return complement == data.Count() ? (data.Count() - 1) : (complement - 1);
    }
}

class CompareEmissionDataTime : IComparer<EmissionsData>
{
    public int Compare(EmissionsData x, EmissionsData y)
    {
        if (x?.Time == null) return -1;
        if (y?.Time == null) return 1;
        if (x.Time == y.Time) return 0;
        return x.Time < y.Time ? -1 : 1;
    }
}