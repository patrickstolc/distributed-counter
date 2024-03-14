FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/CounterAPI.E2E.Tests/CounterAPI.E2E.Tests.csproj", "tests/CounterAPI.E2E.Tests/"]
COPY ["packages/SharedModels/SharedModels.csproj", "packages/SharedModels/"]
RUN dotnet restore "tests/CounterAPI.E2E.Tests/CounterAPI.E2E.Tests.csproj"
COPY . .
WORKDIR "/src/tests/CounterAPI.E2E.Tests"
RUN dotnet build "CounterAPI.E2E.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CounterAPI.E2E.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CounterAPI.E2E.Tests.dll"]
