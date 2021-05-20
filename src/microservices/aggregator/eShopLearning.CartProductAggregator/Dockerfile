#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/microservices/aggregator/eShopLearning.CartProductAggregator/eShopLearning.CartProductAggregator.csproj", "src/microservices/aggregator/eShopLearning.CartProductAggregator/"]
COPY ["src/tool/eShopLearning.Common/eShopLearning.Common.csproj", "src/tool/eShopLearning.Common/"]
RUN dotnet restore "src/microservices/aggregator/eShopLearning.CartProductAggregator/eShopLearning.CartProductAggregator.csproj"
COPY . .
WORKDIR "/src/src/microservices/aggregator/eShopLearning.CartProductAggregator"
RUN dotnet build "eShopLearning.CartProductAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eShopLearning.CartProductAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShopLearning.CartProductAggregator.dll"]