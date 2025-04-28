# API

A RESTful service that queries and handles commands into our core package, there are other TTX packages
like [StreamMonitor](https://github.com/ttxdev/TTX/tree/main/src/TTX.StreamMonitor) that are required to be running for
the actual TTX parody to function. See root README for more details.

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

## Setting up Accessories

For this we can utilize Docker, but you will at least need an instance of Postgres with
the [Timescale extension](https://www.timescale.com/).

```sh
docker compose up -d postgres
```

## Authentication Setup

1. Register a Twitch Application at https://dev.twitch.tv/console/apps/create

- Name: TTX
- OAuth Redirect URL: http://localhost:5116/sessions/twitch/callback
- Category: Application/Website Integration
- Client Type: Confidential

2. Fill in `TTX_TWITCH_CLIENT_ID` and `TTX_TWITCH_CLIENT_SECRET` in `.env` with the values from the registration

3. Access `http://localhost:5116/sessions/login` from a browser

Then after a successful OAuth flow it should return a JWT.

## Run!

```sh
dotnet run
```
