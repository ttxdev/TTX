import { Head } from "fresh/runtime";
import { OrderDirection, PlayerDto, PlayerOrderBy } from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import { nav } from "@/lib/url.ts";
import { calculatePercentChange } from "@/lib/math.ts";
import { formatValue } from "@/lib/formatting.ts";
import MiniChart from "@/islands/MiniChart.tsx";

const TAKE = 20;

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token, ctx.state.auth);
    const page = Number(ctx.url.searchParams.get("page")) || 1;
    const search = ctx.url.searchParams.get("search") || "";
    const orderDir =
      ctx.url.searchParams.get("orderDir") === OrderDirection.Ascending
        ? OrderDirection.Ascending
        : OrderDirection.Descending;
    let orderBy: PlayerOrderBy;
    switch (ctx.url.searchParams.get("orderBy") || PlayerOrderBy.Portfolio) {
      case PlayerOrderBy.Name:
        orderBy = PlayerOrderBy.Name;
        break;
      case PlayerOrderBy.Credits:
        orderBy = PlayerOrderBy.Credits;
        break;
      case PlayerOrderBy.Portfolio:
      default:
        orderBy = PlayerOrderBy.Portfolio;
        break;
    }

    const players = await client.getPlayers(
      page,
      TAKE,
      search,
      orderBy,
      orderDir,
    );

    return {
      data: {
        players: players.data,
        total: players.total,
        index: page,
        orderBy,
        orderDir,
        search,
      },
    };
  },
});

export default define.page<typeof handler>((ctx) => {
  const { players, total, index, orderBy, orderDir, search } = ctx.data;
  const totalPages = Math.max(1, Math.ceil(total / TAKE));
  const pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);
  const ranked = orderBy === PlayerOrderBy.Portfolio &&
    orderDir === OrderDirection.Descending;

  return (
    <main>
      <Head>
        <title>TTX - Leaderboard</title>
        <meta
          name="description"
          content="Check out the top players ranked by their portfolio value on the leaderboard."
        />
      </Head>

      <div class="mx-auto flex w-full max-w-250 flex-col space-y-10 p-4 max-md:my-2 max-md:space-y-6">
        <div class="flex items-end justify-between gap-4 max-md:flex-col max-md:items-stretch max-md:gap-3">
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
              Rankings
            </span>
            <h1 class="font-display text-5xl max-md:text-3xl">Leaderboard</h1>
            <p class="text-sm opacity-60">
              Players ranked by total portfolio value.
            </p>
          </div>
          <div class="flex flex-col gap-1.5 max-md:w-full">
            <search>
              <form method="get" class="join w-full max-w-md">
                {/* Preserve the active sort when running a search. */}
                <input type="hidden" name="orderBy" value={orderBy} />
                <input type="hidden" name="orderDir" value={orderDir} />
                <label class="input join-item flex-1 rounded-l-2xl border-purple-500/40 focus-within:border-purple-500 focus:outline-none">
                  <svg
                    class="h-[1em] opacity-50"
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 24 24"
                  >
                    <g
                      stroke-linejoin="round"
                      stroke-linecap="round"
                      stroke-width="2.5"
                      fill="none"
                      stroke="currentColor"
                    >
                      <circle cx="11" cy="11" r="8"></circle>
                      <path d="m21 21-4.3-4.3"></path>
                    </g>
                  </svg>
                  <input
                    type="text"
                    class="focus:outline-none"
                    placeholder="Search by Player Name"
                    name="search"
                    value={search}
                  />
                </label>
                <button
                  type="submit"
                  class="btn join-item rounded-r-2xl border-purple-600 bg-purple-600 text-white hover:bg-purple-700"
                >
                  Search
                </button>
              </form>
            </search>
            <span class="text-right text-xs opacity-50">
              {total} {total === 1 ? "Player" : "Players"}
            </span>
          </div>
        </div>

        {players.length > 0
          ? (
            <PlayerTable
              url={ctx.url}
              players={players}
              pageIndex={index}
              orderBy={orderBy}
              orderDir={orderDir}
              ranked={ranked}
            />
          )
          : (
            <div class="border-base-content/10 rounded-2xl border border-dashed py-16 text-center">
              <p class="font-display text-xl">No players found</p>
              <p class="mt-1 text-sm opacity-60">
                {search
                  ? `Nobody matches “${search}”.`
                  : "The leaderboard is empty right now."}
              </p>
              {search && (
                <a
                  href={nav(ctx.url, { search: "", page: 1 })}
                  class="mt-4 inline-block font-semibold text-purple-500 hover:underline"
                >
                  Clear search
                </a>
              )}
            </div>
          )}

        {totalPages > 1 && (
          <div class="flex flex-col items-center gap-4">
            <div class="text-sm opacity-50">
              Showing {(index - 1) * TAKE + 1} to{" "}
              {Math.min(index * TAKE, total)} of {total} players
            </div>
            <div class="join max-md:flex-wrap max-md:justify-center">
              <PageLink
                url={ctx.url}
                page={index - 1}
                disabled={index <= 1}
                label="«"
                extra="rounded-l-2xl"
              />
              {pageNumbers.map((i) => (
                <a
                  class={`join-item btn border-base-content/10 ${
                    index === i
                      ? "border-purple-600 bg-purple-600 text-white hover:bg-purple-700"
                      : "bg-base-200/40 hover:bg-base-200/70"
                  }`}
                  href={nav(ctx.url, { page: i })}
                  aria-current={index === i ? "page" : undefined}
                >
                  {i}
                </a>
              ))}
              <PageLink
                url={ctx.url}
                page={index + 1}
                disabled={index >= totalPages}
                label="»"
                extra="rounded-r-2xl"
              />
            </div>
          </div>
        )}
      </div>
    </main>
  );
});

function PageLink(
  { url, page, disabled, label, extra }: {
    url: URL;
    page: number;
    disabled: boolean;
    label: string;
    extra: string;
  },
) {
  return (
    <a
      class={`join-item btn border-base-content/10 bg-base-200/40 hover:bg-base-200/70 ${extra} ${
        disabled ? "btn-disabled" : ""
      }`}
      href={disabled ? undefined : nav(url, { page })}
      aria-disabled={disabled ? "true" : undefined}
      aria-label={label === "«" ? "Previous page" : "Next page"}
    >
      {label}
    </a>
  );
}

function SortHeader(
  { url, label, col, orderBy, orderDir, class: cls }: {
    url: URL;
    label: string;
    col: PlayerOrderBy;
    orderBy: PlayerOrderBy;
    orderDir: OrderDirection;
    class?: string;
  },
) {
  const active = orderBy === col;
  const dir = active && orderDir === OrderDirection.Descending
    ? OrderDirection.Ascending
    : OrderDirection.Descending;

  return (
    <th class={cls}>
      <a
        href={nav(url, { page: 1, orderBy: col, orderDir: dir })}
        class={`inline-flex items-center gap-1 transition-colors hover:text-purple-500 ${
          active ? "text-purple-500" : ""
        }`}
      >
        {label}
        <span class={active ? "" : "opacity-0 transition-opacity"}>
          {active && orderDir === OrderDirection.Ascending ? "↑" : "↓"}
        </span>
      </a>
    </th>
  );
}

const MEDALS: Record<number, string> = {
  1: "bg-[#FFD700]/15 text-[#FFD700] ring-1 ring-[#FFD700]/40",
  2: "bg-[#C0C0C0]/15 text-[#C0C0C0] ring-1 ring-[#C0C0C0]/40",
  3: "bg-[#CD7F32]/20 text-[#CD7F32] ring-1 ring-[#CD7F32]/40",
};

function Rank({ place, medal }: { place: number; medal: boolean }) {
  if (medal && place <= 3) {
    return (
      <span
        class={`font-display mx-auto flex size-9 items-center justify-center rounded-full text-sm ${
          MEDALS[place]
        }`}
      >
        {place}
      </span>
    );
  }
  return <span class="font-mono text-sm opacity-40">{place}</span>;
}

function Change({ change }: { change: number }) {
  const up = change > 0;
  const down = change < 0;
  return (
    <span
      class={`flex items-center gap-0.5 text-xs font-semibold ${
        up ? "text-green-500" : down ? "text-red-500" : "opacity-50"
      }`}
    >
      {up && "▲"}
      {down && "▼"}
      {change.toFixed(2)}%
    </span>
  );
}

function PlayerTable(
  { url, players, pageIndex, orderBy, orderDir, ranked }: {
    url: URL;
    players: PlayerDto[];
    pageIndex: number;
    orderBy: PlayerOrderBy;
    orderDir: OrderDirection;
    ranked: boolean;
  },
) {
  return (
    <div class="border-base-content/10 overflow-hidden rounded-2xl border">
      <table class="table w-full">
        <thead class="bg-base-200/50 text-sm">
          <tr class="border-base-content/10 border-b">
            <th class="w-14 py-4 text-center font-medium opacity-50">#</th>
            <SortHeader
              url={url}
              label="Player"
              col={PlayerOrderBy.Name}
              orderBy={orderBy}
              orderDir={orderDir}
              class="py-4 text-left"
            />
            <SortHeader
              url={url}
              label="Portfolio"
              col={PlayerOrderBy.Portfolio}
              orderBy={orderBy}
              orderDir={orderDir}
              class="py-4 pr-4 text-right"
            />
            <th class="py-4 text-center font-medium opacity-50 max-md:hidden">
              Chart
            </th>
            <th class="w-24 py-4 max-md:hidden"></th>
          </tr>
        </thead>
        <tbody>
          {players.map((player, i) => {
            const href = `/players/${player.slug}`;
            const change = calculatePercentChange(player.history);
            const place = (pageIndex - 1) * TAKE + i + 1;

            return (
              <tr class="group border-base-content/5 hover:bg-base-200/40 border-b transition-colors last:border-0">
                <td class="py-3 text-center">
                  <Rank place={place} medal={ranked} />
                </td>
                <td class="py-3">
                  <div class="flex items-center gap-3 max-md:px-1">
                    <a href={href} class="shrink-0">
                      <img
                        src={player.avatar_url}
                        alt={player.name}
                        class="ring-base-content/10 size-11 rounded-full object-cover ring-2 transition-all group-hover:ring-purple-500/50"
                      />
                    </a>
                    <a href={href} class="flex min-w-0 flex-col">
                      <span class="truncate font-semibold transition-colors group-hover:text-purple-500">
                        {player.name}
                      </span>
                      <Change change={change} />
                    </a>
                  </div>
                </td>
                <td class="py-3 pr-4 text-right">
                  <a href={href} class="font-display text-lg max-md:text-base">
                    {formatValue(player.portfolio)}
                  </a>
                </td>
                <td class="py-3 max-md:hidden">
                  <a
                    href={href}
                    class="mx-auto block h-12 w-32"
                    aria-label="View chart"
                  >
                    <MiniChart value={player.value} history={player.history} />
                  </a>
                </td>
                <td class="py-3 pr-4 text-right max-md:hidden">
                  <a
                    href={href}
                    class="inline-flex items-center gap-1 text-sm font-semibold text-purple-500 opacity-60 transition-all group-hover:translate-x-0.5 group-hover:opacity-100"
                  >
                    View
                    <svg
                      class="size-4"
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="2"
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      aria-hidden="true"
                    >
                      <path d="M5 12h14" />
                      <path d="m12 5 7 7-7 7" />
                    </svg>
                  </a>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
