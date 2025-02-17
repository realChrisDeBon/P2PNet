# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Install clang/zlib1g-dev dependencies for publishing to native
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev \
    curl

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y nodejs

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.config", "."]

# Copy the project files and restore dependencies
COPY ["P2PBootstrap/P2PBootstrap.csproj", "P2PBootstrap/"]
RUN dotnet restore "./P2PBootstrap/P2PBootstrap.csproj"

# Copy the entire project and install JavaScript dependencies
COPY . .
WORKDIR "/src/P2PBootstrap"
RUN npm install --prefix wwwroot
RUN node wwwroot/install-dependencies.js

# Build the project
RUN dotnet build "./P2PBootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./P2PBootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# This stage is used as the base for the final stage when launching from VS to support debugging in regular mode (Default when not using the Debug configuration)
FROM base AS aotdebug
USER root
# Install GDB to support native debugging
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb
USER app

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:9.0} AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./P2PBootstrap"]
