# 8. Data Source Configuration

## Status

Proposed

## Context

The current CarbonAware configuration is not very structured and intuitive for a user, and so requires deep reading of the documentation to properly configure. Moreover, it is not made for use-cases where different interfaces can be configured with different data sources. EG: JSON data source for emissions, but WattTime data source for forecast data. 

## Decision
We propose some structural changes to the configuration to make it more readable and also provide the flexibility to configure each interface with a different data source. 

The 'DataSources' section will contain the configuration needed to load the interfaces with the appropriate underlying data sources. 

Each datsource interface can be configured to have an associated data source. This gives the flexibility to configure interfaces with different data sources. 

Individual data sources will be defined in the 'Configurations' section which will contain the parameters required for configuring the client and any additional parameters required to make API calls to the data source. 

This configuration is flexible and extensible to support any new interfaces and data sources.

The following change is proposed to the config schema -

```json
{
 "DataSources": {
    "CarbonIntensityDataSource": "WattTime",
    "EnergyDataSource": "ElectricityMaps",
    "Configurations": {
      "WattTime": {
        "ClientConfiguration": {
          "Username": "username",
          "Password": "password",
          "BaseURL": "https://api2.watttime.org/v2/"
        }
      },
      "ElectricityMaps": {
        "ClientConfiguration": {
          "API_Key": "abcd",
          "BaseURL": "https://api.electricitymap.org/v3/"
        },
        "disableEstimations": "true",
        "emissionsFactorType": "lifecycle"
      }
    }
  }
```

## Consequences
During initialization of the interface, the config will read to get the data source associated with it. For eg, when a CabonIntensityDatasource is initialized, it will get the data source value from the config, which is 'WattTime' in the above example. It then tries to find the configuration with 'WattTime' as the key from the 'Configurations' section. The parameters retrieved from the config will be then used to load the WattTimeClient.

## Green Impact

Neutral
