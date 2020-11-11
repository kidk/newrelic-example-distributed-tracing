#!/usr/bin/env bash

NEW_RELIC_LICENSE_KEY=$1
SERVICE_BUS=$2

echo "NEW_RELIC_LICENSE_KEY=$NEW_RELIC_LICENSE_KEY"
echo "SERVICE_BUS=$SERVICE_BUS"

dotnet publish

docker build -t dotnet-backend:latest .

docker run -e NEW_RELIC_LICENSE_KEY="$NEW_RELIC_LICENSE_KEY" -e SERVICE_BUS="$SERVICE_BUS" -p 5000:80 dotnet-backend:latest
