FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN dotnet tool install dotnet-ef --tool-path /src/dotnet-ef
ENV PATH="$PATH:/src/dotnet-ef"

# Copy project files separately to restore dependencies
COPY --link Deevenue.Api/Deevenue.Api.csproj ./Deevenue.Api/
COPY --link Deevenue.Cli/Deevenue.Cli.csproj ./Deevenue.Cli/
COPY --link Deevenue.Infrastructure/Deevenue.Infrastructure.csproj ./Deevenue.Infrastructure/
COPY --link Deevenue.Domain/Deevenue.Domain.csproj ./Deevenue.Domain/
RUN dotnet restore Deevenue.Api/Deevenue.Api.csproj \
    && dotnet restore Deevenue.Cli/Deevenue.Cli.csproj

# Copy source code and publish app
COPY --link . .

# Intentionally not publishing Deevenue.Cli as single-file here because that breaks debuggability.
RUN dotnet publish --no-restore --configuration Debug Deevenue.Api/ -o /app \
    && dotnet publish --no-restore --configuration Debug Deevenue.Cli/ -o /cli

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
    && dpkg -i caddy_2.8.4_linux_amd64.deb

RUN curl -LsSf https://astral.sh/uv/install.sh | sh
ENV PATH="$PATH:/root/.local/bin"
RUN uv python install 3.12 \
    && uv tool install "scenedetect[opencv-headless]==0.6.5"

WORKDIR /app
COPY --link --from=build /app .
COPY --link --from=build /src/dev.docker-entrypoint.sh ./docker-entrypoint.sh
COPY --link --from=build /src/Deevenue.Migrations .
COPY --link --from=build /cli /cli

EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit

ENTRYPOINT ["/bin/bash", "/app/docker-entrypoint.sh"]
