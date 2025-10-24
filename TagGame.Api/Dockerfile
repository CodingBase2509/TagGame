# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for restore layer caching
COPY TagGame.sln ./
COPY TagGame.Api/TagGame.Api.csproj TagGame.Api/
COPY TagGame.Api.Core/TagGame.Api.Core.csproj TagGame.Api.Core/
COPY TagGame.Shared/TagGame.Shared.csproj TagGame.Shared/

RUN dotnet restore TagGame.Api/TagGame.Api.csproj --nologo

# Copy the remaining sources and build
COPY . .
RUN dotnet build TagGame.Api/TagGame.Api.csproj -c Release -o /app/build --no-restore --nologo
RUN dotnet publish TagGame.Api/TagGame.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false --nologo

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Recommended: explicit port and URLs
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TagGame.Api.dll"]

