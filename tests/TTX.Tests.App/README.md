Running tests

```sh
dotnet test
```

- By default the database we test against is SQLite which may result in some
  tests being skipped because the PortfolioService has a hard requirement on
  Timescale DB.

## Testing against Timescale

To test against Timescale, we'll set some user secrets while in the
`TTX.Tests.App` directory via the dotnet CLI.

```sh
dotnet user-secrets set "TTX:Infrastructure:Data" "Postgres"
# If you're using the container compose file the default appsettings postgres credentials should be sufficient. 
# Otherwise you can set the credentials as needed
dotnet user-secrets set "TTX:Infrastructure:ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=ttx;Username=postgres;Password=postgres"
```
