#!/bin/sh

if ! command -v caddy 2>&1 /dev/null; then
  echo "Error: 'caddy' not found in PATH. Please install caddy before running this script, e.g. via chocolatey."
  exit 1
fi

cleanup() {
    docker compose -f ./docker-compose.yml -f ./docker-compose.dev.yml -f ./development/setup/docker-compose.setup.yml down setup > /dev/null
}
trap cleanup EXIT

docker compose -f ./docker-compose.yml -f ./docker-compose.dev.yml -f ./development/setup/docker-compose.setup.yml up -d setup > /dev/null

caddy trust
