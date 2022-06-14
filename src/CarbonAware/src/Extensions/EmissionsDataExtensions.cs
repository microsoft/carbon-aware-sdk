namespace CarbonAware.Extensions;

/// <summary>
/// Extension class for working with EmissionsData objects.
/// </summary>
public static class EmissionsDataExtensions
{
    /// <summary>
    /// Projects the data as a rolling average for a specified window size. 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This type of rolling average windowing is necessary for determining the optimal period for any workload whose  
    ///         duration is longer than the granularity of the EmissionsData. Take the following examples of values for 5 
    ///         minute increments that produce false "optimal" answers for a 10 minute long workload:
    ///     </para>
    ///     <para>
    ///         [10, 20, 30, 40, 5] - The optimal single point is the last one, but our workload cannot start at that time and finish within the dataset.
    ///         [15, 25, 35, 22.5]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    ///     <para>
    ///         [5, 55, 10, 10, 10] - The optimal single point is the first one, but our workload started at that time will have an suboptimal average value (AVG(5,55) == 30).
    ///         [30, 32.5, 10, 10]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    ///     <para>
    ///         [12, 11, 10, 12, 13] - The optimal single point is the center one, but our workload started at that time will have an suboptimal average value (AVG(10,12) == 11).
    ///         [11.5, 10.5, 11, 12.5]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    /// </remarks>
    /// <param name="data">The IEnumerable<EmissionsData> being operated on.</param>
    /// <param name="windowSize">The duration of the window to be averaged.</param>
    /// <param name="tickSize">The duration the windows slides forward before calculating the next average.</param>
    /// <returns>An enumerable of emissions data objects, each representing a single average window.</returns>
    public static IEnumerable<EmissionsData> RollingAverage(this IEnumerable<EmissionsData> data, TimeSpan windowSize = default, TimeSpan tickSize = default)
    {
        if (data.Count() == 0){ yield break; }

        if (windowSize == default)
        {
           foreach(var d in data){ yield return d; }
           yield break;
        }

        var q = new Queue<EmissionsData>();
        var _data = data.GetEnumerator();
        _data.MoveNext();
        EmissionsData current = _data.Current;
        EmissionsData last = null;

        if (tickSize == default)
        {
            tickSize = (current.Duration > TimeSpan.Zero) ? current.Duration : throw new InvalidOperationException("RollingAverage tickSize must be > 0");
        }

        // Set initial rolling average window
        DateTimeOffset windowStartTime = current.Time;
        DateTimeOffset windowEndTime = windowStartTime + windowSize;

        while (current != null)
        {
            // Enqueue data points relevant to current rolling average window
            while (current != null && windowEndTime > current.Time)
            {
                q.Enqueue(current);
                last = current;
                _data.MoveNext();
                current = _data.Current;
            }

            // Calculate average for everything in the queue if we enqueued enough data points to cover the window
            if (last != null && last.Time + last.Duration >= windowEndTime)
            {
                yield return new EmissionsData()
                {
                    Time = windowStartTime,
                    Duration = windowEndTime - windowStartTime,
                    Location = last.Location,
                    Rating = AverageOverPeriod(q, windowStartTime, windowEndTime)
                };
            }

            // Set bounds for the next window
            windowStartTime = windowStartTime + tickSize;
            windowEndTime = windowStartTime + windowSize;

            // Dequeue items not needed for next window average
            var peek = q.Peek();
            while (peek != null && peek.Time + peek.Duration < windowStartTime)
            {
                q.Dequeue();
                peek = q.Count == 0 ? null : q.Peek();
            }
        }
    }

    /// <summary>
    /// Finds the average rating of a continuous, chronological set of EmissionsData objects for a given period. 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method takes into account fractional data points and weights the average accordingly.
    ///         Take the following (HH:MM, rating) hour-long data points for example: (01:00, 30), (02:00, 100), (03:00, 50)
    ///     </para>
    ///     <para>
    ///         To calculate the average over the time period 01:30 - 03:30, the proportion each point contributes
    ///         to the whole period must be taken into account.  This proportion is calculated as (utilized time / total duration)
    ///         For the 2 hour period (01:30-03:30) the proportions would be:
    ///         (30 * (30m/120m)) + (100 * (60m/120m)) + (50 * (30m/120m)); which simplifies to 
    ///         (30 * (1/4)) + (100 * (1/2)) + (50 * (1/4)); or
    ///         (30/4) + (100/2) + (50/4)
    ///     </para>
    ///     <para>
    ///         This method works just as well for simple averages typically calculated as the sum of the values divided by the number of values.
    ///         (30 + 100 + 50)/3;  We can use the equation above to average the three points over their full 3 hour duration.
    ///         (30 * (60m/180m)) + (100 * (60m/180m)) + (50 * (60m/180m)); which simplifies to
    ///         (30 * (1/3)) + (100 * (1/3)) + (50 * (1/3)); or
    ///         (30/3) + (100/3) + (50/3); or further
    ///         (30 + 100 + 50)/3 
    ///     </para>
    /// </remarks>
    /// <param name="data">The IEnumerable<EmissionsData> being operated on.</param>
    /// <param name="startTime">The start time of the data to be averaged.</param>
    /// <param name="endTime">The end time of the data to be averaged.</param>
    /// <returns>The average rating of the data for the specified time period</returns>
    public static double AverageOverPeriod(this IEnumerable<EmissionsData> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        double rating = 0.0;
        TimeSpan totalDuration = endTime - startTime;
        DateTimeOffset? lastEndTime = null;
        foreach (var current in data)
        {
            lastEndTime = (lastEndTime == null || current.Time == lastEndTime) ? current.Time + current.Duration : throw new InvalidOperationException($"AverageOverPeriod requires continuous chronological data. Previous point covered through {lastEndTime}; Current point starts at {current.Time}."); ;

            if (current.Time + current.Duration > startTime && current.Time < endTime)
            {
                var lowerBound = (startTime >= current.Time) ? startTime : current.Time;
                var upperBound = (endTime < current.Time + current.Duration) ? endTime : current.Time + current.Duration;
                rating += current.Rating * (upperBound - lowerBound) / totalDuration;
            }
        }

        return rating;
    }
}