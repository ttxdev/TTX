# TTX Web

The TTX frontend, built with [Fresh](https://fresh.deno.dev/) (Preact) on
[Deno](https://deno.com/), bundled with Vite and styled with Tailwind/daisyUI.

## Development Setup

### Requirements

- [Deno](https://deno.com/)
- A running [TTX API](../api) (see the root
  [`CONTRIBUTING.md`](../../CONTRIBUTING.md))

### Configure

Copy the example env file and install dependencies. The `.env` controls which
backend the frontend talks to — `FRESH_PUBLIC_API_BASE_URL` should point at your
running API.

```sh
cp .env.example .env
deno i
```

### Start

Run the dev server (Vite + Fresh with hot reload):

```sh
deno run dev
```

## Tasks

Defined in [`deno.json`](./deno.json):

| Task                | Description                                          |
| ------------------- | --------------------------------------------------- |
| `deno run dev`      | Start the Vite dev server with hot reload           |
| `deno run build`    | Build the production bundle into `_fresh/`          |
| `deno run start`    | Serve the built bundle (`_fresh/server.js`)         |
| `deno run check`    | Format check, lint, and type-check                  |
| `deno run swagger`  | Regenerate the typed API client from the API's spec |
| `deno run update`   | Update Fresh to the latest release                  |

> `deno run swagger` reads the API's OpenAPI spec via [`ttx.nswag`](./ttx.nswag),
> so the API must be running first.
