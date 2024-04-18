FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["projects/FailedMessageConsumer/FailedMessageConsumer.csproj", "projects/FailedMessageConsumer/"]
COPY ["packages/FailedMessageCache/FailedMessageCache.csproj", "packages/FailedMessageCache/"]
COPY ["packages/MessageClient/MessageClient.csproj", "packages/MessageClient/"]
COPY ["packages/SharedModels/SharedModels.csproj", "packages/SharedModels/"]
RUN dotnet restore "projects/FailedMessageConsumer/FailedMessageConsumer.csproj"
COPY . .
WORKDIR "/src/projects/FailedMessageConsumer"
RUN dotnet build "FailedMessageConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FailedMessageConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FailedMessageConsumer.dll"]
