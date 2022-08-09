# Containerized CarbonAware WebApi

The goal of this readme is to show how to build a container WebApi image that can be used to deploy the application into a container registry and that can be used later to run the service.

## Build and List Runtime Image

Use `docker` to build the WebApi images.
(Note: Make sure the run docker at the root branch)

```sh
cd ./$(git rev-parse --show-cdup)
docker build -t carbon_aware:v1 -f ms-internal/containerized_image/Dockerfile .
```

List `carbon_aware` image 

```sh
docker image ls carbon_aware
REPOSITORY     TAG       IMAGE ID       CREATED             SIZE
carbon_aware   v1        6293e2528bf2   About an hour ago   230MB
```

## Run WebApi Image

1. For instance, the following command uses localhost port 8000 mapped to the WebApi port 80 and setting [WattTime](https://www.watttime.org) credentials.

    ```sh
    docker run --rm -p 8000:80 \
    > -e CarbonAwareVars__CarbonIntensityDataSource="WattTime" \
    > -e WattTimeClient__Username="username" \
    > -e WattTimeClient__Password="userpwd" \
    > carbon_aware:v1
    ```
1. Verify that the WebApi is responding to requests, either using a Rest Client (e.g. postman, curl)
    ```sh
    curl -v -s -X 'POST' http://localhost:8000/emissions/forecasts/batch  -H 'accept: */*' -H 'Content-Type: application/json' -d '[
        {
            "requestedAt": "2021-11-01T00:00:00Z",
            "dataStartAt": "2021-11-01T00:05:00Z",
            "dataEndAt": "2021-11-01T23:55:00Z",
            "windowSize": 5,
            "location": "eastus"
        }
    ]'
    ...
    > POST /emissions/forecasts/batch HTTP/1.1
    > Host: localhost:8000
    ...
    < HTTP/1.1 200 OK
    < Content-Type: application/json; charset=utf-8
    ...
    < 
    [{"generatedAt":"2021-11-01T00:00:00+00:00","optimalDataPoint":{
        ...
    }]
    ```

## Upload image to a Container Registry

For easy image consumption, upload it to a well known Container Registry, either local or cloud provider. The following are examples of using [docker hub](https://hub.docker.com) or [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-quickstart-task-cli)
### Docker Hub

### Azure Container Registry

Assuming there is Container Registry created, using the user's credentials, lets push the image using `docker` (there other way using for instance `Azure CLI`)
```sh
docker login <myacrname>.azurecr.io -u username -p <CopiedKeyFromPortal>
cd ./$(git rev-parse --show-cdup)
docker build -t <myacrname>.azurecr.io/carbon_aware:v1 -f ms-internal/containerized_image/Dockerfile .
docker push <myacrname>.azurecr.io/carbon_aware:v1
```


## Pipeline integration (Github Action)
