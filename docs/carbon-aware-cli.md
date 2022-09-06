# Carbon Aware CLI Reference

The following is the documentation for the Carbon Aware CLI

- [Carbon Aware CLI Reference](#carbon-aware-cli-reference)
  - [Build and Install](#build-and-install)
  - [Using the CLI](#using-the-cli)
    - [emissions](#emissions)
      - [Description](#description)
      - [Usage](#usage)
      - [Options](#options)
      - [Example](#example)

## Build and Install

Build the CLI using the `dotnet publish` command:

```bash
dotnet publish ./src/CarbonAware.CLI/src/CarbonAware.CLI.csproj -c Release -o <your desired installation path>
```

> By default this will build for your host operating system.  To build for a platform other than your host platform you can specify the target runtime like this, using any valid [Runtime ID](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#using-rids) (EG `win-x64`, `linux-x64`, `osx-x64`):
>
> ```bash
> dotnet publish .\src\CarbonAware.CLI\src\CarbonAware.CLI.csproj -c Release -r <RuntimeID> --self-contained -o <your desired installation path>
> ```

## Using the CLI

To use the CLI for the first time, navigate to your installation directory and run the binary with the `-h` flag to see the help menu.

On Windows: `.\caw.exe -h`
On MacOS/Linux: `.\caw -h`

### emissions

#### Description

Emissions command keyword to retrieve emissions data

#### Usage

`caw emissions [options]`

#### Options

```text
  -l, --location <location> (REQUIRED)  A list of locations
  --startTime <startTime>               Start time of emissions data
  --endTime <endTime>                   End time of emissions data
  -?, -h, --help                        Show help and usage information
```

#### Example

command: `.\caw.exe emissions -l eastus`

output:

```text
EmissionsData { Location = eastus, Time = 8/30/2022 12:45:11 PM +00:00, Rating = 65, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 8/30/2022 8:45:11 PM +00:00, Rating = 65, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 8/31/2022 4:45:11 AM +00:00, Rating = 4, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 8/31/2022 12:45:11 PM +00:00, Rating = 53, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 8/31/2022 8:45:11 PM +00:00, Rating = 49, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/1/2022 4:45:11 AM +00:00, Rating = 81, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/1/2022 12:45:11 PM +00:00, Rating = 30, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/1/2022 8:45:11 PM +00:00, Rating = 38, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/2/2022 4:45:11 AM +00:00, Rating = 19, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/2/2022 12:45:11 PM +00:00, Rating = 54, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/2/2022 8:45:11 PM +00:00, Rating = 55, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/3/2022 4:45:11 AM +00:00, Rating = 5, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/3/2022 12:45:11 PM +00:00, Rating = 22, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/3/2022 8:45:11 PM +00:00, Rating = 84, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/4/2022 4:45:11 AM +00:00, Rating = 30, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/4/2022 12:45:11 PM +00:00, Rating = 16, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/4/2022 8:45:11 PM +00:00, Rating = 60, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/5/2022 4:45:11 AM +00:00, Rating = 90, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/5/2022 12:45:11 PM +00:00, Rating = 16, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/5/2022 8:45:11 PM +00:00, Rating = 83, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/6/2022 4:45:11 AM +00:00, Rating = 73, Duration = 08:00:00 }
EmissionsData { Location = eastus, Time = 9/6/2022 12:45:11 PM +00:00, Rating = 84, Duration = 08:00:00 }
```
