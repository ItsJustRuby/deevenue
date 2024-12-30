#!/bin/sh
set -e

mkdir -p ./coverage
rm -rf ./coverage/*


if [ "$CI" != "" ]
then
    BUILDKIT_PROGRESS=plain docker compose -f docker-compose.tests.github.yml up --build
else
    MSYS_NO_PATHCONV=1 docker compose -f docker-compose.tests.yml up --build
fi
