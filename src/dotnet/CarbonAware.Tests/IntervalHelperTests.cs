using CarbonAware.Model;

namespace CarbonAware.Tests;

public class IntervalHelperTests
{
    private readonly DateTimeOffset startDateTimeOffset = new DateTimeOffset(DateTime.Parse("2021-09-01T09:30:00.0000000Z"));
    private readonly DateTimeOffset endDateTimeOffset = new DateTimeOffset(DateTime.Parse("2021-09-01T10:15:00.0000000Z"));
    private readonly double minSamplingWindow = 60;

    /// <summary>
    /// Test exit cases of min sampling filtering
    /// </summary>
    [Test]
    public void TestMinSamplingFilteringExitCases()
    {
        // Sample data from 8:30 to 10:30
        var data = new EmissionsData[5]
        {
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T08:30:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T09:00:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T09:30:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T10:00:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T10:30:00.0000000Z")
            },
        };

        // If pass in empty data, expect empty result
        var emptyResult = IntervalHelper.MinSamplingFiltering(Enumerable.Empty<EmissionsData>(), startDateTimeOffset, endDateTimeOffset, minSamplingWindow);
        Assert.False(emptyResult.Any());

        // If pass in interval >= min sampling window, expect same data result
        var minSamplingWindow30 = 30;
        var minWindowValid = IntervalHelper.MinSamplingFiltering(data, startDateTimeOffset, endDateTimeOffset, minSamplingWindow30);
        Assert.True(minWindowValid.Count() == 5);
    }

    /// <summary>
    /// Test min sampling filtering
    /// </summary>
    [Test]
    public void TestMinSamplingFiltering()
    {
        var input = new EmissionsData[4]
        {
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T08:30:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T09:00:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T09:30:00.0000000Z")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01T10:00:00.0000000Z")
            },
        };
        var minWindowFiltering = IntervalHelper.MinSamplingFiltering(input, startDateTimeOffset, endDateTimeOffset, minSamplingWindow);
        // We expect the 9, 9:30, and 10 data points
        Assert.True(minWindowFiltering.Count() == 3);
        Assert.IsTrue(minWindowFiltering.First().Time.Hour + 1 == minWindowFiltering.Last().Time.Hour);
        Assert.IsTrue(minWindowFiltering.First().Time.Minute == 0);
        Assert.IsTrue(minWindowFiltering.Last().Time.Minute == 0);
    }

    /// <summary>
    /// Test to ensure that our manual comparator is working as we expect
    /// </summary>
    [Test]
    public void TestComparator()
    {
        var data = new EmissionsData[3]
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-12-01")
            },
            new EmissionsData {
                Location = "sydney",
                Time = DateTime.Parse("2021-12-01")
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-09-01")
            }
        };

        var sameDateData = new EmissionsData[2]
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-12-01")
            },
            new EmissionsData {
                Location = "sydney",
                Time = DateTime.Parse("2021-12-01")
            }
        };

        Array.Sort(data, new CompareEmissionDataSort());
        Array.Sort(sameDateData, new CompareEmissionDataSort());

        // First item should be eastus because it occurs first.
        Assert.True(data[0].Location == "eastus");

        // Order should've remained unchanged because occurs at same time
        Assert.True(sameDateData[0].Location == "westus");
        Assert.True(sameDateData[1].Location == "sydney");
    }
}
