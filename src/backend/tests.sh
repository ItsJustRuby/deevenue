#!/bin/sh
set -e

mkdir -p /results/coverage

dotnet test --collect="XPlat Code Coverage" \
    --settings coverage.runsettings \
    --results-directory /results/coverage

./reportgenerator/reportgenerator \
    "-reports:/results/coverage/*/*.xml" \
     "-reporttypes:lcov" \
     "-targetdir:/results/report"
