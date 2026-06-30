# Contributing

TTX is a monorepo. The backend is a Rust [cargo workspace](./Cargo.toml); the
frontend is a separate [Deno](https://deno.com/) project. Local services
(Postgres/TimescaleDB and Redis) run via [`compose.yaml`](./compose.yaml), and
common tasks are wrapped in the [`Justfile`](./Justfile).

- [Swagger UI](https://api.ttx.gg/swagger-ui)
- [OpenAPI JSON](https://api.ttx.gg/api-docs/openapi.json)

## Layout

- [`src/ttx`](./src/ttx) — Core domain crate: creators, players, shares,
  transactions, events, jobs, caching, and telemetry. Owns the database
  migrations and benchmarks. The other crates depend on it.
- [`src/api`](./src/api) — Axum HTTP + WebSocket server. Serves the REST API and
  the Swagger docs (utoipa), and includes the `seed` command.
- [`src/jobs`](./src/jobs) — Standalone executable for background jobs.
- [`src/private`](./src/private) — Closed-source overrides for 
- [`src/web`](./src/web/README.md) — frontend web app

## Requirements

- [Rust](https://www.rust-lang.org/)
- [Deno](https://deno.com/)
- [Docker](https://www.docker.com/) (or Podman)
- [Just](https://github.com/casey/just)

## Getting Started

Start the local services (Postgres + Redis):

```sh
just up
```

Run the API (applies migrations, then serves on the configured port):

```sh
just api
```

Seed the database with sample data:

```sh
just seed
```

Run the background jobs executable:

```sh
just jobs
```

Run the frontend (see [`src/web/README.md`](./src/web/README.md) for `.env`
setup and dependency install first):

```sh
just web
```

When you're done, tear the services down with `just down`.

## Development Tasks

Run `just` to list every recipe. The most common:

| Recipe           | Description                                      |
| ---------------- | ------------------------------------------------ |
| `just test`      | Run tests for the whole workspace                |
| `just check`     | Type-check the whole workspace                   |
| `just clippy`    | Lint with clippy (warnings as errors)            |
| `just fmt`       | Format the whole workspace                       |
| `just fmt-check` | Verify formatting without writing changes        |
| `just bench`     | Run the `ttx` domain benchmarks                  |
| `just ci`        | Full local check suite: `fmt-check clippy test`  |
| `just swagger`   | Regenerate the OpenAPI client                    |

Before opening a pull request, please run `just ci` and make sure it passes.
