FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Meilidown.Indexer.csproj", "."]
RUN dotnet restore "Meilidown.Indexer.csproj"
COPY . .
RUN dotnet build "Meilidown.Indexer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Meilidown.Indexer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Meilidown.Indexer.dll"]
