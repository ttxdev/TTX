FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

COPY . .
RUN --mount=type=secret,id=github_token \
  export GITHUB_TOKEN=$(cat /run/secrets/github_token) && \
  dotnet nuget update source ttxdev --username ttxdev --password $GITHUB_TOKEN --store-password-in-clear-text && \
  dotnet restore .

RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

COPY --from=build /publish /app
CMD ["/app/TTX.Interface.StreamMonitor"]
