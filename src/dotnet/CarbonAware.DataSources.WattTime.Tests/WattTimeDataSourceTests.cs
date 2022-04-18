using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class WattTimeDataSourceTests
{
    private Mock<ILogger<WattTimeDataSource>> Logger { get; set; }

    private Mock<IWattTimeClient> WattTimeClient { get; set; }

    private ActivitySource ActivitySource { get; set; }

    private WattTimeDataSource DataSource { get; set; }

    private List<Forecast> Forcasts { get; set; }

    private Mock<IRegionConverter> RegionConverter { get; set; }

    private BalancingAuthority BalancingAuthority { get; set; }

    private DateTimeOffset StartDate { get; set; }

    private DateTimeOffset EndDate { get; set; }

    private string Region { get; set; }

    [SetUp]
    public void Setup()
    {
        this.Region = "us-east";

        this.BalancingAuthority = new BalancingAuthority() { Abbreviation = "BA" };
        this.StartDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        this.EndDate = new DateTimeOffset(2022, 4, 18, 12, 33, 42, TimeSpan.FromHours(-6));
        this.Forcasts = new List<Forecast>() 
        { 
            new Forecast() 
            { 
                GeneratedAt = this.StartDate.DateTime, 
                ForecastData = new List<GridEmissionDataPoint>() 
                { 
                    new GridEmissionDataPoint() 
                    { 
                        BalancingAuthorityAbbreviation = this.BalancingAuthority.Abbreviation, 
                        Datatype = "dataType", 
                        Frequency = 30, 
                        Market = "market", 
                        PointTime = this.StartDate.DateTime, 
                        Value = 5, 
                        Version = "1" 
                    } 
                } 
            } 
        };

        this.ActivitySource = new ActivitySource("WattTimeDataSourceTests");

        this.Logger = new Mock<ILogger<WattTimeDataSource>>();
        this.WattTimeClient = new Mock<IWattTimeClient>();
        this.RegionConverter = new Mock<IRegionConverter>();

        this.RegionConverter.Setup(r => r.ConvertAzureRegionAsync(this.Region)).ReturnsAsync(this.BalancingAuthority);

        this.WattTimeClient.Setup(w => w.GetForecastByDateAsync(
            this.BalancingAuthority, 
            this.StartDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), 
            this.EndDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))
        ).ReturnsAsync(() => this.Forcasts);

        this.DataSource = new WattTimeDataSource(this.Logger.Object, this.WattTimeClient.Object, this.ActivitySource, this.RegionConverter.Object);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var result = await this.DataSource.GetCarbonIntensityAsync(this.Region, this.StartDate, this.EndDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.AreEqual(5m, first.Rating);
        Assert.AreEqual(this.Region, first.Location);
        Assert.AreEqual(this.StartDate.DateTime, first.Time);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsEmptyListWhenNoRecordsFound()
    {
        this.Forcasts.Clear();

        var result = await this.DataSource.GetCarbonIntensityAsync(this.Region, this.StartDate, this.EndDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        Assert.ThrowsAsync<Exception>(async () => await this.DataSource.GetCarbonIntensityAsync("badregion", this.StartDate, this.EndDate));
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
