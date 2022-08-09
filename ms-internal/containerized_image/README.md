# Containerized CarbonAware WebApi

The goal of this readme is to show how to build a container WebApi image that can be used to deploy the application into a container registry and that can be used later to run the service.

## Build Runtime Image

(Note: Make sure the run docker at the root of the branch)

```sh
cd <root_branch>
docker build -t carbon_aware -f ms-internal/containerized_image/Dockerfile .
```

## Upload to Container Registry

### Docker Hub


### Azure Container Registry


## Pipeline integration (Github Action)

