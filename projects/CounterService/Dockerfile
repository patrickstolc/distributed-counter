FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["projects/CounterService/CounterService.csproj", "projects/CounterService/"]
COPY ["packages/SharedModels/SharedModels.csproj", "packages/SharedModels/"]
COPY ["packages/MessageClient/MessageClient.csproj", "packages/MessageClient/"]
COPY ["packages/Repository/Repository.csproj", "packages/Repository/"]
RUN dotnet restore "projects/CounterService/CounterService.csproj"
COPY . .
WORKDIR "/src/projects/CounterService"
RUN dotnet build "CounterService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CounterService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CounterService.dll"]
