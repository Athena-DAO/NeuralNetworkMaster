FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY NeuralNetworkMaster/NeuralNetworkMaster.csproj NeuralNetworkMaster
RUN dotnet restore
COPY . .
WORKDIR /src/NeuralNetworkMaster
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY NeuralNetworkMaster/appsettings.json /app

ENTRYPOINT ["dotnet", "NeuralNetworkMaster.dll"]
