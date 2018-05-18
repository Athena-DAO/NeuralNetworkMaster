FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY NeuralNetworkMaster/NeuralNetworkMaster.csproj NeuralNetworkMaster/
RUN dotnet restore NeuralNetworkMaster/NeuralNetworkMaster.csproj
COPY . .
WORKDIR /src/NeuralNetworkMaster
RUN dotnet build NeuralNetworkMaster.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish NeuralNetworkMaster.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NeuralNetworkMaster.dll"]
