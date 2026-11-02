#!/bin/bash

[ $# -eq 0 ] && { echo "Usage: $0 release_version"; exit 1; }

IMAGE="${CI_REGISTRY_IMAGE,,}"

# BUILDING DOCKER IMAGES
echo "Building docker image: $IMAGE:$1..."
docker buildx build --platform linux/amd64,linux/arm64 \
  -t "$IMAGE:$1" \
  -t "$IMAGE:latest" \
  --push .