# Juridical Worker

Worker responsible for identify legal process.

## Environment settings 

### .NET

- Use [dotnet user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)

1) Get project folder:

```bash
src/Juridical.Worker
```

2) Create secrets:

```bash
dotnet user-secrets set "LEGAL_PROCESS_USER" "{YOUR_SECRET}"
dotnet user-secrets set "LEGAL_PROCESS_PASSWORD" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_API_TOKEN" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_FROM" "{YOUR_SECRET}"
dotnet user-secrets set "MESSAGE_SERVICE_TO" "{YOUR_SECRET}"
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

### .NET

1) Run selenium:

```bash
docker run -d -p 4444:4444 -p 7900:7900 --shm-size="2g" --name selenium selenium/standalone-chrome:4.1.1-20220121
```

2) Run project:

```bash
src/Juridical.Worker && dotnet watch run
```

### Docker

- Run project

```bash
docker-compose up -d
```

### Kubernetes

- Create [Container Registry (GCP)](https://cloud.google.com/container-registry/docs/pushing-and-pulling)

1) Configure auth [GCP CLI](https://cloud.google.com/sdk/gcloud) login:

```bash
gcloud auth login
```

2) Configure auth configure docker:

```bash
gcloud auth configure-docker
```

3) Push image for private registry:

```bash
docker build -t juridical/juridical-worker:v1 .
docker tag juridical/juridical-worker:v1 gcr.io/{PROJECT_ID}/juridical-worker:v1
docker push gcr.io/{PROJECT_ID}/juridical-worker:v1
```

4) Set image worker local deployment:

```yaml
...
      containers:
        - name: juridical-worker
          image: gcr.io/{PROJECT_ID}/juridical-worker:latest
...
```

- Run minikube

1) Start cluster:

```bash
minikube start
```

2) (Optional) Open dashboard:

```bash
minikube dashboard
```

3) Run k8s files:

```bash
sh k8s/deploy.sh
```

## Deploy

### GitHub Actions

- Create service account from [GCP](https://cloud.google.com/iam/docs/creating-managing-service-accounts)

1) Create service account:

```bash
gcloud iam service-accounts create "$SERVICE_ACCOUNT_NAME" --display-name "$SERVICE_ACCOUNT_DISPLAY_NAME" --project "$PROJECT_ID"
```

2) Enable IAM Credentials:

```bash
gcloud services enable iamcredentials.googleapis.com --project "${PROJECT_ID}"
```

3) Get service account email:

```bash
gcloud iam service-accounts list
```

4) Add policy permissions:

```bash
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
	--member=serviceAccount:"$SERVICE_ACCOUNT_EMAIL" \
	--role=roles/container.admin
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
	--member=serviceAccount:"$SERVICE_ACCOUNT_EMAIL" \
	--role=roles/storage.admin
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
	--member=serviceAccount:"$SERVICE_ACCOUNT_EMAIL" \
	--role=roles/container.clusterViewer
```

- Enabling keyless authentication from [GitHub Actions GCP](https://cloud.google.com/blog/products/identity-security/enabling-keyless-authentication-from-github-actions)

1) Create Workload Identity pool:

```bash
gcloud iam workload-identity-pools create "$POOL_NAME" \
  --project="${PROJECT_ID}" \
  --location="global" \
  --display-name="$POOL_DISPLAY_NAME"
```

2) Get Workload Identity Id:

```bash
gcloud iam workload-identity-pools describe "$POOL_NAME" \
  --project="${PROJECT_ID}" \
  --location="global" \
  --format="value(name)"
```

3) Create Workload Identity GitHub provider:

```bash
gcloud iam workload-identity-pools providers create-oidc "$PROVIDER_NAME" \
  --project="${PROJECT_ID}" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --display-name="$PROVIDER_DISPLAY_NAME" \
  --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" \
  --issuer-uri="https://token.actions.githubusercontent.com
```

4) Create authentications from the Workload Identity provider:

```bash
gcloud iam service-accounts add-iam-policy-binding "$SERVICE_ACCOUNT_EMAIL" \
  --project="${PROJECT_ID}" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/${WORKLOAD_IDENTITY_POOL_ID}/attribute.repository/${GITHUB_USER}/${GITHUB_REPOSITORY}"
```

5) Get Workload Identity Provider resource name:

```bash
gcloud iam workload-identity-pools providers describe "$PROVIDER_NAME" \
  --project="${PROJECT_ID}" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --format="value(name)"
```
