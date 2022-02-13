PATH_DEFAULT=k8s

kubectl apply -f $PATH_DEFAULT/juridical-namespace.yaml

kubectl create secret generic juridical-worker-secret -n juridical --from-env-file=.env

kubectl apply -f $PATH_DEFAULT/juridical-selenium-deployment.yaml
kubectl apply -f $PATH_DEFAULT/juridical-worker-deployment.yaml

kubectl apply -f $PATH_DEFAULT/juridical-selenium-service.yaml
