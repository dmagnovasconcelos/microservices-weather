#!/bin/brash
set -e

aws ecr get-login-password --region us-east-1 weather-ecr-agent | docker login --username AWS --password-stdin 614407438923.dkr.ecr.us-east-1.amazonaws.com
docker build -f ./Dockerfile -t cloud-weather-report:latest .
docker tag cloud-weather-report:latest 614407438923.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-report:latest
docker push 614407438923.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-report:latest