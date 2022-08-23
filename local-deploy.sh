#!/bin/bash

PATH_DEFAULT=k8s

kubectl apply -f $PATH_DEFAULT/namespaces/juridical-namespace.yaml

kubectl create secret generic juridical-legal-process-secret -n juridical --from-env-file=.env
kubectl create secret generic juridical-message-secret -n juridical --from-env-file=.env

kubectl apply -f $PATH_DEFAULT/deployments/juridical-selenium-deployment.yaml
kubectl apply -f $PATH_DEFAULT/deployments/juridical-worker-deployment.yaml

kubectl apply -f $PATH_DEFAULT/services/juridical-selenium-service.yaml
