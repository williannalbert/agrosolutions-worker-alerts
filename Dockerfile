FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AgroSolutions.Alerts.Worker/AgroSolutions.Alerts.Worker.csproj", "AgroSolutions.Alerts.Worker/"]
COPY ["AgroSolutions.Alerts.Infrastructure/AgroSolutions.Alerts.Infrastructure.csproj", "AgroSolutions.Alerts.Infrastructure/"]
COPY ["AgroSolutions.Alerts.Application/AgroSolutions.Alerts.Application.csproj", "AgroSolutions.Alerts.Application/"]
COPY ["AgroSolutions.Alerts.Domain/AgroSolutions.Alerts.Domain.csproj", "AgroSolutions.Alerts.Domain/"]

RUN dotnet restore "./AgroSolutions.Alerts.Worker/AgroSolutions.Alerts.Worker.csproj"

COPY . .

WORKDIR "/src/AgroSolutions.Alerts.Worker"
RUN dotnet build "./AgroSolutions.Alerts.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgroSolutions.Alerts.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroSolutions.Alerts.Worker.dll"]