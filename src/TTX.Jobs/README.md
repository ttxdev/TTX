# TTX.Jobs

Separate executable for processing jobs.

## Development Setup

If you're working on the backend you only need to run the TTX.Api project, 
this project is only for deployment.

### Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/)
- [Docker](https://www.docker.com/)

### Configure

Make sure postgres and redis are running. There is a compose file at the root 
of this monorepo.

```sh
docker compose up -d postgres redis
```

### Start

```sh
dotnet run
```
