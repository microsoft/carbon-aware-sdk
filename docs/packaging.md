# Packaging the Carbon Aware SDK

With the addition of the C# Client Library as a way to consume the Carbon Aware SDK, we have also added [bash scripts](../scripts/package/) to package the library, and have included a sample [Console App](../samples/lib-integration/) showing how the package can be consumed.

## Included Projects
The current package include 8 projects from the SDK:

1. "GSF.CarbonAware"
2. "CarbonAware"
3. "CarbonAware.Aggregators"
4. "CarbonAware.DataSources.Json"
5. "CarbonAware.DataSources.Registration"
6. "CarbonAware.DataSources.WattTime"
7. "CarbonAware.LocationSources.Azure"
8. "CarbonAware.Tools.WattTimeClient"

These 8 projects enable users of the library to consume the current routes exposed by the library. The package that needs to be added to a new C# project is `GSF.CarbonAware`.

## Included Scripts
There are 2 scripts included to help the packaging process
1. `add_package.sh <dotnet_project> <package_destination>`
2. `create_packages.sh <dotnet_solution> <package_destination>`

The [`create_package`](../scripts/package/create_packages.sh)  script is called with 2 parameters: the dotnet solution file (`.sln`) path, and the output directory destination for the package. The [`add_package`](../scripts/package/add_package.sh) script is also called with 2 parameters: the project file (`.csproj`) path, and the with the output directory destination for the package. 

To see an example of both scripts being invoked, you can look at the github action detailed in [build-packages.yaml](../.github/workflows/build-packages.yaml).

### Running the packaging scripts
Prerequisites:
- .NET Core 6.0
- Alternatively: 
	- Docker
	- VSCode (it is recommended to work in a Dev Container)
	- Remote Containers extension for VSCode: https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers

## Console App Sample
There is a sample console app in the [lib integration folder](../samples/lib-integration/ConsoleApp/). The app shows how to use dependency injection to pull in the packages and ping the SDK.

In order to run the app, you will need to run the scripts commands similar to what `build-packages.yaml` does, to create the packages, add them, and the restore them into the app.

