# Install mosquitto client
sudo apt-add-repository ppa:mosquitto-dev/mosquitto-ppa -y
sudo apt-get update && sudo apt-get install mosquitto-clients mosquitto ninja-build libmosquitto-dev uuid-dev libjson-c-dev libprotobuf-c-dev -y

#Install step cli
wget https://github.com/smallstep/cli/releases/download/v0.24.4/step-cli_0.24.4_amd64.deb
sudo dpkg -i step-cli_0.24.4_amd64.deb
rm step-cli_0.24.4_amd64.deb

az extension add --name azure-iot

k3d cluster delete
k3d registry create registry.localhost --port 5500
k3d cluster create  \
            -p '1883:1883@loadbalancer' \
            -p '8883:8883@loadbalancer' 
