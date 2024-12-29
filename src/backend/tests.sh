#!/bin/sh
mkdir -p /results/coverage

dotnet test --collect="XPlat Code Coverage" \
    --settings coverage.runsettings \
    --results-directory /results/coverage

# TODO: Test if codecov prefers lcov or Cobertura.
./reportgenerator/reportgenerator \
    "-reports:/results/coverage/*/*.xml" \
     "-reporttypes:lcov" \
     "-targetdir:/results/report"
    #  "-reporttypes:lcov;Cobertura" \
