# Install WebApi in Toy AKS

## Pre
- Check for ARC and AKS services already set (azure portal)

- Build local container for development with the [devcontainer](./../.devcontainer/devcontainer.json) features: `kubectl-helm-minikube` and `azure-cli` so it is easier to run Helm, Kubectl and Azure CLI commands on the dev container.

- Build and push CarbonAware WebApi image using this [Dockerfile](../src/Dockerfile) to ACR
  (i.e using `docker`)
    ```sh
    cd src
    docker login <arcserver>.azurecr.io
    docker build -t ca_webapi-<ver>:<tag>
    docker tag ca_webapi_<ver>:<tag> <arcserver>.azurecr.io/ca_webapi_<ver>:<tag>
    docker push <arcserver>.azurecr.io/ca_webapi_<ver>:<tag>
    ```
  (i.e using `az acr build`)
    ```sh
    cd src
    az login --use-device-code
    az account set --subscription <>
    az acr build --image ca_webapi_<ver>:<tag> --registry <ACR Registry Name> --file Dockerfile .
    ```

- Create Helm charts default (one time only)
    ```sh
    helm create ca-deploy-charts
    ```
    - Modify `templates/deployment.yaml` and `values.yaml` with the appropiate information to pull from ACR and set liveness and readiness properties correctly.

- Check installation and running pod(s) with `kubectl`
    - Login to Azure
    ```sh
    az login  --use-device-code
    ```
    - Register subscription and set AKS credentials following the steps at `Connect to <MY_AKS>` on the azure portal.
    ```sh
    az account set --subscription <>
    az aks ...
    ...
    ```
    - Verify that `kubectl` can access AKS.
    ```sh
    kubectl get deployments --all-namespaces=true
    ```
    - Configure `helm` with ACR credentials. 
     ```sh
     helm registry login <arcserver>.azurecr.io --username <username> --password <passwd>
     ```
    - Deploy App using `helm` charts
    ```sh
    cd helm_webapi_deployment/ca-deploy-charts
    helm install ca-webapi-chart .
    ```

    - Verify that pod is running
    ```sh
    kubectl get po
    kubectl logs ca-webapi-chart-<id>
    kubectl describe po ca-webapi-<id>
    ```

- Connect to WebApi service based on the output after the installation of the chart

    ```sh
    Get the application URL by running these commands:
    export POD_NAME=$(kubectl get pods --namespace default -l "app.kubernetes.io/name=ca-webapi-helm,app.kubernetes.io/instance=ca-webapi-chart" -o jsonpath="{.items[0].metadata.name}")
    export CONTAINER_PORT=$(kubectl get pod --namespace default $POD_NAME -o jsonpath="{.spec.containers[0].ports[0].containerPort}")
    echo "Visit http://127.0.0.1:8080 to use your application"
    kubectl --namespace default port-forward $POD_NAME 8080:$CONTAINER_PORT
    ```
    On the local machine, use an http client and do for instance:
    ```sh
    curl http://127.0.0.1:8080/emissions/bylocation?location=eastus
    ```

- Setup env variables to access to backend service (i.e WattTime)
    - Create file to set environment variables (see [env-values.yml](./ca-deploy-charts/env-values.yaml))
    - Modify [deployment.yaml](./ca-deploy-charts/templates/deployment.yaml) to include reference to `env` names/values
    - Uninstall previous charts if that is the case
        ```sh
        helm uninstall ca-webapi-chart
        ```
    - Install chart with new env var values
        ```sh
        helm install -f env-values.yaml ca-webapi-chart .
        ```
    - Upgrade chart for instance when changes in the case base are made:
        ```sh
        helm upgrade -f env-values.yaml ca-webapi-chart .
        ```
      This will keep the same pod's ip address.
    - Set up again `POD_NAME` and `CONTAINER_PORT` in order to access the service
    - Perform http request (should see data coming from data source provider (i.e WattTime))

- Setup LoadBalancer
    - If the goal is to 'expose' the web service through a public ip, change the [values.yaml](./ca-deploy-charts/values.yaml) service:cluster type to `LoadBalancer` and redeploy.
    - AKS would display what ip can be used to access it. Use `kubectl get service` for instance

    ```sh
    kubectl get service
    NAME                 TYPE           CLUSTER-IP    EXTERNAL-IP      PORT(S)        AGE
    ...
    ca-webapi-chart      LoadBalancer   10.0.53.91    52.226.221.208   80:31753/TCP   18s
    ...

    ``` 
    - Request the data using the EXTERNAL-IP
    
    ```sh
    curl -v http://52.226.221.208/emissions/bylocation?location=eastus
    ```

## Using NGINX as LoadBalancer + Service ClusterIP

- Install nginx ingress (see ./install-ingress.sh)
- Use `ca-with-ingresslbl` as the example Helm chart to configure the service
  and the ingress controller.
    ```sh
    helm install -f env-values.yaml ca-with-ingress . --namespace ingress-basic
    ```

- Check LoadBalancer's External IP address, then modify the client:
    ```sh
    curl -v -s http://<LoadBalanceIP:PORT>/v1/sci-scores/marginal-carbon-intensity...."
    ```

Most of the configuration is at `values.yaml` file.

File `ca-with-ingress-ingress.yaml` is a ingress definition that can be deployed manually using `kubectl` instead of changing `values.yaml` default section of `ingress`
To use this file directly deploy using kubectl
```sh
kubectl apply -f ca-with-ingress-ingress.yaml --namespace ingress-basic
```

## References
[Helm local Instructions](../samples/helmexample/README.md)

[Helm quickstart](https://helm.sh/docs/intro/quickstart/)

[Helm env vars](https://jhooq.com/helm-pass-environment-variables/)

[Nginx - Azure](https://docs.microsoft.com/en-us/azure/aks/ingress-basic?tabs=azure-cli)
