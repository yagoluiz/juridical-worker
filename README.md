# Juridical Worker

[![juridical-worker-deploy](https://github.com/yagoluiz/juridical-worker/actions/workflows/juridical-worker-deploy.yaml/badge.svg)](https://github.com/yagoluiz/juridical-worker/actions/workflows/juridical-worker-deploy.yaml)

Worker responsible for identify legal process.

## Environment settings local

### .NET

- Use [dotnet user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)

1) Get project folder legal process worker:

```bash
src/Juridical.LegalProcess.Worker
```

2) Create secrets:

```bash
dotnet user-secrets set "LEGAL_PROCESS_USER" "YOUR_SECRET"
dotnet user-secrets set "LEGAL_PROCESS_PASSWORD" "YOUR_SECRET"
```

1) Get project folder message worker:

```bash
src/Juridical.Message.Worker
```

2) Create secrets:

```bash
dotnet user-secrets set "MESSAGE_SERVICE_API_TOKEN" "YOUR_SECRET"
dotnet user-secrets set "MESSAGE_SERVICE_FROM" "YOUR_SECRET"
dotnet user-secrets set "MESSAGE_SERVICE_TO" "YOUR_SECRET"
```

### Docker

- Create **.env** file

```bash
PROJECT_ID=juridical-test
PUBSUB_EMULATOR_HOST=127.0.0.1:8085
WEB_DRIVER_URI=http://juridical-selenium:4444/wd/hub
LEGAL_PROCESS_USER=YOUR_SECRET
LEGAL_PROCESS_PASSWORD=YOUR_SECRET
MESSAGE_SERVICE_API_TOKEN=YOUR_SECRET
MESSAGE_SERVICE_FROM=YOUR_SECRET
MESSAGE_SERVICE_TO=YOUR_SECRET
```

## Instructions for run project

### Pub/Sub Emulator

1) Run pub/sub emulator:

```bash
cd emulators/ && docker-compose up -d
```

2) Publish message:

```bash
docker exec -it juridical-pubsub-emulator /bin/bash
```

```bash
python3 /root/bin/pubsub-client.py publish ${PUBSUB_PROJECT_ID} juridical.legal-process.resulted '{ "ProcessCount": 1 }'
```

### .NET

1) Run selenium:

```bash
docker run -d -p 4444:4444 -p 7900:7900 --shm-size="2g" -e VNC_NO_PASSWORD=1 --name selenium selenium/standalone-chrome:110.0
```

2) Run projects:

```bash
cd src/Juridical.LegalProcess.Worker && dotnet watch run
```

```bash
cd src/Juridical.Message.Worker && dotnet watch run
```

### Docker

- Run project

```bash
docker-compose up -d
```

### Push images (optional)

- Create [Container Registry (GCP)](https://cloud.google.com/container-registry/docs/pushing-and-pulling)

1) Configure auth [GCP CLI](https://cloud.google.com/sdk/gcloud) login:

```bash
gcloud auth login
```

2) Configure auth configure docker:

```bash
gcloud auth configure-docker
```

3) Push images for private registry:

```bash
docker build \
  -f ./src/Juridical.LegalProcess.Worker/Dockerfile \
  -t juridical/juridical-legal-process-worker:v1 \
  ./src/ &&
docker tag juridical/juridical-legal-process-worker:v1 gcr.io/$PROJECT_ID/juridical-legal-process-worker:v1 &&
docker push gcr.io/$PROJECT_ID/juridical-legal-process-worker:v1
```

```bash
docker build \
  -f ./src/Juridical.Message.Worker/Dockerfile \
  -t juridical/juridical-message-worker:v1 \
  ./src/ &&
docker tag juridical/juridical-message-worker:v1 gcr.io/$PROJECT_ID/juridical-message-worker:v1 &&
docker push gcr.io/$PROJECT_ID/juridical-message-worker:v1
```

## Infrastructure

### Terraform

- Create service account from [GCP](https://cloud.google.com/iam/docs/creating-managing-service-accounts)

1) Create service account:

```bash
gcloud iam service-accounts create $SERVICE_ACCOUNT_NAME \
  --display-name "$SERVICE_ACCOUNT_DISPLAY_NAME" --project $PROJECT_ID
```

2) Get service account email:

```bash
gcloud iam service-accounts list
```

3) Create credentials key:

```bash
# SERVICE_ACCOUNT_CREDENTIALS=~/.config/gcloud/CREDENTIALS_FILE_NAME.json

gcloud iam service-accounts keys create $SERVICE_ACCOUNT_CREDENTIALS \
  --iam-account $SERVICE_ACCOUNT_EMAIL
```

4) Add policy permissions:

```bash
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/storage.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/artifactregistry.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/container.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/iam.serviceAccountUser
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/viewer
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/pubsub.admin
```

- Run local infrastructure

1) Install [Terraform](https://www.terraform.io/downloads.html) and create GOOGLE_CREDENTIALS variable:

```bash
export GOOGLE_CREDENTIALS=~/.config/gcloud/CREDENTIALS_FILE_NAME.json
```

2) Execute init:

```bash
cd infra/ && terraform init
```

3) Execute apply:

```bash
terraform apply \
  -var="project_id=$PROJECT_ID" \
  -var="service_account=$SERVICE_ACCOUNT_EMAIL"
```

- (Optional) Create remote backend bucket in Cloud Storage:

1) Create bucket:

```bash
gsutil mb -p $PROJECT_ID -l $LOCATION -b on gs://$BUCKET_NAME
```

## Deploy

### GitHub Actions

- Create service account from [GCP](https://cloud.google.com/iam/docs/creating-managing-service-accounts)

1) Create service account:

```bash
gcloud iam service-accounts create $SERVICE_ACCOUNT_NAME \
  --display-name "$SERVICE_ACCOUNT_DISPLAY_NAME" --project $PROJECT_ID
```

2) Enable IAM Credentials:

```bash
gcloud services enable iamcredentials.googleapis.com --project $PROJECT_ID
```

3) Get service account email:

```bash
gcloud iam service-accounts list
```

4) Add policy permissions:

```bash
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/container.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/storage.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/artifactregistry.admin
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/container.clusterViewer
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/logging.logWriter
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/monitoring.metricWriter
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/stackdriver.resourceMetadata.writer
gcloud projects add-iam-policy-binding $PROJECT_ID \
	--member=serviceAccount:$SERVICE_ACCOUNT_EMAIL \
	--role=roles/pubsub.admin
```

- Enabling keyless authentication from [GitHub Actions GCP](https://cloud.google.com/blog/products/identity-security/enabling-keyless-authentication-from-github-actions)

1) Create Workload Identity pool:

```bash
gcloud iam workload-identity-pools create "$POOL_NAME" \
  --project="$PROJECT_ID" \
  --location="global" \
  --display-name="$POOL_DISPLAY_NAME"
```

2) Get Workload Identity Id:

```bash
gcloud iam workload-identity-pools describe "$POOL_NAME" \
  --project="$PROJECT_ID" \
  --location="global" \
  --format="value(name)"
```

3) Create Workload Identity GitHub provider:

```bash
gcloud iam workload-identity-pools providers create-oidc "$PROVIDER_NAME" \
  --project="$PROJECT_ID" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --display-name="$PROVIDER_DISPLAY_NAME" \
  --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" \
  --issuer-uri="https://token.actions.githubusercontent.com
```

4) Create authentications from the Workload Identity provider:

```bash
gcloud iam service-accounts add-iam-policy-binding "$SERVICE_ACCOUNT_EMAIL" \
  --project="$PROJECT_ID" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/$WORKLOAD_IDENTITY_POOL_ID/attribute.repository/$GITHUB_USER/$GITHUB_REPOSITORY"
```

5) Get Workload Identity Provider resource name:

```bash
gcloud iam workload-identity-pools providers describe "$PROVIDER_NAME" \
  --project="$PROJECT_ID" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --format="value(name)"
```
