#!/bin/sh
mkdir -p /results/coverage

dotnet test --collect="XPlat Code Coverage" \
    --settings coverage.runsettings \
    --results-directory /results/coverage

./reportgenerator/reportgenerator \
    "-reports:/results/coverage/*/*.xml" \
     "-reporttypes:Cobertura" \
     "-targetdir:/results/report"
