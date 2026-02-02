FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for container health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Api/LifeGames.Api/LifeGames.Api.csproj", "Api/LifeGames.Api/"]
COPY ["Application/LifeGames.Application/LifeGames.Application.csproj", "Application/LifeGames.Application/"]
COPY ["Domain/LifeGames.Domain/LifeGames.Domain.csproj", "Domain/LifeGames.Domain/"]
COPY ["Infrastructure/LifeGames.Infrastructure/LifeGames.Infrastructure.csproj", "Infrastructure/LifeGames.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Api/LifeGames.Api/LifeGames.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/Api/LifeGames.Api"
RUN dotnet build "LifeGames.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LifeGames.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "LifeGames.Api.dll"]
