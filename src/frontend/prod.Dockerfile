FROM oven/bun:1.2-alpine AS base
WORKDIR /src

# install dependencies into temp directory
# this will cache them and speed up future builds
FROM base AS deps
RUN mkdir -p /temp/dev && mkdir -p /temp/prod

WORKDIR /temp/dev
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile

FROM base AS build
WORKDIR /src
COPY --from=deps /temp/dev/node_modules node_modules
COPY --from=deps /temp/dev/package.json .

COPY . .
RUN bun --bun run build

ENTRYPOINT [ "cp", "-r", "build" ]
CMD [ "/output" ]
