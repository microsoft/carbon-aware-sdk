The Carbon Aware SDK provides a CLI to get the marginal carbon intensity for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the CLI, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - https://github.com/microsoft/carbon-aware-sdk/blob/dev/GettingStarted.md

# CLI Command Naming Conventions: 

- The CLI contains an aggregator keyword. 
- The only current aggregator keyword is `emissions`, which takes multiple commands. 
- Each group/aggregator keyword can be analogous to each controller. For example, we may create a ‘sci-score’ keyword to support functionality covered in SciController.  
- Multi-word subgroups/commands should be hyphenated (e.g. foo-resource instead of fooresource) 
- The commands must follow a "[noun] [noun] [verb]" pattern where: 
    - The first ‘noun’ is the aggregator command 
    - The second ‘noun’ is optional and if present, defines the specific functionality 
    - The verb is the command name that is generally one of the following - 
        - list : command to list instances of a resource, backed server-side by a GET request.    
        - get-xxx: command to retrieve a single resource, backed server-side by a GET request.    

Currently, the Carbonaware SDK only supports GET/POST requests. We may have to add different commands if there are any CRUD operations added to the SDK. 

# Example

| CarbonAware CLI  
| Group (noun)   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Command (verb)   
| |	&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;	 Parameter   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Argument   
| | &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|   
$ emissions list  --locations eastus 


## Command Group

### `emissions`

The `emissions` keyword is used to invoke any commands related to getting Carbon Emissions data.

## Command Sub-groups

### `current-forecast`
### `batch-forecast`
### `average-intensity`
### `batch-average-intensity`

## Command Keywords

#### `list` 
Used when an array of instances are expected

#### `show`
Used when there a single instance is expected

# Carbon Aware CLI Commands  

## emissions list 

**Inputs:**

| Keyword       | Description                         | Optional? | Default | Example                                           |
| ------------- | :---------------------------------- | :-------- | :------ | :------------------------------------------------ |
| `--locations` | Space separated list of locations.  | No        | N/A     | `--locations "useast" "uswest"`                   |
| `--startTime` | Start time for emissions data       | Yes       | `null`  | `--startTime "2022-05-17T20:45:11.5092741+00:00"` |
| `--endTime`    | Ending time for emissions data.     | Yes       | `null`  | `--endTime "2022-05-17T20:45:11.5092741+00:00"`    |

**Outputs:**

Outputs a nested `JSON` object with the location, time, rating and duration of each window to `STD Out`. 

EG Input
```
.\CarbonAware.NewCLI.exe emissions list --locations eastus
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

## emissions current-forecast list

**Inputs:**

| Keyword       | Description                         | Optional? | Default | Example                                           |
| ------------- | :---------------------------------- | :-------- | :------ | :------------------------------------------------ |
| `--locations` | Space separated list of locations.  | No        | N/A     | `--locations "useast" "uswest"`                   |
| `--startTime` | Start time for emissions data       | Yes       | `null`  | `--startTime "2022-05-17T20:45:11.5092741+00:00"` |
| `--endTime`    | Ending time for emissions data.     | Yes       | `null`  | `--endTime "2022-05-17T20:45:11.5092741+00:00"`    |
| `--windowSize`| The estimated duration (in minutes) of the workload.     | Yes       | `null`  | `--windowSize 10`    |

**Outputs:**

Outputs a nested `JSON` object with the location, time, rating and duration of each window to `STD Out`. 

EG Input
```
.\CarbonAware.NewCLI.exe emissions current-forecast list --locations eastus
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

## emissions average-intensity show

**Inputs:**

| Keyword       | Description                         | Optional? | Default | Example                                           |
| ------------- | :---------------------------------- | :-------- | :------ | :------------------------------------------------ |
| `--locations` | Space separated list of locations.  | No        | N/A     | `--locations "useast" "uswest"`                   |
| `--startTime` | Start time for emissions data       | Yes       | `null`  | `--startTime "2022-05-17T20:45:11.5092741+00:00"` |
| `--endTime`    | Ending time for emissions data.     | Yes       | `null`  | `--toTime "2022-05-17T20:45:11.5092741+00:00"`    |

EG Input
```
.\CarbonAware.NewCLI.exe emissions average-intensity show --locations eastus
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