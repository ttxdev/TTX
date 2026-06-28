import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import { getRedir, removeRedir, setSession } from "@/lib/auth/sessions.ts";

export const handler = define.handlers({
  async GET(ctx) {
    const code = ctx.url.searchParams.get("code");
    // `state` is optional: the login URL doesn't send one (no CSRF check yet),
    // so Twitch only echoes back `code`.
    const state = ctx.url.searchParams.get("state") ?? undefined;
    if (!code) {
      return new Response("Missing code", { status: 400 });
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
