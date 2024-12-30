#!/bin/sh
set -e

mkdir -p ./coverage
rm -rf ./coverage/*

if [ "$CI" != "" ]
then
    docker compose -f docker-compose.tests.github.yml up --build --exit-code-from "backend-tests"
else
    MSYS_NO_PATHCONV=1 docker compose -f docker-compose.tests.yml up --build --exit-code-from "backend-tests"
fi
