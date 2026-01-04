import { App, staticFiles } from "fresh";
import type { State } from "./utils.ts";
import { getSession } from "./lib/auth/sessions.ts";

export const app = new App<State>();

app.use(staticFiles());

app.use(async (ctx) => {
  const session = getSession(ctx.req.headers);
  const clientId = Deno.env.get("FRESH_PUBLIC_DISCORD_CLIENT_ID")!;
  ctx.state.token = session?.token ?? "";
  ctx.state.user = session?.user;
  ctx.state.discordId = ctx.url.host == `${clientId}.discordsays.com`
    ? clientId
    : null;

  return await ctx.next();
});

app.get("/:slug", (ctx) => ctx.redirect(`/creators/${ctx.params.slug}`, 301));

// Include file-system based routes here
app.fsRoutes();
