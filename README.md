# Juridical Worker

Worker responsible for identify legal process.

## Environment settings 

### .NET CLI

- Use [dotnet user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)

```bash
src/Juridical.Worker

dotnet user-secrets set "LEGAL_PROCESS_USER" "{YOUR_SECRET}"
dotnet user-secrets set "LEGAL_PROCESS_PASSWORD" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_API_TOKEN" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_FROM" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_TO" "{YOUR_SECRET}"

dotnet user-secrets list
```

### Docker and Kubernetes

- Create **.env** file

```bash
LEGAL_PROCESS_USER={YOUR_SECRET}
LEGAL_PROCESS_PASSWORD={YOUR_SECRET}
MESSAGE_SERVICE_API_TOKEN={YOUR_SECRET}
MESSAGE_SERVICE_FROM={YOUR_SECRET}
MESSAGE_SERVICE_TO={YOUR_SECRET}
```

## Instructions for run project

### .NET CLI

- Run selenium

```bash
docker run -d -p 4444:4444 -p 7900:7900 --shm-size="2g" --name selenium selenium/standalone-chrome:4.1.1-20220121
```

- Run project

```bash
src/Juridical.Worker

dotnet watch run
```
### Docker

```bash
docker-compose up -d
```

### Kubernetes

- Create Container Registry (GCP)

```bash
gcloud auth login

gcloud auth configure-docker
```
- Push image for registry

```bash
docker build -t juridical/juridical-worker:v1 .

docker tag juridical/juridical-worker:v1 gcr.io/{YOUR_GCP_PROJECT}/juridical-worker:v1

docker push gcr.io/{YOUR_GCP_PROJECT}/juridical-worker:v1
```

- Run minikube

```bash
minikube start
```

```bash
minikube dashboard
```

- Run files

```bash
sh k8s/deploy.sh
```
