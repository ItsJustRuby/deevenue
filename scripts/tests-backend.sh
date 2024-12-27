#!/bin/sh

mkdir -p ./coverage
rm -rf ./coverage/*
docker compose -f docker-compose.tests.yml up --build
