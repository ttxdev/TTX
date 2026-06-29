import { TTXClient } from "./api.ts";

export function getApiClient(
  token?: string,
  auth?: { unauthorized: boolean },
): TTXClient {
  return new TTXClient(Deno.env.get("FRESH_PUBLIC_API_BASE_URL")!, {
    async fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
      if (!init) {
        init = {};
      }
      init.headers ??= new Headers();
      if (token) {
        // @ts-expect-error NOTE(dylhack): Can't set headers on RequestInit
        init.headers["Authorization"] = "Bearer " + token;
      }

      const res = await fetch(url, init);
      if (token && auth && res.status === 401) {
        auth.unauthorized = true;
      }
      return res;
    },
  });
}
