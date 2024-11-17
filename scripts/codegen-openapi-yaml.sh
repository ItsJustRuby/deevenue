#!/bin/bash

# Generate OpenAPI definitions
docker build \
    -t deevenue-prod-backend-openapi \
    -f src/backend/openAPI.Dockerfile \
    src/backend

# Copy OpenAPI definitions out of container
# Workaround for Windows/git bash specifically to do path expansion correctly
MSYS_NO_PATHCONV=1 docker run --rm \
    --volume "$(pwd)/development/openAPI:/output" \
    deevenue-prod-backend-openapi
