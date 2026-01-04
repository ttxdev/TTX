import { getApiClient } from "@/lib/index.ts";
import { define, State } from "@/utils.ts";
import { getRedir, removeRedir, setSession } from "@/lib/auth/sessions.ts";
import { Context } from "https://jsr.io/@fresh/core/2.2.0/src/context.ts";

export const handler = define.handlers({
  async GET(ctx: Context<State>) {
    const code = ctx.url.searchParams.get("code");
    const state = ctx.url.searchParams.get("state");
    if (!code || !state) {
      return new Response("Missing code or state", { status: 400 });
    }

    const client = getApiClient(ctx.state.token);
    const { access_token } = await client.twitchCallback(code, state);
    const headers = new Headers();
    setSession(headers, access_token);

    const redir = getRedir(ctx.req.headers);
    if (redir) {
      removeRedir(headers);
    }
    headers.set("Location", redir ?? "/");

    return new Response(undefined, {
      status: 307,
      headers: headers,
    });
  },
});
