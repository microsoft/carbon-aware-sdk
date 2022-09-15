﻿using CarbonAware.DataSources.Configuration;
using CarbonAware.DataSources.Mocks;
using CarbonAware.DataSources.Json.Mocks;
using CarbonAware.DataSources.WattTime.Mocks;
using NUnit.Framework;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CarbonAware.CLI.IntegrationTests;

/// <summary>
/// A base class that does all the common setup for the Integration Testing
/// Overrides WebAPI factory by switching out different configurations via _datasource
/// </summary>
public abstract class IntegrationTestingBase
{
    private string _executableName = "caw";
    internal DataSourceType _dataSource;
    internal string? _dataSourceEnv;
    protected IDataSourceMocker _dataSourceMocker;
    protected TestConsole _console = new();


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationTestingBase(DataSourceType dataSource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _dataSource = dataSource;
        _dataSourceEnv = Environment.GetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource");
    }

    protected int InvokeCLI(string arguments)
    {
        // Initialize process here
        Process proc = new Process();
        proc.StartInfo.FileName = _executableName;
        // add arguments as whole string
        proc.StartInfo.Arguments = arguments;

        // use it to start from testing environment
        proc.StartInfo.UseShellExecute = false;

        // redirect outputs to have it in testing console
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;

        // set working directory
        proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

        Console.SetOut(_console.Out.CreateTextWriter());
        Console.SetError(_console.Error.CreateTextWriter());

        // start and wait for exit
        proc.Start();
        proc.WaitForExit();

        // get output to testing console.
        Console.WriteLine(proc.StandardOutput.ReadToEnd());
        Console.Write(proc.StandardError.ReadToEnd());

        // reset Console Streams
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        var standardError = new StreamWriter(Console.OpenStandardError());
        standardOutput.AutoFlush = true;
        standardError.AutoFlush = true;
        Console.SetOut(standardOutput);
        Console.SetError(standardError);

        // return exit code
        return proc.ExitCode;
    }

    [OneTimeSetUp]
    public void Setup()
    {
        //Switch between different data sources as needed
        //Each datasource should have an accompanying DataSourceMocker that will perform setup activities
        switch (_dataSource)
        {
            case DataSourceType.JSON:
                {
                    Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "JSON");
                    _dataSourceMocker = new JsonDataSourceMocker();
                    break;
                }
            case DataSourceType.WattTime:
                {
                    Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", "WattTime");
                    _dataSourceMocker = new WattTimeDataSourceMocker();
                    break;
                }
            case DataSourceType.None:
                {
                    throw new NotSupportedException($"DataSourceType {_dataSource.ToString()} not supported");
                }
        }

        string assemblyCodeBase =
                System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get directory name
        string dirName = Path.GetDirectoryName(assemblyCodeBase) ?? "";

        // remove URL-prefix if it exists
        if (dirName.StartsWith("file:\\"))
            dirName = dirName.Substring(6);

        // set current folder
        Environment.CurrentDirectory = dirName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _executableName += ".exe";
    }

    [SetUp]
    public void SetupTests()
    {
        _console = new TestConsole();
        _dataSourceMocker.Initialize();
    }

    [TearDown]
    public void ResetTests()
    {
        _dataSourceMocker.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _dataSourceMocker.Dispose();
        Environment.SetEnvironmentVariable("CarbonAwareVars__CarbonIntensityDataSource", _dataSourceEnv);
    }
}