#!/usr/bin/env bash

set -e
set -o pipefail

if [ $# -eq 0 ]
  then
    echo "Usage: build.sh [version]"
    exit 1
fi

echo 'Copy Common'
#mkdir Common
#cp -r ../Common/* ./Common/

echo 'Building Images'
docker build --no-cache -t asia.gcr.io/$NAMESPACE/$SERVICE:$1 .

echo 'Scanning Images'
trivy image asia.gcr.io/$NAMESPACE/$SERVICE:$1

echo 'Pushing Images to registry'
docker push asia.gcr.io/$NAMESPACE/$SERVICE:$1

echo 'Removing Images'
docker rmi asia.gcr.io/$NAMESPACE/$SERVICE:$1

echo 'Removing Common'
#rm -rf Common
