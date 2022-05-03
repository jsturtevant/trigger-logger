# Trigger logger

This is a small application that listens to kubernetes events and starts a WPR listener when a namespace is created.  It could be extended for any kubernetes events and even beyond kubernetes such as when a process is started.  

This leverages Hostprocess container feature in kubernetes to run commands directly on the host.

## Hostpocess container

Build the container:

```
docker build -t jsturtevant/trigger-logger:latest .
```

Deploy:

```
kubectl apply -f trigger-logger.yaml
```

## Running locally

```
dotnet run
```

## Publishing as single file
Configured to produce a [single binary(https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file#publish-a-single-file-app---cli)].  To build run:

```
dotnet publish -r win-x64
```