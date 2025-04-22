# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ENV TTX_CONNECTION_STRING ""
ENV TTX_SECRET_KEY ""
ENV TTX_TWITCH_CLIENT_ID ""
ENV TTX_TWITCH_CLIENT_SECRET ""
ENV TTX_TWITCH_REDIRECT_URI ""
ENV TTX_DISCORD_CLIENT_ID ""
ENV TTX_DISCORD_CLIENT_SECRET ""
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/TTX.Api/TTX.Api.csproj", "src/TTX.Api/"]
RUN dotnet restore "./src/TTX.Api/TTX.Api.csproj"
COPY . .
WORKDIR "/src/src/TTX.Api"
RUN dotnet build "./TTX.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TTX.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ENV TTX_CONNECTION_STRING ""
ENV TTX_SECRET_KEY ""
ENV TTX_TWITCH_CLIENT_ID ""
ENV TTX_TWITCH_CLIENT_SECRET ""
ENV TTX_TWITCH_REDIRECT_URI ""
ENV TTX_DISCORD_CLIENT_ID ""
ENV TTX_DISCORD_CLIENT_SECRET ""
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TTX.Api.dll"]