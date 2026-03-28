# Stage 1: Base runtime and dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
# Install curl as root before switching users
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Backend/CommBackend/CommBackend/CommBackend.csproj", "Backend/CommBackend/CommBackend/"]
RUN dotnet restore "Backend/CommBackend/CommBackend/CommBackend.csproj"
COPY . .
WORKDIR "/src/Backend/CommBackend/CommBackend"
RUN dotnet build "CommBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CommBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Switch to the non-root user for security after setup is done
USER $APP_UID
ENTRYPOINT ["dotnet", "CommBackend.dll"]
# -- MAKE SURE THERE IS NOTHING BELOW THIS LINE --