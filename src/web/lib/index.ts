import { TTXClient } from "./api.ts";

export function getApiClient(token?: string): TTXClient {
  return new TTXClient(Deno.env.get("FRESH_PUBLIC_API_BASE_URL")!, {
    fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
      if (!init) {
        init = {};
      }
      init.headers ??= new Headers();
      if (token) {
        // @ts-expect-error NOTE(dylhack): Can't set headers on RequestInit
        init.headers["Authorization"] = "Bearer " + token;
      }

      return fetch(url, init);
    },
  });
}
