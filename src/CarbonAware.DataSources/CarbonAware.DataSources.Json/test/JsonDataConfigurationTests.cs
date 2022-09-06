using NUnit.Framework;
using CarbonAware.DataSources.Json.Configuration;
using System.Reflection;
using System.IO;
using System;

namespace CarbonAware.DataSources.Json.Tests;

[TestFixture]
public class JsonDataConfigurationTests
{

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private JsonDataConfiguration Config { get; set; }
    private string AssemblyPath { get; set; }
    #pragma warning restore CS8618
    
    private const string BaseDir = "data-files";

    [SetUp]
    public void Setup()
    {
        Config = new JsonDataConfiguration();
        AssemblyPath = Assembly.GetExecutingAssembly().Location;
    }

    [Test]
    public void GetDefaultDataFileLocation_IsNotNull_ExpectedBaseDir()
    {
        Assert.That(Config.DataFileLocation, Is.Not.Null);
        var expectedDir = Path.Combine(Path.GetDirectoryName(AssemblyPath)!, BaseDir);
        Assert.That(Config.DataFileLocation, Contains.Substring(expectedDir));
    }

    [TestCase("../newfile.json", TestName = "setting parent's dir")]
    [TestCase("~/newfile.json", TestName = "setting user's home dir")]
    public void SetDataFileLocation_ThrowsArgumentException(string filePath)
    {
        var ex = Assert.Throws<ArgumentException>(() => Config.DataFileLocation = filePath);
        Assert.That(ex!.Message, Contains.Substring("not supported characters"));
    }

    [TestCase("newfile.json", TestName = "same location as base dir")]
    [TestCase("/file1.json", TestName = "setting root dir")]
    [TestCase("another_dir/anotherfile.json", TestName = "subdir under base dir")]
    [TestCase("new-dir 123/Sub_Dir/anotherfile.json", TestName = "subdirs with numbers and upper case chars under base dir")]
    public void SetDataFileLocation_Success(string filePath)
    {
        Config.DataFileLocation = filePath;
        var expected = Path.Combine(Path.GetDirectoryName(AssemblyPath)!, BaseDir, filePath);
        Assert.That(Config.DataFileLocation, Is.EqualTo(expected));
    }
}
