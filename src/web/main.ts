import { App, staticFiles } from "fresh";
import type { State } from "./utils.ts";
import { getSession, removeSession } from "./lib/auth/sessions.ts";
import { trace } from "npm:@opentelemetry/api@1";

export const app = new App<State>();

app.use(staticFiles());

app.use(async (ctx) => {
  const span = trace.getActiveSpan();
  if (span) {
    span.setAttribute("http.route", ctx.url.pathname);
    span.updateName(`${ctx.req.method} ${ctx.url.pathname}`);
  }

  return await ctx.next();
});

app.use(async (ctx) => {
  const session = getSession(ctx.req.headers);
  const clientId = Deno.env.get("FRESH_PUBLIC_DISCORD_CLIENT_ID") ?? "";
  ctx.state.token = session?.token ?? "";
  ctx.state.user = session?.user;
  ctx.state.discordId = ctx.url.host == `${clientId}.discordsays.com`
    ? clientId
    : null;
  ctx.state.auth = { unauthorized: false };

  const res = await ctx.next();

  if (ctx.state.auth.unauthorized && ctx.state.token) {
    removeSession(res.headers);
  }

  return res;
});

app.get("/:slug", (ctx) => ctx.redirect(`/creators/${ctx.params.slug}`, 301));

// Include file-system based routes here
app.fsRoutes();
