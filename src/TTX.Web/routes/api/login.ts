import { define } from "../../utils.ts";
import { requestLogin } from "../../lib/auth/sessions.ts";

export const handler = define.handlers({
  GET(ctx) {
    const from = ctx.url.searchParams.get("from") ?? "/";
    return requestLogin(from);
  },
});
