import { define } from "@/utils.ts";
import { Head } from "fresh/runtime";
import { getApiClient } from "../lib/index.ts";
import { LootBoxDto } from "../lib/api.ts";
import Gamba from "./(_islands)/Gamba.tsx";

export const handler = define.handlers({
  async GET(
    ctx,
  ): Promise<{ data: { boxes: LootBoxDto[]; loggedIn: boolean } }> {
    if (!ctx.state.token) {
      return { data: { boxes: [], loggedIn: false } };
    }

    try {
      const client = getApiClient(ctx.state.token, ctx.state.auth);
      const self = await client.getSelf();
      return { data: { boxes: self.loot_boxes, loggedIn: true } };
    } catch (err) {
      console.error(err);
      return { data: { boxes: [], loggedIn: false } };
    }
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <>
      <Head>
        <title>TTX - Gamba</title>
        <meta
          name="description"
          content="Open your lootboxes for a shot at rare creator shares on TTX Gamba."
        />
      </Head>
      {ctx.data.loggedIn
        ? <Gamba boxes={ctx.data.boxes} state={ctx.state} />
        : <GambaLoginPrompt url={ctx.url} />}
    </>
  );
});

function GambaLoginPrompt({ url }: { url: URL }) {
  return (
    <div class="mx-auto flex max-w-md flex-col items-center gap-4 p-8 py-24 text-center">
      <h1 class="bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-4xl font-black text-transparent md:text-6xl">
        TTX GAMBA
      </h1>
      <p class="opacity-70">
        Log in to open your lootboxes and win shares of your favorite creators.
      </p>
      <a
        href={`/api/login?from=${encodeURIComponent(url.pathname)}`}
        class="btn mt-2 rounded-lg border-none bg-gradient-to-r from-amber-500 to-yellow-500 font-bold text-black shadow transition-transform hover:scale-105"
      >
        Log in to play
      </a>
    </div>
  );
}
