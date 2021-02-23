# Letmein on Google Cloud Kubernetes

## Initial Google Cloud Account Setup

- Sign in to Google Cloud: https://console.cloud.google.com/
- Add a credit card: https://console.cloud.google.com/billing to
- Create a project

## Google SDK setup

- Windows: https://cloud.google.com/sdk/docs/quickstart-windows
- Linux: 
    - `wget https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/google-cloud-sdk-129.0.0-linux-x86.tar.gz`
    - `tar xvf google-cloud-sdk-129.0.0-linux-x86.tar.gz`
- `./google-cloud-sdk/bin/gcloud components install kubectl`

The easiest authentication method is to generate a JSON file and use that. You can do this via the "..." menu for service accounts in IAM and "Create Key".

## Kubernetes setup

- Open a powershell window
- Put the JSON file you downloaded before into your current working directory.
- Create a GCE volume: https://console.cloud.google.com/compute/disks
- Add a cluster for project at https://console.cloud.google.com/kubernetes/
  - I used 3 micro instances. Up to 5 nodes are managed for free, you do pay for the VMs: https://cloud.google.com/products/calculator/ - $12 a month at the time of writing.

```
$env:HOME="$(pwd)" 
$env:GOOGLE_APPLICATION_CREDENTIALS="$(pwd)\google-cloud-credentials.json"
$cluster="cluster-sites"
$project="chris-kube"
gcloud config set compute/zone europe-west1-b 
gcloud config set project $project 
gcloud container clusters get-credentials $cluster --zone europe-west1-b --project $project
kubectl proxy
Now go to 127.0.0.1:8001/ui
```

### Docker images in the Google Registry (optonal)

This isn't for Letmein but is left here for reference.


#### Pushing

You can alter the steps for Travis, Appveyor, Gitlab etc. by putting the credentials JSON file inside a secret project variable, and echo'ing that to a JSON file on disk during the build.
Open a powershell window

```
$DOCKER_VERSION="1.0.0" 
$project="chris-kube" 
$registryUrl="gcr.io/$project/letmein:$DOCKER_VERSION"
docker build -t $registryUrl . 
docker login -e 1234@5678.com -u _json_key -p "$(cat google-cloud-credentials.json)" https://gcr.io 
docker push $registryUrl
```

#### Updating an image

Update to a new Docker image
$project="chris-kube"
$registryUrl="gcr.io/$project/letmein:5293dd"
$deploymentName="$project-node"

kubectl set image deployment/$deploymentName $deploymentName=$registryUrl