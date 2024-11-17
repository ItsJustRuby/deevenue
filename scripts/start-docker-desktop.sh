#!/bin/bash

if [ "$CI" != "" ]
then
    echo "Running on CI, assuming that the docker daemon is already running."
    exit 0
fi

/c/Program\ Files/Docker/Docker/Docker\ Desktop.exe

until docker info > /dev/null 2>&1
do
  sleep 1
done
