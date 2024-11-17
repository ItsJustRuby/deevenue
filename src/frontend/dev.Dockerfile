FROM oven/bun:1.1-alpine AS base
WORKDIR /src

# install dependencies into temp directory
# this will cache them and speed up future builds
FROM base AS install
RUN mkdir -p /temp/dev
WORKDIR /temp/dev
COPY --link package.json bun.lockb ./
RUN bun install --frozen-lockfile

# run the app
FROM base AS release
WORKDIR /src
COPY --from=install /temp/dev/node_modules node_modules
COPY --from=install /temp/dev/package.json .

EXPOSE 3000/tcp
ENTRYPOINT [ "bun", "--bun", "run" ]
CMD [ "dev", "--host" ]
