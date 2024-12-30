FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0
SHELL ["/bin/bash", "-o", "pipefail", "-c"]
WORKDIR /src
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path /src/reportgenerator

RUN apt-get -y update && \
    apt-get install -y --no-install-recommends \
    ffmpeg=7:5.1.6-0+deb12u1 \
    && rm -rf /var/lib/apt/lists/*

RUN curl -LsSf https://astral.sh/uv/install.sh | sh
ENV PATH="$PATH:/root/.local/bin"
RUN uv python install 3.12 \
    && uv tool install "scenedetect[opencv-headless]==0.6.5"

# Copy project and solution files separately to restore dependencies
COPY --link Deevenue.Api/Deevenue.Api.csproj ./Deevenue.Api/
COPY --link Deevenue.Infrastructure/Deevenue.Infrastructure.csproj ./Deevenue.Infrastructure/
COPY --link Deevenue.Domain/Deevenue.Domain.csproj ./Deevenue.Domain/

COPY --link Deevenue.Api.Tests/Deevenue.Api.Tests.csproj ./Deevenue.Api.Tests/
COPY --link Deevenue.Domain.Tests/Deevenue.Domain.Tests.csproj ./Deevenue.Domain.Tests/
COPY --link Deevenue.Infrastructure.Tests/Deevenue.Infrastructure.Tests.csproj ./Deevenue.Infrastructure.Tests/

RUN find . -name "*.Tests.csproj" -exec dotnet restore {} \;

# Copy rest of the source code
COPY --link . .

RUN find . -name "*.Tests.csproj" -exec dotnet build --no-restore {} \;

ENTRYPOINT [ "sh", "./tests.sh" ]
