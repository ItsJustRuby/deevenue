#!/bin/bash
bunx openapi-typescript \
    "${PWD}/development/openAPI/Deevenue.Api.json" \
    -o "${PWD}/src/frontend/src/lib/api/oats/schema.d.ts"
