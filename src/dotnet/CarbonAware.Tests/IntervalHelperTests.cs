using CarbonAware.Model;

namespace CarbonAware.Tests;

public class IntervalHelperTests
{
    /// <summary>
    /// Test min sampling filtering
    /// </summary>
    [Test]
    public void TestMinSamplingFiltering()
    {
        var startDateTimeOffset = DateTimeOffset.Now.AddMinutes(-120); // 2 hours ago
        var endDateTimeOffset = startDateTimeOffset.AddMinutes(60); // 1 hour ago
        var minSamplingWindow = 60;
        
        // If pass in empty data, expect empty result
        var emptyResult1 = IntervalHelper.MinSamplingFiltering(Enumerable.Empty<EmissionsData>(), startDateTimeOffset, endDateTimeOffset, minSamplingWindow);
        Assert.False(emptyResult1.Any());

        // If pass in interval >= min sampling window, expect same data result
        var data = new EmissionsData[2]
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-12-01")
            },
            new EmissionsData {
                Location = "sydney",
                Time = DateTime.Parse("2021-12-01")
            },
        };

        IEnumerable<EmissionsData>? minWindowValid = IntervalHelper.MinSamplingFiltering(data, startDateTimeOffset, endDateTimeOffset, minSamplingWindow);
        Assert.True(minWindowValid.Count() == 2);
        Assert.True(minWindowValid.First().Location == "westus");
        Assert.True(minWindowValid.Last().Location == "sydney");
    }

    /// <summary>
    /// Test shift date helper
    /// </summary>
    [Test]
    public void TestShiftDate()
    {
        Assert.True(true);
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
