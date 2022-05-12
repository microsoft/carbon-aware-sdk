# Python proxy
This folder contains a simple python proxy server which can be used to manually test the proxy connection of the SDK. 

## Getting the server address if running in a dev container

If you run the server in a devcontainer, make sure to see on what docker ip is running on the host.
Use the following command on the host running docker
```sh
docker ps
CONTAINER ID   IMAGE                                                   COMMAND                  CREATED          STATUS          PORTS                    NAMES
58db33d661be   vsc-python-proxy-ca6316bf94ea9aa0d4c49b40dcea4137       "/bin/sh -c 'echo Co…"   22 minutes ago   Up 22 minutes                            keen_ardinghelli
....
```

Select `CONTAINER ID`
```sh
docker inspect 58db33d661be
```

```json
"NetworkSettings": {
            "Bridge": "",
            "SandboxID": "50f05a67594ec49e2c8df5698ccb324fb9ffe2f1fd3b85d484dce1bc1e1c1cc2",
...
            "Networks": {
                "bridge": {
                    ....
                    "Gateway": "172.17.0.1",
                    "IPAddress": "172.17.0.4",
                    ....
                }
            }
...
```
Select `IPAddress` for your http_proxy, for instance on another container:
```sh
export http_proxy=http://172.17.0.4:8080
curl -v www.microsoft.com
```

## References

[Proxy Protocol](http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt)
