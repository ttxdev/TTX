# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /opt/app
COPY . .

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /opt/app
COPY --from=base /opt/app .
RUN dotnet build src/TTX.Api -c $BUILD_CONFIGURATION -o /opt/app/build/api
RUN dotnet build src/TTX.Jobs -c $BUILD_CONFIGURATION -o /opt/app/build/jobs

FROM build
WORKDIR /opt/app
COPY --from=build /opt/app/build/jobs .
CMD ["dotnet", "TTX.Jobs.dll"]
