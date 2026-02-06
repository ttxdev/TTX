import { Head } from "fresh/runtime";
import { OrderDirection, PlayerDto, PlayerOrderBy } from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import { nav } from "@/lib/url.ts";
import { calculatePercentChange } from "@/lib/math.ts";
import { formatValue } from "@/lib/formatting.ts";
import MiniChart from "./(_islands)/MiniChart.tsx";

const TAKE = 20;

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token);
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
        players: players.data.map<PlayerDto>((p) => p.toJSON()),
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
  const totalPages = Math.max(1, Math.ceil(ctx.data.total / 20));
  const pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <main>
      <Head>
        <title>TTX - Leaderboard</title>
        <meta
          name="description"
          content="Check out the top players ranked by their portfolio value on the leaderboard."
        />
      </Head>

      <div class="mx-auto flex w-full max-w-250 flex-col space-y-12 p-4 max-md:my-2 max-md:space-y-6">
        <div class="flex items-center justify-between gap-4 max-md:flex-col max-md:gap-2">
          <p class="font-display self-start text-center text-5xl max-md:text-3xl">
            Players
          </p>
          <div class="flex flex-col gap-2 max-md:flex-col max-md:items-start max-md:justify-start">
            <search>
              <form method="get" class="join max-w-md">
                <label class="input-bordered input flex-1 rounded-l-2xl border-purple-400 focus:outline-none">
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
                    class=" focus:outline-none"
                    placeholder="Search by Player Name"
                    name="search"
                  />
                </label>
                <button
                  type="submit"
                  class="btn rounded-r-2xl border-purple-400 bg-purple-400 text-white"
                >
                  Search
                </button>
              </form>
            </search>
            <span class="w-full text-right text-xs opacity-50">
              {ctx.data.total}{"  "}
              {ctx.data.total === 1 ? "Player" : "Players"}
            </span>
          </div>
        </div>

        <PlayerTable
          players={ctx.data.players}
          pageIndex={ctx.data.index}
        />
      </div>

      {totalPages > 1 && (
        <div class="mt-8 flex flex-col items-center gap-4 max-md:gap-3">
          <div class="text-sm text-gray-500 max-md:text-center max-md:text-base dark:text-gray-400">
            Showing {(ctx.data.index - 1) * 20 + 1} to{" "}
            {Math.min(ctx.data.index * 20, ctx.data.total)} of {ctx.data.total}
            {" "}
            players
          </div>
          <div class="join max-md:scale-100">
            <a
              class="join-item btn rounded-l-2xl border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700"
              href={ctx.data.index === 1
                ? undefined
                : nav(ctx.url, { page: ctx.data.index - 1 })}
              aria-label="Previous page"
            >
              «
            </a>
            {pageNumbers.map((i) => {
              <a
                class={`join-item btn border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700 ${
                  ctx.data.index ===
                      i
                    ? "border-purple-400 bg-purple-400/80 text-white hover:bg-[#8f44fb]"
                    : "hover:bg-gray-100 dark:hover:bg-gray-800"
                }`}
                href={nav(ctx.url, { page: i })}
              >
                {i}
              </a>;
            })}
            <a
              class="join-item btn rounded-r-2xl border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700"
              href={ctx.data.index === totalPages
                ? undefined
                : nav(ctx.url, { page: ctx.data.index + 1 })}
            >
              »
            </a>
          </div>
        </div>
      )}
    </main>
  );
});

function PlayerTable(
  { players, pageIndex }: {
    players: PlayerDto[];
    pageIndex: number;
  },
) {
  return (
    <table class="table w-full max-md:text-base">
      <thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
        <tr class="border-b text-sm max-md:text-base">
          <th class="rounded-tl-2xl py-6 text-center max-md:px-4">Player</th>
          <th class="py-6 text-center max-md:px-4">Portfolio</th>
          <th class="py-6 text-center max-md:hidden max-md:px-4">Chart</th>
          <th class="rounded-tr-2xl py-6 text-center max-md:hidden max-md:px-4">
            Action
          </th>
        </tr>
      </thead>
      <tbody>
        {players.map((player, i) => {
          const href = `/players/${player.slug}`;
          const change = calculatePercentChange(player.history);
          const placement = (pageIndex - 1) * 20 + i;
          let placementLabel = "";
          if (placement > 2) {
            const place = (pageIndex - 1) * 20 + i + 1;
            const suffix = place % 10 === 1 && place !== 11
              ? "st"
              : place % 10 === 2 && place !== 12
              ? "nd"
              : place % 10 === 3 && place !== 13
              ? "rd"
              : "th";
            placementLabel = `${place}${suffix}`;
          }

          return (
            <tr class="group border-b border-gray-100 transition-colors hover:bg-gray-50/50 dark:border-gray-800 dark:hover:bg-gray-800/50">
              <td class="py-6 text-center max-md:px-4">
                <div class="items-left justify-left flex flex-col gap-3">
                  <div class="flex flex-row items-center gap-4">
                    <div class="flex flex-col items-center">
                      <div class="relative">
                        <a href={href}>
                          <img
                            src={player.avatar_url}
                            alt={player.name}
                            class="size-12 rounded-full object-cover ring-2 ring-gray-200 transition-all group-hover:ring-purple-400 max-md:size-14 dark:ring-gray-700"
                          />
                        </a>
                        {placement === 0 && (
                          <span class="absolute -top-1 -right-1 rounded-full bg-yellow-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-yellow-500">
                            1st
                          </span>
                        )}
                        {placement === 1 && (
                          <span class="absolute -top-1 -right-1 rounded-full bg-gray-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-gray-500">
                            2nd
                          </span>
                        )}
                        {placement === 2 && (
                          <span class="absolute -top-1 -right-1 rounded-full bg-orange-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-orange-500">
                            3rd
                          </span>
                        )}
                        {placement === 3 && (
                          <span class="absolute -top-1 -right-1 rounded-full bg-gray-200 px-2 py-0.5 text-xs font-bold text-gray-700 dark:bg-gray-700 dark:text-gray-300">
                            4th
                          </span>
                        )}
                        {placement > 3 && (
                          <span class="absolute -top-1 -right-1 rounded-full bg-gray-200 px-2 py-0.5 text-xs font-bold text-gray-700 dark:bg-gray-700 dark:text-gray-300">
                            {placementLabel}
                          </span>
                        )}
                      </div>
                    </div>
                    <a href={href} class="flex min-w-0 flex-col">
                      <span class="max-md:text-base max-md:font-semibold">
                        {player.name}
                      </span>
                      <span
                        class={`${
                          change > 0
                            ? "text-green-500"
                            : change < 0
                            ? "text-red-500"
                            : "text-gray-500"
                        } text-sm font-semibold text-nowrap`}
                      >
                        {change > 0 && ("↗")}
                        {change < 0 && ("↘")}
                        {change.toFixed(2)}%
                      </span>
                    </a>
                  </div>
                </div>
              </td>
              <td class="py-6 text-center max-md:px-4 max-md:text-base max-md:font-semibold">
                <a href={href}>
                  {formatValue(player.portfolio)}
                </a>
              </td>
              <td class="flex items-center justify-center py-6 text-center max-md:hidden max-md:px-4">
                <a href={href} class="block h-16 w-32">
                  <MiniChart value={player.value} history={player.history} />
                </a>
              </td>
              <td class="py-6 text-center max-md:hidden max-md:px-4">
                <a
                  href={href}
                  class="btn btn-ghost rounded-lg text-purple-400 hover:bg-purple-400/10"
                >
                  View
                </a>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
