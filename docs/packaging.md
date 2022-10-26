# Packaging the Carbon Aware SDK

With the addition of the C# Client Library as a was to consume the Carbon Aware SDK, we have also added [bach scripts](../scripts/package/) to package the library, and have included a sample [Console App](../samples/lib-integration/) showing how the package can be consumed.

## Included Projects
The current package include 8 projects from the SDK:

1. "GSF.CarbonIntensity"
2. "CarbonAware"
3. "CarbonAware.Aggregators"
4. "CarbonAware.DataSources.Json"
5. "CarbonAware.DataSources.Registration"
6. "CarbonAware.DataSources.WattTime"
7. "CarbonAware.LocationSources.Azure"
8. "CarbonAware.Tools.WattTimeClient"

These 8 projects enable users of the library to consume the current routes exposed by the library.

## Running the packaging scripts
Prerequisites:
- .NET Core 6.0
- Alternatively: 
	- Docker
	- VSCode (it is recommended to work in a Dev Container)
	- Remote Containers extension for VSCode: https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers