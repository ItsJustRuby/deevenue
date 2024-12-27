#!/bin/bash

env COMPOSE_PATH_SEPARATOR=: \
    COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml \
    docker compose up -d --build
