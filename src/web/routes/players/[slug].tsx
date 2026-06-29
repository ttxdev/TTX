import { Head } from "fresh/runtime";
import { PlayerDto, PlayerShareDto, PlayerTransactionDto } from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import { formatShareAmount, formatValue } from "@/lib/formatting.ts";
import PlayerCard from "./(_islands)/PlayerCard.tsx";
import Shares from "./(_islands)/Shares.tsx";
import LatestTransactions from "./(_components)/LatestTransactions.tsx";
import type { ComponentChildren } from "preact";

type PlayerView = {
  player: PlayerDto;
  shares: PlayerShareDto[];
  transactions: PlayerTransactionDto[];
  isStreamer: boolean;
};

export const handler = define.handlers({
  async GET(ctx): Promise<{ data: PlayerView | null }> {
    try {
      const client = getApiClient(ctx.state.token, ctx.state.auth);
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
    } catch (err) {
      console.error(err);
      return { data: null };
    }
  },
});

export default define.page<typeof handler>((ctx) => {
  if (!ctx.data) {
    return <PlayerNotFound />;
  }

  const { player, shares, transactions, isStreamer } = ctx.data;
  const description = `${player.name}'s portfolio on TTX — currently worth ${
    formatValue(player.portfolio)
  }. Track their holdings and latest trades.`;
  const totalShares = shares.reduce((sum, s) => sum + s.quantity, 0);

  return (
    <main>
      <Head>
        <title>TTX - {player.name}</title>
        <meta name="description" content={description} />
        <meta property="og:type" content="profile" />
        <meta property="og:title" content={`${player.name} · TTX`} />
        <meta property="og:description" content={description} />
        <meta property="og:image" content={player.avatar_url} />
        <meta name="twitter:card" content="summary" />
        <meta name="twitter:title" content={`${player.name} · TTX`} />
        <meta name="twitter:description" content={description} />
        <meta name="twitter:image" content={player.avatar_url} />
      </Head>

      <div class="mx-auto flex w-full max-w-250 flex-col gap-4 p-4">
        <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <h1 class="text-2xl font-bold">{player.name}</h1>
          {isStreamer && (
            <a
              href={`/creators/${player.slug}`}
              class="bg-primary hover:bg-primary/80 inline-flex items-center justify-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-white transition-colors sm:text-sm"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-3 w-3 sm:h-4 sm:w-4"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fill-rule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-8.707l-3-3a1 1 0 00-1.414 0l-3 3a1 1 0 001.414 1.414L9 9.414V13a1 1 0 102 0V9.414l1.293 1.293a1 1 0 001.414-1.414z"
                  clip-rule="evenodd"
                />
              </svg>
              <p>Switch to streamer profile</p>
            </a>
          )}
        </div>

        <PlayerCard state={ctx.state} player={player} />

        <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <StatCard
            label="Credits"
            value={formatValue(player.credits)}
            accent="border-green-500/30 bg-green-500/10"
            valueClass="text-green-500"
          />
          <StatCard
            label="Holdings"
            value={`${shares.length} ${
              shares.length === 1 ? "creator" : "creators"
            }`}
            accent="border-purple-500/30 bg-purple-500/10"
            valueClass="text-purple-500"
          />
          <StatCard
            label="Total Shares"
            value={formatShareAmount(totalShares)}
          />
        </div>

        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
          <Shares shares={shares} />
          <LatestTransactions transactions={transactions} />
        </div>
      </div>
    </main>
  );
});

function StatCard(
  { label, value, accent, valueClass }: {
    label: string;
    value: ComponentChildren;
    accent?: string;
    valueClass?: string;
  },
) {
  return (
    <div
      class={`flex flex-col gap-1 rounded-2xl border p-4 ${
        accent ?? "border-base-content/10 bg-base-200/40"
      }`}
    >
      <span class="text-[10px] font-medium tracking-widest uppercase opacity-60">
        {label}
      </span>
      <span class={`font-display text-2xl ${valueClass ?? ""}`}>{value}</span>
    </div>
  );
}

function PlayerNotFound() {
  return (
    <>
      <Head>
        <title>TTX - Player not found</title>
      </Head>
      <div class="mx-auto flex w-full max-w-250 flex-col items-center gap-4 p-4 py-24 text-center">
        <p class="font-display text-6xl max-md:text-4xl">404</p>
        <p class="font-display text-2xl max-md:text-xl">Player not found</p>
        <p class="max-w-md text-sm opacity-60">
          We couldn't find that player. They may not be on TTX, or the link may
          be incorrect.
        </p>
        <a
          href="/players"
          class="btn mt-2 rounded-lg border-none bg-purple-600 px-5 text-white shadow hover:bg-purple-700"
        >
          View the leaderboard
        </a>
      </div>
    </>
  );
}
