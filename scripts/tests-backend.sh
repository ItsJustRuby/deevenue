#!/bin/sh

mkdir -p ./coverage
rm -rf ./coverage/*
MSYS_NO_PATHCONV=1 docker compose -f docker-compose.tests.yml up --build
