#!/bin/sh
set -ev

docker build \
    -t deevenue-prod-frontend-generator \
    -f src/frontend/prod.Dockerfile \
    src/frontend

rm -rf "$(pwd)/src/entrypoint/dist"
mkdir -p "$(pwd)/src/entrypoint/dist"

MSYS_NO_PATHCONV=1 docker run --rm \
    --volume "$(pwd)/src/entrypoint/dist:/output" \
    deevenue-prod-frontend-generator
