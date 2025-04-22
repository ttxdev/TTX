# ValueMonitor (Bot)

Open-source version of TTX Value Monitor.

# Setup

- [Download dotnet](https://dotnet.microsoft.com/en-us/)

To access our dependencies we need to [Generate a GitHub PAT](https://github.com/settings/tokens/new), with the read:packages scope.

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

For this we can utilize Docker, but you will at least need an instance of Redis.

```sh
docker compose up -d postgres 
```

##  Run!

```sh
dotnet run
```
