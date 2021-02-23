[1]: https://www.digitalocean.com/community/tutorials/an-introduction-to-kubernetes

# Kubernetes deployment of letmein

You can run Letmein in a Kubernetes cluster fairly easily, details below are tested on a local Kubernetes cluster.

There is also some instructions for doing it on Google Cloud which is probably the least friction of the Kubernetes as a service offerings.

[This Digital Ocean article][1] has a really good run down of Kubernetes architecture.

## Run a single docker container (a pod)

```
$deploymentName="letmein"
$registryUrl="anotherchris/letmein:latest"

kubectl run $deploymentName --image=$registryUrl `
--port=5000 `
--env="REPOSITORY_TYPE=Postgres" `
--env='POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein"' `
--env="ASPNETCORE_URLS=http://0.0.0.0:5000" `
--requests "cpu=1"

kubectl delete pod $deploymentName
```

## Creating and running the deployments

The deployment YAML files in this folder combine the service, volumes and deployments into a single YAML file.

There are two services, one for Postgres, one for the website. For production environments it'd be better to use Postgres on something like AWS RDS, and to have NGinx infront of Letmein.

### 1. Deployment and services
```
kubectl create -f .\letmein-postgres-deployment.yml
kubectl create -f .\letmein-website-deployment.yml
```

To see the service locally:
```
minikube service letmein-website
```

Applying changes or editing the deployments:

```
kubectl apply -f ./letmein-website-deployment.yml
kubectl edit deployment letmein--website-deployment.yml
```

### 2. Ingress (expose the service)
```
kubectl create -f .\letmein-ingress.yml
kubectl get ingress letmein-ingress
kubectl describe ingress.networking.k8s.io/letmein-ingress
```

### 3. Tearing down
```
kubectl delete deployment/letmein-postgres;kubectl delete deployment/letmein-website; kubectl delete service/letmein-postgres; kubectl delete service/letmein-website

kubectl delete PersistentVolumeClaim/postgres-pv-claim
kubectl delete ingress.networking.k8s.io/letmein-ingress
```

### Additional resources and links

#### Local Kubernetes
*(Disable Docker Desktop Kubernetes, Minikube is a lot easier).*

Windows:
```
choco install minikube
minikube start
minikube dashboard
```

For development, install the Kubernetes VSCode extension.

#### How resources are defined in Kubernetes

> CPU resources are defined in millicores. If your container needs two full cores to run, 
you would put the value "2000m". If your container only needs ¼ of a core, you would 
put a value of "250m".


#### Debugging your DNS

```
kubectl apply -f https://k8s.io/examples/admin/dns/dnsutils.yaml
kubectl exec -i -t dnsutils -- nslookup kubernetes.default
kubectl exec -i -t dnsutils -- nslookup letmein-postgres
```

#### Links

- [Deployment docs](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Env variable data](https://kubernetes.io/docs/tasks/inject-data-application/)
- [Kubernetes resources details](https://cloud.google.com/blog/products/containers-kubernetes/kubernetes-best-practices-resource-requests-and-limits)
- [Example Wordpress/MySQL deployment](https://kubernetes.io/docs/tutorials/stateful-application/mysql-wordpress-persistent-volume/)

