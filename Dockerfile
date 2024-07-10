FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

ARG STARTUP_PROJECT=T-Badge.Api
ARG APPLICATION_CONTEXT=T-Badge.Infrastructure.Persistence.ApplicationContext

WORKDIR /src

COPY ["T-Badge.sln", "./"]
COPY ["T-Badge.Api/T-Badge.Api.csproj", "T-Badge.Api/"]
COPY ["T-Badge.Contracts/T-Badge.Contracts.csproj", "T-Badge.Contracts/"]

RUN dotnet restore "./T-Badge.sln"

COPY . .

WORKDIR "/src/T-Badge.Api"

RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "T-Badge.Api.dll"]