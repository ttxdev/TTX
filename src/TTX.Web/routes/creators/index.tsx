import {
  CreatorDtoPaginationDto,
  CreatorOrderBy,
  OrderDirection,
} from "@/lib/api.ts";
import { define } from "@/utils.ts";
import { Head, Partial } from "fresh/runtime";
import MiniChart from "./(_islands)/MiniChart.tsx";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
import { getApiClient } from "@/lib/index.ts";
import { calculatePercentChange } from "@/lib/math.ts";

const TAKE = 20;

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token);
    const index = Number(ctx.url.searchParams.get("page") ?? "1");
    const orderDir =
      ctx.url.searchParams.get("orderDir") == OrderDirection.Ascending
        ? OrderDirection.Ascending
        : OrderDirection.Descending;
    const search = ctx.url.searchParams.get("search") ?? "";
    let orderBy: CreatorOrderBy;
    switch (ctx.url.searchParams.get("orderBy") ?? "IsLive") {
      case "Name":
        orderBy = CreatorOrderBy.Name;
        break;
      case "Value":
        orderBy = CreatorOrderBy.Value;
        break;
      case "IsLive":
      default:
        orderBy = CreatorOrderBy.IsLive;
        break;
    }

    const page = await client.getCreators(
      index,
      TAKE,
      search,
      orderBy,
      orderDir,
    );

    return { data: { page, orderBy, orderDir, index } };
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <div>
      <Head>
        <title>TTX - Creators</title>
        <meta
          name="description"
          content="View the vast array of creators that you can pump and dump... I mean.... INVEST your hard earned tokens in on TTX"
        />
      </Head>
      <div class="mx-auto flex w-full max-w-250 flex-col space-y-12 p-4 max-md:my-2 max-md:space-y-6">
        <div class="flex items-center justify-between gap-4 max-md:flex-col max-md:gap-2">
          <p class="font-display self-start text-center text-5xl max-md:text-3xl">
            Creators
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
                    placeholder="Search by Channel Name"
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
              {ctx.data.page.total}
              {ctx.data.page.total === 1 ? "Creator" : "Creators"}
            </span>
          </div>
        </div>

        <Partial name="creators">
          {ctx.data.page.data.length > 0 && (
            <CreatorTable
              url={ctx.url}
              creators={ctx.data.page}
              index={ctx.data.index}
              orderBy={ctx.data.orderBy}
              orderDir={ctx.data.orderDir}
            />
          )}
          {ctx.data.page.data.length === 0 && <p>No channels available</p>}
        </Partial>
      </div>
    </div>
  );
});

function CreatorTable(props: {
  url: URL;
  creators: CreatorDtoPaginationDto;
  index: number;
  orderBy: CreatorOrderBy;
  orderDir: OrderDirection;
}) {
  const oppositeDir = props.orderDir === OrderDirection.Ascending
    ? OrderDirection.Descending
    : OrderDirection.Ascending;
  const totalPages = Math.max(
    1,
    Math.floor((props.creators.total - 1) / TAKE) + 1,
  );
  const pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);
  const isAscending = props.orderDir == OrderDirection.Ascending;

  function nav(
    params: {
      index?: number;
      orderBy?: CreatorOrderBy;
      orderDir?: OrderDirection;
    },
  ) {
    const url = new URL(props.url.origin + "/creators");
    url.searchParams.set(
      "page",
      params.index?.toString() ?? props.index.toString(),
    );
    url.searchParams.set("orderBy", params.orderBy ?? props.orderBy);
    url.searchParams.set(
      "orderDir",
      params.orderDir ?? props.orderDir.toString(),
    );

    return url.toString();
  }

  return (
    <div>
      <div class="overflow-x-auto rounded-2xl border border-gray-200 shadow-sm dark:border-gray-800">
        <table class="table w-full max-md:text-sm">
          <thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
            <tr class="border-b text-sm max-md:text-sm">
              <th class="rounded-tl-2xl py-4 text-center max-md:px-2">
                <a
                  href={nav({
                    orderBy: CreatorOrderBy.Name,
                    orderDir: oppositeDir,
                  })}
                >
                  Creator
                  {props.orderBy === CreatorOrderBy.Name && (
                    <span class="ml-1">
                      {isAscending ? "↑" : "↓"}
                    </span>
                  )}
                </a>
              </th>
              <th class="py-4 text-center max-md:px-2">
                <a
                  href={nav({
                    orderBy: CreatorOrderBy.Value,
                    orderDir: oppositeDir,
                  })}
                >
                  Price
                  {props.orderBy === CreatorOrderBy.Value && (
                    <span class="ml-1">
                      {isAscending ? "↑" : "↓"}
                    </span>
                  )}
                </a>
              </th>
              <th class="py-4 text-center max-md:hidden">Chart</th>
              <th class="rounded-tr-2xl py-4 text-center max-md:px-2">
                <a
                  href={nav({
                    orderBy: CreatorOrderBy.IsLive,
                    orderDir: oppositeDir,
                  })}
                >
                  Status
                  {props.orderBy === CreatorOrderBy.IsLive && (
                    <span class="ml-1">
                      {isAscending ? "↑" : "↓"}
                    </span>
                  )}
                </a>
              </th>
            </tr>
          </thead>
          <tbody>
            {props.creators.data.map((creator) => {
              const creatorHref = `/creators/${creator.slug}`;
              const percentageChange = calculatePercentChange(creator.history);

              return (
                <tr class="group cursor-pointer border-b border-gray-100 transition-colors hover:bg-gray-50/50 dark:border-gray-800 dark:hover:bg-gray-800/50">
                  <td class="py-4 max-md:px-2">
                    <a
                      href={creatorHref}
                      rel="noopener noreferrer"
                      class="block"
                    >
                      <div class="flex items-center gap-2">
                        <div class="relative shrink-0">
                          <img
                            src={creator.avatar_url}
                            alt={creator.name}
                            class="size-12 rounded-full object-cover ring-2 ring-gray-200 transition-all group-hover:ring-purple-400 max-md:size-8 dark:ring-gray-700"
                          />
                          {creator.stream_status.is_live && (
                            <span class="absolute -top-1 -right-1 rounded-full bg-red-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-red-500">
                              LIVE
                            </span>
                          )}
                        </div>
                        <div class="flex min-w-0 flex-col">
                          <div class="truncate font-semibold max-md:text-sm">
                            {creator.name}
                          </div>
                          <div class="flex items-center gap-1">
                            <span class="text-xs text-gray-500">
                              {formatTicker(creator.ticker)}
                            </span>
                            <span
                              class={`${
                                percentageChange > 0
                                  ? "text-green-500"
                                  : percentageChange < 0
                                  ? "text-red-500"
                                  : "text-gray-500"
                              } text-xs font-semibold text-nowrap`}
                            >
                              {percentageChange > 0 && "↗"}
                              {percentageChange < 0 && "↘"}
                              {percentageChange.toFixed(2)}%
                            </span>
                          </div>
                        </div>
                      </div>
                    </a>
                  </td>
                  <td class="py-4 text-center max-md:px-2 max-md:text-sm max-md:font-semibold">
                    <a
                      href={creatorHref}
                      rel="noopener noreferrer"
                      class="block"
                    >
                      {formatValue(creator.value)}
                    </a>
                  </td>
                  <td class="flex items-center justify-center py-4 max-md:hidden">
                    <a
                      href={creatorHref}
                      rel="noopener noreferrer"
                      class="block"
                      aria-label="View Chart"
                    >
                      <div class="h-16 w-32">
                        <MiniChart history={creator.history} />
                      </div>
                    </a>
                  </td>
                  <td class="py-4 text-center max-md:px-2">
                    <a
                      href={creatorHref}
                      rel="noopener noreferrer"
                      class="block"
                    >
                      {creator.stream_status.is_live && (
                        <span class="inline-flex items-center rounded-full bg-red-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-red-500">
                          LIVE
                        </span>
                      )}
                      {!creator.stream_status.is_live && (
                        <span class="inline-flex items-center rounded-full bg-gray-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-gray-500">
                          OFFLINE
                        </span>
                      )}
                    </a>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      {totalPages > 1 && (
        <div class="mt-8 flex flex-col items-center gap-4 max-md:gap-3">
          <div class="text-sm text-gray-500 max-md:text-center max-md:text-sm dark:text-gray-400">
            {`Showing ${(props.index - 1) * 20 + 1} to ${
              Math.min(props.index * TAKE, props.creators.total)
            } of ${props.creators.total} creators`}
          </div>
          <div class="join max-md:scale-90 max-md:flex-wrap max-md:justify-center">
            <a
              href={nav({ index: props.index - 1 })}
              aria-label="Previous page"
              class="join-item btn rounded-l-2xl border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700"
            >
              «
            </a>
            {pageNumbers.map((p) => (
              <a
                href={nav({ index: p })}
                class={`join-item btn border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700 ${
                  props.index ===
                      p
                    ? "border-purple-400 bg-purple-400/80 text-white hover:bg-[#8f44fb]"
                    : "hover:bg-gray-100 dark:hover:bg-gray-800"
                }`}
              >
                {p}
              </a>
            ))}
            <a
              href={nav({ index: props.index + 1 })}
              class="join-item btn rounded-r-2xl border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700"
            >
              »
            </a>
          </div>
        </div>
      )}
    </div>
  );
}
