# TTX.Tests.App

These tests are for the `TTX.App` package.

## Development Setup

### Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/)
- [Docker](https://www.docker.com/)

### Configuration

By default the database we test against is SQLite. If you need to test with
time series data, like creator votes or player portfolios, then you can setup
postgres:

```sh
dotnet user-secrets set "TTX:Infrastructure:Data" "Postgres"
# If you're using the container compose file the default appsettings postgres credentials should be sufficient. 
# Otherwise you can set the credentials as needed
dotnet user-secrets set "TTX:Infrastructure:ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=ttx;Username=postgres;Password=postgres"
```

### Test

```sh
dotnet test
```
