FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project file separately to restore dependencies
COPY --link Deevenue.Api/Deevenue.Api.csproj ./Deevenue.Api/
COPY --link Deevenue.Infrastructure/Deevenue.Infrastructure.csproj ./Deevenue.Infrastructure/
COPY --link Deevenue.Domain/Deevenue.Domain.csproj ./Deevenue.Domain/
RUN dotnet restore Deevenue.Api/Deevenue.Api.csproj

# Copy source code and publish app
COPY --link . .

RUN mkdir -p /app && \
    dotnet publish --no-restore --configuration Release Deevenue.Api/ -o /app

ENTRYPOINT [ "cp", "Deevenue.Api/Deevenue.Api.json" ]
CMD [ "/output" ]
