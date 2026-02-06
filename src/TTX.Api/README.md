# TTX.API

## Development Setup

### Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/)
- [Docker](https://www.docker.com/)

### Configure

Make sure postgres and redis are running. There is a compose file at the root 
of this monorepo.

Then we'll need Twitch OAuth credentials and we can set them using user secrets.

```sh
docker compose up -d postgres redis
cd ./src/TTX.Api
# Get Twitch OAuth creds https://dev.twitch.tv/
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:ClientId" "<CLIENT ID>"
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:ClientSecret" "<SECRET>"
# Optional
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:RedirectUri" "<REDIRECT URL>"
```

### Start

```sh
dotnet run
```

### Seeding

```sh
# do this once
dotnet run seed
```
