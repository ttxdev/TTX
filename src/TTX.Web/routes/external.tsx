import { define } from "../utils.ts";
import { DiscordNav } from "./(_islands)/DiscordNav.tsx";

export const handler = define.handlers({
  GET(ctx) {
    const destination = decodeURIComponent(
      ctx.url.searchParams.get("to") ?? "/",
    );

    if (!ctx.state.discordId) {
      return new Response(undefined, {
        status: 302,
        headers: {
          Location: destination,
        },
      });
    }

    return { data: { destination, clientId: ctx.state.discordId } };
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <DiscordNav
      clientId={ctx.data.clientId}
      destination={ctx.data.destination}
    />
  );
});
