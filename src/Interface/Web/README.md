# Web

Primary TTX frontend https://ttx.gg

# Setup

- [Download NodeJS](https://nodejs.org/)

```sh
npm i
```

## Setup Environment Variables

Copy the `.env.example` into `.env` and fill out the required variables.

```sh
cp .env.example .env
# fill out env
```

## Setting up Accessories

By default, the environment is pointed at production which is perfectly fine. However, if you want to run
the API checkout [Api](https://github.com/ttxdev/TTX/tree/main/src/Interface/Api)

##  Run!

```sh
npm run dev
```

### Generating the API Client

The API is generated using [NSwag](https://github.com/RicoSuter/NSwag), [dotnet](https://dotnet.microsoft.com/en-us/) is required.

```sh
npm run genapi
```

- and then we must make a change to the `api.ts` to point to the global `fetch` function instead of window.

```diff
# this should be around line 17
-        this.http = http ? http : window as any;
+        this.http = http ? http : { fetch };
```
  
- If you want to point to your own API instance, open the `ttx.nswag` file and update the URL.
