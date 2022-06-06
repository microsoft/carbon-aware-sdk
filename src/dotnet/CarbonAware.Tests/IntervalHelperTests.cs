using CarbonAware.Model;

namespace CarbonAware.Tests;

public class IntervalHelperTests
{
    private readonly DateTimeOffset startDateTimeOffset = new (2021, 9, 1, 9, 30, 0, TimeSpan.Zero);
    private readonly DateTimeOffset endDateTimeOffset = new (2021, 9, 1, 10, 15, 0, TimeSpan.Zero);
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
                Time = new DateTime(2021,9,1,8,30,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,9,0,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,9,30,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,10,0,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,10,30,0)
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
                Time = new DateTime(2021,9,1,8,30,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,9,0,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,9,30,0)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1,10,0,0)
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
                Time = new DateTime(2021,12,1)
            },
            new EmissionsData {
                Location = "sydney",
                Time = new DateTime(2021,12,1)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2021,9,1)
            }
        };

        var sameDateData = new EmissionsData[2]
        {
            new EmissionsData {
                Location = "westus",
                Time = new DateTime(2021,12,1)
            },
            new EmissionsData {
                Location = "sydney",
                Time = new DateTime(2021,12,1)
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
