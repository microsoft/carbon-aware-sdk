using CarbonAware.Model;

namespace CarbonAware.Tests;

public class EmissionsDataTests
{
    const string noon = "2021-09-01T12:00:00Z";
    const string beforeNoon = "2021-09-01T11:00:00Z";
    const string wellBeforeNoon = "2021-09-01T10:00:00Z";
    const string afterNoon = "2021-09-01T13:00:00Z";

    [TestCase(beforeNoon, afterNoon, ExpectedResult = true, TestName = "Time is greater than start and less than end > && <")]
    [TestCase(beforeNoon, noon, ExpectedResult = true, TestName = "Time is greater than start and equal to end > && ==")]
    [TestCase(wellBeforeNoon, beforeNoon, ExpectedResult = false, TestName = "Time is after both times > && >")]
    [TestCase(noon, afterNoon, ExpectedResult = false, TestName = "Time is the start time == && <= ")]
    [TestCase(wellBeforeNoon, null, ExpectedResult = false, TestName = "Should always be false with no end date")]
    [TestCase(afterNoon, afterNoon, ExpectedResult = false, TestName = "Should always be false if Time is less than both < && <")]
    public bool TimeBetween(DateTime start, DateTime end)
    {
        var data = new EmissionsData()
        {
            Location = "Sydney",
            Rating = 100,
            Time = DateTime.Parse(noon)
        };

        return data.TimeBetween(start, end);
    }
}
