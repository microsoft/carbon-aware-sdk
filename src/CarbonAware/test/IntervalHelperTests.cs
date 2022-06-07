using CarbonAware.Model;

namespace CarbonAware.Tests;

public class IntervalHelperTests
{
    private readonly DateTimeOffset startDateTimeOffset = new (2021, 9, 1, 9, 40, 0, TimeSpan.Zero);
    private readonly DateTimeOffset endDateTimeOffset = new (2021, 9, 1, 10, 20, 0, TimeSpan.Zero);
    private readonly TimeSpan minSamplingWindow = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Test exit cases of min sampling filtering
    /// </summary>
    [Test]
    public void TestMinSamplingFiltering()
    {
        // Sample data from 8:30 to 10:30
        var data = new EmissionsData[5]
        {
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,9,0,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,9,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,10,0,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,10,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            }
        };

        // If pass in empty data, will just return empty data
        var emptyResult = IntervalHelper.MinSamplingFiltering(Enumerable.Empty<EmissionsData>(), startDateTimeOffset, endDateTimeOffset);
        Assert.False(emptyResult.Any());

        // If pass in duration, will ignore data value. With 45 min duration, captures 3 data points
        var constantDuration = IntervalHelper.MinSamplingFiltering(data, startDateTimeOffset, endDateTimeOffset, TimeSpan.FromMinutes(45));
        Assert.True(constantDuration.Count() == 3);

        // If don't pass in duration, will lookup value in data. WIth included 30 min duration, captures 2 data points
        var minWindowValid = IntervalHelper.MinSamplingFiltering(data, startDateTimeOffset, endDateTimeOffset);
        Assert.True(minWindowValid.Count() == 2);
    }

    /// <summary>
    /// Test shift date functionality
    /// </summary>
    [Test]
    public void TestShiftDate()
    {
        // When time between start and end is greater than window, dates are not shifted
        Double window30 = 30;
        (DateTimeOffset, DateTimeOffset) notShifted = IntervalHelper.ShiftDate(startDateTimeOffset, endDateTimeOffset, window30);
        Assert.True(notShifted.Item1.Equals(startDateTimeOffset));
        Assert.True(notShifted.Item2.Equals(endDateTimeOffset));


        // When time between start and end is less than minimum window, dates are shifted
        Double window60 = 60;
        (DateTimeOffset, DateTimeOffset) shifted = IntervalHelper.ShiftDate(startDateTimeOffset, endDateTimeOffset, window60);
        Assert.True(shifted.Item1.AddMinutes(window60).Equals(startDateTimeOffset));
        Assert.True(shifted.Item2.AddMinutes(-window60).Equals(endDateTimeOffset));
    }
}
