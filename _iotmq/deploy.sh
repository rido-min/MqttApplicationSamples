helm install mq oci://e4kpreview.azurecr.io/helm/az-e4k --version 0.1.0-preview-rc4
kubectl create secret tls iotmq-tls-secret --cert localhost.crt --key localhost.key
kubectl create configmap client-ca-configmap --from-file chain.pem
kubectl apply -f iotmq.yaml