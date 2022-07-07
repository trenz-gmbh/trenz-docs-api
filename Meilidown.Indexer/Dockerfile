FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.16 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Meilidown.Indexer/Meilidown.Indexer.csproj"
WORKDIR "/src/Meilidown.Indexer"
RUN dotnet build "Meilidown.Indexer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Meilidown.Indexer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Meilidown.Indexer.dll"]
