# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["QuantFlow.Api.Rest/QuantFlow.Api.Rest.csproj", "QuantFlow.Api.Rest/"]
COPY ["QuantFlow.Common/QuantFlow.Common.csproj", "QuantFlow.Common/"]
COPY ["QuantFlow.Data.InfluxDB/QuantFlow.Data.InfluxDB.csproj", "QuantFlow.Data.InfluxDB/"]
COPY ["QuantFlow.Domain/QuantFlow.Domain.csproj", "QuantFlow.Domain/"]
COPY ["QuantFlow.Data.MongDB/QuantFlow.Data.MongoDB.csproj", "QuantFlow.Data.MongDB/"]
COPY ["QuantFlow.Data.SQLServer/QuantFlow.Data.SQLServer.csproj", "QuantFlow.Data.SQLServer/"]
RUN dotnet restore "./QuantFlow.Api.Rest/QuantFlow.Api.Rest.csproj"
COPY . .
WORKDIR "/src/QuantFlow.Api.Rest"
RUN dotnet build "./QuantFlow.Api.Rest.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QuantFlow.Api.Rest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuantFlow.Api.Rest.dll"]