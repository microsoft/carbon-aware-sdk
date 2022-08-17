The Carbon Aware SDK provides a CLI to get the marginal carbon intensity for a given location and time period. The values reported in the Green Software Foundation's specification for marginal carbon intensity (Grams per Kilowatt Hour). In order to use the API, the environment needs to be prepared with a set of configuration parameters. Instructions on setting up the environment could be found here - https://github.com/microsoft/carbon-aware-sdk/blob/dev/GettingStarted.md

# Carbon Aware CLI Commands
- [Carbon Aware CLI Commands](#carbon-aware-cli-commands)
  - [Command Keywords](#command-keywords)
    - [`emissions`](#emissions)
      - [`list`](#list)
  
## Command Keywords

### `emissions`

The `emissions` keyword is used to invoke any commands related to getting Carbon Emissions data.

#### `list`

**Inputs:**

| Keyword       | Description                         | Optional? | Default | Example                                           |
| ------------- | :---------------------------------- | :-------- | :------ | :------------------------------------------------ |
| `--locations` | Space separated list of locations.  | No        | N/A     | `--locations "useast" "uswest"`                   |
| `--startTime` | Start time for emissions data       | Yes       | `null`  | `--startTime "2022-05-17T20:45:11.5092741+00:00"` |
| `--toTime`    | Ending time for emissions data.     | Yes       | `null`  | `--toTime "2022-05-17T20:45:11.5092741+00:00"`    |
| `--best`      | Bool Filter to give the best result | Yes       | `false` | `--best true`                                     |

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