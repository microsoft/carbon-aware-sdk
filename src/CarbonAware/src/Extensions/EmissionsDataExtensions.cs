namespace CarbonAware.Extensions;

public static class EmissionsDataExtensions
{
    public static IEnumerable<EmissionsData> RollingAverage(this IEnumerable<EmissionsData> data, TimeSpan windowSize = default, TimeSpan tickSize = default)
    {
        if(data.Count() == 0){ yield break; }

        if(windowSize == default)
        {
           foreach(var d in data){ yield return d; }
           yield break;
        }

        var q = new Queue<EmissionsData>();
        var _data = data.GetEnumerator();
        _data.MoveNext();
        EmissionsData current = _data.Current;
        EmissionsData last = null;

        if(tickSize == default)
        {
            tickSize = (current.Duration > TimeSpan.Zero) ? current.Duration : throw new Exception("RollingAverage tickSize must be > 0");
        }

        // Set initial rolling average window
        DateTimeOffset windowStartTime = current.Time;
        DateTimeOffset windowEndTime = windowStartTime + windowSize;

        while(current != null)
        {
            // Enqueue data points relevant to current rolling average window
            while(current != null && windowEndTime > current.Time)
            {
                q.Enqueue(current);
                last = current;
                _data.MoveNext();
                current = _data.Current;
            }

            // Calculate average for everything in the queue if we enqueued enough data points to cover the window
            if(last != null && last.Time + last.Duration >= windowEndTime)
            {
                yield return AverageOverPeriod(q, windowStartTime, windowEndTime);
            }

            // Set bounds for the next window
            windowStartTime = windowStartTime + tickSize;
            windowEndTime = windowStartTime + windowSize;

            // Dequeue items not needed for next window average
            var peek = q.Peek();
            while(peek != null && peek.Time + peek.Duration < windowStartTime)
            {
                q.Dequeue();
                peek = q.Count == 0 ? null : q.Peek();
            }
        }
    }

    // TODO: Test this method outright when used as public method in the future.
    private static EmissionsData AverageOverPeriod(this IEnumerable<EmissionsData> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        EmissionsData newDataPoint = new EmissionsData()
        {
            Time = startTime,
            Duration = (endTime - startTime),
            Rating = 0.0,
            Location = data.First().Location
        };
        foreach(var current in data)
        {
            if(current.Time + current.Duration > startTime && current.Time < endTime)
            {
                var lowerBound = (startTime >= current.Time) ? startTime : current.Time;
                var upperBound = (endTime < current.Time + current.Duration) ? endTime : current.Time + current.Duration;
                newDataPoint.Rating += current.Rating * (upperBound - lowerBound) / newDataPoint.Duration;
            }
        }

        return newDataPoint;
    }
}