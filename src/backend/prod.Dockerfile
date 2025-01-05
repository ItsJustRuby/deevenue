# syntax=docker/dockerfile:1.10
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN dotnet tool install dotnet-ef --tool-path /src/dotnet-ef
ENV PATH="$PATH:/src/dotnet-ef"

# Copy project file separately to restore dependencies
COPY --link Deevenue.Api/Deevenue.Api.csproj ./Deevenue.Api/
COPY --link Deevenue.Cli/Deevenue.Cli.csproj ./Deevenue.Cli/
COPY --link Deevenue.Infrastructure/Deevenue.Infrastructure.csproj ./Deevenue.Infrastructure/
COPY --link Deevenue.Domain/Deevenue.Domain.csproj ./Deevenue.Domain/
RUN dotnet restore Deevenue.Api/Deevenue.Api.csproj \
    && dotnet restore Deevenue.Cli/Deevenue.Cli.csproj

# Copy source code and publish app
COPY --link . .

RUN --mount=type=secret,id=SENTRY_ORG,env=SENTRY_ORG \
    --mount=type=secret,id=SENTRY_PROJECT,env=SENTRY_PROJECT \
    --mount=type=secret,id=SENTRY_AUTH_TOKEN,env=SENTRY_AUTH_TOKEN \
    echo $SENTRY_ORG && echo $SENTRY_PROJECT && echo $SENTRY_AUTH_TOKEN && false

RUN --mount=type=secret,id=SENTRY_ORG,env=SENTRY_ORG \
    --mount=type=secret,id=SENTRY_PROJECT,env=SENTRY_PROJECT \
    --mount=type=secret,id=SENTRY_AUTH_TOKEN,env=SENTRY_AUTH_TOKEN \
    mkdir -p /app /cli && \
    dotnet publish --no-restore --configuration Release \
    /p:TreatWarningsAsErrors=true \
    /p:SentryOrg="$SENTRY_ORG" /p:SentryProject="$SENTRY_PROJECT" \
    /p:SentryUploadSymbols=true /p:SentryUploadSources=true \
    Deevenue.Api/ -o /app && \
    dotnet publish --no-restore --configuration Release \
    /p:TreatWarningsAsErrors=true \
    /p:SentryOrg="$SENTRY_ORG" /p:SentryProject="$SENTRY_PROJECT" \
    /p:SentryUploadSymbols=true /p:SentryUploadSources=true \
    -r linux-x64 -p:PublishSingleFile=true --self-contained false\
    Deevenue.Cli/ -o /app

RUN dotnet ef migrations has-pending-model-changes --project Deevenue.Infrastructure \
    && dotnet ef migrations bundle --project Deevenue.Infrastructure --output Deevenue.Migrations

FROM mcr.microsoft.com/dotnet/aspnet:9.0
SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN apt-get -y update && \
    apt-get install -y --no-install-recommends \
    ffmpeg=7:5.1.6-0+deb12u1 \
    curl=7.88.1-10+deb12u8 \
    # For wait-for:
    netcat-traditional=1.10-47 \
    && rm -rf /var/lib/apt/lists/* \
    && curl -O https://raw.githubusercontent.com/eficode/wait-for/v2.2.3/wait-for \
    && mv wait-for /bin && chmod +x /bin/wait-for \
    && curl -OL https://github.com/caddyserver/caddy/releases/download/v2.8.4/caddy_2.8.4_linux_amd64.deb \
    && dpkg -i caddy_2.8.4_linux_amd64.deb \
    && rm caddy_2.8.4_linux_amd64.deb

RUN addgroup --gid "10000" "deevenue" \
    && adduser \
    --disabled-password \
    --gecos "" \
    --home "/home/deevenue" \
    --ingroup "deevenue" \
    --uid "10001" \
    deevenue && \
    mkdir -p /app

USER deevenue:deevenue

RUN curl -LsSf https://astral.sh/uv/install.sh | sh
ENV PATH="$PATH:/home/deevenue/.local/bin"
RUN uv python install 3.12 \
    && uv tool install "scenedetect[opencv-headless]==0.6.5"

WORKDIR /app
COPY --link --from=build /app .
COPY --link --from=build /src/prod.docker-entrypoint.sh ./docker-entrypoint.sh
COPY --link --from=build /src/Deevenue.Migrations .

EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit

ENTRYPOINT ["/bin/bash", "/app/docker-entrypoint.sh"]
