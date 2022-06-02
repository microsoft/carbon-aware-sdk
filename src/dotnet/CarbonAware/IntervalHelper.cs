using System.Diagnostics;
using System.Reflection;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;

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
        int indexEnd = Find_Nearest(newData, endDate);
        var filteredData = new EmissionsData[indexEnd - indexStart + 1];
        Array.Copy(newData.ToArray(), indexStart, filteredData, 0, indexEnd - indexStart + 1);
        return filteredData;
    }

    private static int Find_Nearest(IEnumerable<EmissionsData> data, DateTime findTime)
    {
            var dummyValue = new EmissionsData {
                Time = findTime
            };
            int index = Array.BinarySearch(data.ToArray(), dummyValue, new CompareEmissionDataTime());
            if (index < 0)
            {
                var compl = ~index;
                if (compl == data.Count())
                {
                    return data.Count() - 1;
                }
                var left_dist = Math.Abs(data.ElementAt(compl - 1).Time.Second - findTime.Second);
                var right_dist = Math.Abs(data.ElementAt(compl).Time.Second - findTime.Second);
                return  Math.Min(left_dist, right_dist) == left_dist ? (compl - 1) : compl;
            }
            return index;
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