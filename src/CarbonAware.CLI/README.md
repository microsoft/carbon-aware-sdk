The Carbon Aware SDK provides a CLI to get the emissions data and forecast data for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the CLI, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - https://github.com/microsoft/carbon-aware-sdk/blob/dev/GettingStarted.md

# CLI Command Naming Conventions: 

- The CLI contains an aggregator keyword. 
- The only current aggregator keyword is `emissions`, which takes multiple sub-commands. 
- Each group/aggregator keyword can be analogous to each controller. For example, we may create a ‘sci-score’ keyword to support functionality covered in SciController.  
- Multi-word subgroups/commands should be hyphenated (e.g. foo-resource instead of fooresource) 
- The commands must follow a "[noun] [noun]" pattern where: 
    - The first ‘noun’ is the aggregator command 
    - The second ‘noun’ defines the specific functionality     

Currently, the Carbonaware SDK only supports GET/POST requests. We may have to add different commands if there are any CRUD operations added to the SDK. 

# Example

| CarbonAware CLI  
| Group (noun)   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Command (noun)   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Argument   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|   
$ emissions observed eastus 


## Command Group

### `emissions`

The `emissions` keyword is used to invoke any commands related to getting Carbon Emissions data.

## Command Sub-groups

### `observed`
### `current-forecast`
### `average-intensity`

# Carbon Aware CLI Commands  

## emissions observed 

EG Input
```
.\CarbonAware.CLI.exe emissions observed eastus westus
```

EG Output
```
[
  {
    "Location":"eastus",
    "Time":"2022-08-04T12:45:11+00:00",
    "Rating":97,
    "Duration":"08:00:00"
  },
  {
    "Location":"eastus",
    "Time":"2022-08-04T20:45:11+00:00",
    "Rating":48,
    "Duration":"08:00:00"
  }
]
```

## emissions current-forecast

EG Input
```
.\CarbonAware.CLI.exe emissions current-forecast eastus
```

EG Output
[
  {
    "requestedAt": "2022-06-01T00:03:30Z",
    "location": "eastus",
    "dataStartAt": "2022-06-01T12:00:00Z",
    "dataEndAt": "2022-06-01T18:00:00Z",
    "windowSize": 30,
    "generatedAt": "2022-06-01T00:00:00Z",
    "optimalDataPoint": {
      "location": "eastus",
      "timestamp": "2022-06-01T14:45:00Z",
      "duration": 30,
      "value": 359.23
    },
    "forecastData": [
      {
        "location": "eastus",
        "timestamp": "2022-06-01T14:40:00Z",
        "duration": 30,
        "value": 380.99
      }
    ]
   }
]
```

## emissions average-intensity

EG Input
```
.\CarbonAware.CLI.exe emissions average-intensity eastus
```

EG Output
```
{
  "location": "eastus",
  "startTime": "2022-03-01T15:30:00Z",
  "endTime": "2022-03-01T18:30:00Z",
  "carbonIntensity": 345.434
}
```