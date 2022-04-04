# WattTime API Client
This project contains a client library for the [WattTime API](https://www.watttime.org/api-documentation/).

## Running tests

One can run the tests using the `dotnet test` command.

```bash
cd src\dotnet\CarbonAware.Tools.WattTimeClient.Tests
dotnet test
```

## Configuration
To use the client library in another project you need to add the following to the project:

In `myProject.csproj`:
```xml
<ItemGroup>
    <ProjectReference Include="..\CarbonAware.Tools.WattTimeClient\CarbonAware.Tools.WattTimeClient.csproj" />
</ItemGroup>
```

Make your WattTime username and password accessible via [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) to the application.

EG

```bash
set WattTimeClient__Username=<your username>
set WattTimeClient__Password=<your password>
```

Add and configure the client using dependency injection:

```csharp
using CarbonAware.Tools.WattTimeClient;

services.ConfigureWattTimeClient(configuration);
```
