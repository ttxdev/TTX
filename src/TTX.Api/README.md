# API

## Development Setup

**Requirements**

- [Download dotnet](https://dotnet.microsoft.com/en-us/)
- [Docker](https://www.docker.com/)

```sh
docker compose up -d postgres
# Get Twitch OAuth creds https://dev.twitch.tv/
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:ClientId" "<CLIENT ID>"
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:ClientSecret" "<SECRET>"
# Optional
dotnet user-secrets set "TTX:Infrastructure:Twitch:OAuth:RedirectUri" "<REDIRECT URL>"
# Start!
dotnet run
```
