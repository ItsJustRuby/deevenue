#!/bin/sh
set -e

wait-for "$DEEVENUE_DB_HOST:5432"
CONNECTION="Host=$DEEVENUE_DB_HOST;Database=$DEEVENUE_DB_DB;Username=$DEEVENUE_DB_USER;Password=$DEEVENUE_DB_PASSWORD;IncludeErrorDetail=true"
./Deevenue.Migrations --connection "$CONNECTION"

./Deevenue.Api &
SERVER_PID=$!

if [ -d "/import" ]; then
  echo "Found /import directory, importing demo data."
  /cli/Deevenue.Cli import /import
fi

# Note that this does leave two processes running in the container:
# Deevenue.Api and this shell.
# Unfortunately, since Deevenue.Api is this shell process's child process,
# there is not really a way to yield to it. Neither nohup nor disown
# seem to do what I want here.
wait $SERVER_PID
