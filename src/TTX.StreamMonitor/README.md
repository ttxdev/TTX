# StreamMonitor

StreaMonitor is responsible for listening to Twitch channels and keeping our Creator stream statuses up-to-date.

# Setup

- [Download dotnet](https://dotnet.microsoft.com/en-us/)

To access our dependencies we need to [Generate a GitHub PAT](https://github.com/settings/tokens/new), with the read:
packages scope.

> [!NOTE]
> You can skip this step if you've set a token for ttxdev already

```sh
dotnet nuget update source ttxdev --username GTHUB_USERNAME --password GITHUB_PAT
```

## Setup Environment Variables

Copy the `.env.example` into `.env` and fill out the required variables.

```sh
cp .env.example .env
# fill out env
```

## Run!

```sh
dotnet run
```
