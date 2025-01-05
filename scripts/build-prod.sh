#!/bin/sh
set -ev

BACKEND_TAG="${DEEVENUE_BACKEND_TAG:-deevenue-prod-backend}"
ENTRYPOINT_TAG="${DEEVENUE_ENTRYPOINT_TAG:-deevenue-prod-entrypoint}"

docker build \
    -t "$BACKEND_TAG" \
    --secret id=SENTRY_AUTH_TOKEN,env=SENTRY_AUTH_TOKEN \
    --secret id=SENTRY_ORG,env=SENTRY_ORG \
    --secret id=SENTRY_PROJECT,env=SENTRY_PROJECT \
    -f src/backend/prod.Dockerfile \
    src/backend

# shellcheck source=/dev/null
. "$(dirname "$0")/build-prod-frontend.sh"

docker build \
    -t "$ENTRYPOINT_TAG" \
    -f src/entrypoint/Dockerfile \
    src/entrypoint
