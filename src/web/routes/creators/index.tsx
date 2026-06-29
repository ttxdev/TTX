import {
  CreatorOrderBy,
  OrderDirection,
  PaginationDto_CreatorDto,
} from "@/lib/api.ts";
import { define } from "@/utils.ts";
import { Head, Partial } from "fresh/runtime";
import MiniChart from "@/islands/MiniChart.tsx";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
import { getApiClient } from "@/lib/index.ts";
import { calculatePercentChange } from "@/lib/math.ts";
import { nav } from "@/lib/url.ts";

const TAKE = 20;

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token, ctx.state.auth);
    const index = Number(ctx.url.searchParams.get("page")) || 1;
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

    return { data: { page, orderBy, orderDir, index, search } };
  },
});

export default define.page<typeof handler>((ctx) => {
  const { page, orderBy, orderDir, index, search } = ctx.data;

  return (
    <div>
      <Head>
        <title>TTX - Creators</title>
        <meta
          name="description"
          content="View the vast array of creators that you can pump and dump... I mean.... INVEST your hard earned tokens in on TTX"
        />
      </Head>
      <div class="mx-auto flex w-full max-w-250 flex-col space-y-10 p-4 max-md:my-2 max-md:space-y-6">
        <div class="flex items-end justify-between gap-4 max-md:flex-col max-md:items-stretch max-md:gap-3">
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
              Market
            </span>
            <h1 class="font-display text-5xl max-md:text-3xl">Creators</h1>
            <p class="text-sm opacity-60">
              Buy and sell shares of your favorite streamers.
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
                    placeholder="Search by Channel Name"
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
              {page.total} {page.total === 1 ? "Creator" : "Creators"}
            </span>
          </div>
        </div>

        <Partial name="creators">
          {page.data.length > 0
            ? (
              <CreatorTable
                url={ctx.url}
                creators={page}
                index={index}
                orderBy={orderBy}
                orderDir={orderDir}
              />
            )
            : (
              <div class="border-base-content/10 rounded-2xl border border-dashed py-16 text-center">
                <p class="font-display text-xl">No creators found</p>
                <p class="mt-1 text-sm opacity-60">
                  {search
                    ? `No channels match “${search}”.`
                    : "No channels available right now."}
                </p>
                {search && (
                  <a
                    href="/creators"
                    class="mt-4 inline-block font-semibold text-purple-500 hover:underline"
                  >
                    Clear search
                  </a>
                )}
              </div>
            )}
        </Partial>
      </div>
    </div>
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
    col: CreatorOrderBy;
    orderBy: CreatorOrderBy;
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
        {active && (
          <span>{orderDir === OrderDirection.Ascending ? "↑" : "↓"}</span>
        )}
      </a>
    </th>
  );
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

function StatusBadge({ live }: { live: boolean }) {
  if (live) {
    return (
      <span class="badge badge-sm gap-1 border-none bg-red-500/15 font-semibold text-red-500">
        <span class="inline-block size-2 animate-pulse rounded-full bg-red-500" />
        LIVE
      </span>
    );
  }
  return (
    <span class="badge badge-sm bg-base-content/10 border-none font-semibold opacity-60">
      Offline
    </span>
  );
}

function CreatorTable(props: {
  url: URL;
  creators: PaginationDto_CreatorDto;
  index: number;
  orderBy: CreatorOrderBy;
  orderDir: OrderDirection;
}) {
  const totalPages = Math.max(
    1,
    Math.floor((props.creators.total - 1) / TAKE) + 1,
  );
  const pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <div class="flex flex-col gap-8">
      <div class="border-base-content/10 overflow-hidden rounded-2xl border">
        <table class="table w-full">
          <thead class="bg-base-200/50 text-sm">
            <tr class="border-base-content/10 border-b">
              <SortHeader
                url={props.url}
                label="Creator"
                col={CreatorOrderBy.Name}
                orderBy={props.orderBy}
                orderDir={props.orderDir}
                class="py-4 text-left"
              />
              <SortHeader
                url={props.url}
                label="Price"
                col={CreatorOrderBy.Value}
                orderBy={props.orderBy}
                orderDir={props.orderDir}
                class="py-4 pr-4 text-right"
              />
              <th class="py-4 text-center font-medium opacity-50 max-md:hidden">
                Chart
              </th>
              <SortHeader
                url={props.url}
                label="Status"
                col={CreatorOrderBy.IsLive}
                orderBy={props.orderBy}
                orderDir={props.orderDir}
                class="py-4 pr-4 text-right"
              />
            </tr>
          </thead>
          <tbody>
            {props.creators.data.map((creator) => {
              const href = `/creators/${creator.slug}`;
              const change = calculatePercentChange(creator.history);
              const live = creator.stream_status.is_live;

              return (
                <tr class="group border-base-content/5 hover:bg-base-200/40 border-b transition-colors last:border-0">
                  <td class="py-3">
                    <div class="flex items-center gap-3 max-md:px-1">
                      <a href={href} class="relative shrink-0">
                        <img
                          src={creator.avatar_url}
                          alt={creator.name}
                          class={`size-11 rounded-full object-cover ring-2 transition-all ${
                            live
                              ? "ring-red-500/60"
                              : "ring-base-content/10 group-hover:ring-purple-500/50"
                          }`}
                        />
                        {live && (
                          <span class="border-base-100 absolute -right-0.5 -bottom-0.5 size-3 rounded-full border-2 bg-red-500" />
                        )}
                      </a>
                      <a href={href} class="flex min-w-0 flex-col">
                        <span class="truncate font-semibold transition-colors group-hover:text-purple-500">
                          {creator.name}
                        </span>
                        <span class="flex items-center gap-1.5">
                          <span class="font-mono text-xs opacity-50">
                            {formatTicker(creator.ticker)}
                          </span>
                          <Change change={change} />
                        </span>
                      </a>
                    </div>
                  </td>
                  <td class="py-3 pr-4 text-right">
                    <a
                      href={href}
                      class="font-display text-lg max-md:text-base"
                    >
                      {formatValue(creator.value)}
                    </a>
                  </td>
                  <td class="py-3 max-md:hidden">
                    <a
                      href={href}
                      class="mx-auto block h-12 w-32"
                      aria-label="View chart"
                    >
                      <MiniChart
                        value={creator.value}
                        history={creator.history}
                      />
                    </a>
                  </td>
                  <td class="py-3 pr-4 text-right">
                    <a href={href}>
                      <StatusBadge live={live} />
                    </a>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div class="flex flex-col items-center gap-4">
          <div class="text-sm opacity-50">
            Showing {(props.index - 1) * TAKE + 1} to{" "}
            {Math.min(props.index * TAKE, props.creators.total)} of{" "}
            {props.creators.total} creators
          </div>
          <div class="join max-md:flex-wrap max-md:justify-center">
            <PageLink
              url={props.url}
              page={props.index - 1}
              disabled={props.index <= 1}
              label="«"
              extra="rounded-l-2xl"
            />
            {pageNumbers.map((p) => (
              <a
                class={`join-item btn border-base-content/10 ${
                  props.index === p
                    ? "border-purple-600 bg-purple-600 text-white hover:bg-purple-700"
                    : "bg-base-200/40 hover:bg-base-200/70"
                }`}
                href={nav(props.url, { page: p })}
                aria-current={props.index === p ? "page" : undefined}
              >
                {p}
              </a>
            ))}
            <PageLink
              url={props.url}
              page={props.index + 1}
              disabled={props.index >= totalPages}
              label="»"
              extra="rounded-r-2xl"
            />
          </div>
        </div>
      )}
    </div>
  );
}
