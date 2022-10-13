# 8. Data Source Configuration

## Status

Proposed

## Context

The current CarbonAware configuration is not very structured and intuitive for a user, and so requires deep reading of the documentation to properly configure. Moreover, it is not made for use-cases where different interfaces can be configured with different data sources. EG: JSON data source for emissions, but WattTime data source for forecast data. 

## Decision
We propose some structural changes to the configuration to make it more readable and also provide the flexibility to configure each interface with different datasource. 

The 'DataSourceConfigurations' section will define each datasource with specific configuration parameters required for client creation and any additional optional parameters that would be needed to make API calls to the datasource. 

The 'Datasource' section will define the mapping of interfaces with the datasource.

The following change is proposed to the config schema -

```json
{
 "DataSourceConfigurations": {  
    "WattTime": {  
      "ClientConfiguration": {  
        "Username": "username",  
        "Password": "password",  
        "BaseURL": "https://api2.watttime.org/v2/"  
      }, 
    },  
    "ElectricityMaps": {  
      "ClientConfiguration": {  
         "API_Key": "abcd",  
     	   "BaseURL": "https://api.electricitymap.org/v3/"  
      }  
    }  
 },  

  "DataSources": {  
    "CarbonIntensityDataSource": {  
     "$ref": "#/DataSourceConfigurations/WattTime" 
   },  
   "EnergyDataSource": {  
     "$ref": "#/DataSourceConfigurations/ElectricityMaps" 
   },  
 }
}
```

## Consequences


## Green Impact

Neutral
