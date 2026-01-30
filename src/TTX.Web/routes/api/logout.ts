import { removeSession } from "../../lib/auth/sessions.ts";
import { define } from "../../utils.ts";

export const handler = define.handlers({
  GET() {
    const headers = new Headers();
    removeSession(headers);
    headers.set("Location", "/");

    return new Response(undefined, {
      headers,
      status: 307,
    });
  },
});
