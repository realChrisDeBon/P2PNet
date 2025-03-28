# Use the official image as a parent image
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

# Copy the NuGet configuration file from the root directory
COPY ["NuGet.config", "./"]

# Copy the project files from the root directory
COPY ["P2PBootstrap/P2PBootstrap.csproj", "P2PBootstrap/"]

# Restore dependencies
RUN dotnet restore "./P2PBootstrap/P2PBootstrap.csproj"

# Copy the entire project from the P2PBootstrap directory
COPY . .

WORKDIR "/src/P2PBootstrap"

# Install JavaScript dependencies
RUN npm install --prefix wwwroot
RUN node wwwroot/install-dependencies.js

# Build the project
RUN dotnet build "./P2PBootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./P2PBootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Final stage for production
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:9.0} AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./P2PBootstrap"]