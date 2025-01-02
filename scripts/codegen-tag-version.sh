#!/bin/bash

get_constants() {
    echo "{ \"version\": \"$(git rev-parse --short HEAD)\" }"
}

# Manually diff the constants.json file and see if it needs updating first.
# Otherwise this would bust the cache for the Docker build.
diff \
    src/frontend/constants.json \
    <(get_constants) \
    > /dev/null

HAS_DIFF=$?
if [ $HAS_DIFF -ne 0 ]; then
    get_constants > src/frontend/constants.json
fi
