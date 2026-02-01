import { Head } from "fresh/runtime";
import { PlayerDto, PlayerShareDto, PlayerTransactionDto } from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import PlayerCard from "./(_islands)/PlayerCard.tsx";
import Shares from "./(_islands)/Shares.tsx";
import LatestTransactions from "./(_components)/LatestTransactions.tsx";

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token);
    const player = await client.getPlayer(ctx.params.slug);
    const transactions = player.transactions
      .sort((b, a) => a.created_at.getTime() - b.created_at.getTime())
      .map<PlayerTransactionDto>((t) => t.toJSON());

    const isStreamer = await client.getCreator(player.slug)
      .then(() => true)
      .catch(() => false);

    return {
      data: {
        player: player.toJSON() satisfies PlayerDto,
        shares: player.shares.map<PlayerShareDto>((s) => s.toJSON()),
        transactions,
        isStreamer,
      },
    };
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <main>
      <Head>
        <title>TTX - {ctx.data.player.name}</title>
        <meta
          name="description"
          content={`Player profile for ${ctx.data.player.name}`}
        />
        <meta property="og:title" content={ctx.data.player.name} />
        <meta
          property="og:description"
          content={`Player profile for ${ctx.data.player.name}`}
        />
        <meta property="og:image" content={ctx.data.player.avatar_url} />
      </Head>

      <div class="relative mx-auto w-full max-w-[1000px]">
        <div class="mx-2 my-5">
          <PlayerCard
            state={ctx.state}
            player={ctx.data.player}
            placement={1}
            isStreamer={ctx.data.isStreamer}
          />
        </div>

        <div class="m-2 flex flex-col gap-4 md:flex-row">
          <div class="md:w-1/2">
            <Shares shares={ctx.data.shares} />
          </div>
          <div class="md:w-1/2">
            <LatestTransactions transactions={ctx.data.transactions} />
          </div>
        </div>
      </div>
    </main>
  );
});
