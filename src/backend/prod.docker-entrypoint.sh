#!/bin/sh
set -e

wait-for "$DEEVENUE_DB_HOST:5432"
CONNECTION="Host=$DEEVENUE_DB_HOST;Database=$DEEVENUE_DB_DB;Username=$DEEVENUE_DB_USER;Password=$DEEVENUE_DB_PASSWORD;IncludeErrorDetail=true"
./Deevenue.Migrations --connection "$CONNECTION"

./Deevenue.Api
