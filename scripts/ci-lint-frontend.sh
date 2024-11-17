#!/bin/sh
set -e

bun install --cwd src/frontend --frozen-lockfile
bun run --cwd src/frontend format
bun run --cwd src/frontend lint
bun run --cwd src/frontend check
