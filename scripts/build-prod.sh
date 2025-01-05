#!/bin/sh
set -ev

BACKEND_TAG="${DEEVENUE_BACKEND_TAG:-deevenue-prod-backend}"
ENTRYPOINT_TAG="${DEEVENUE_ENTRYPOINT_TAG:-deevenue-prod-entrypoint}"

docker build \
    -t "$BACKEND_TAG" \
    --secret id=sentry_auth_token,env=DEEVENUE_BUILD_SENTRY_AUTH_TOKEN \
    --secret id=sentry_org,env=DEEVENUE_BUILD_SENTRY_ORG \
    --secret id=sentry_project,env=DEEVENUE_BUILD_SENTRY_PROJECT \
    -f src/backend/prod.Dockerfile \
    src/backend

# shellcheck source=/dev/null
. "$(dirname "$0")/build-prod-frontend.sh"

docker build \
    -t "$ENTRYPOINT_TAG" \
    -f src/entrypoint/Dockerfile \
    src/entrypoint
