import type { CreatorDto } from "@/lib/api.ts";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
import { calculatePercentChange } from "@/lib/math.ts";
import MiniChart from "@/islands/MiniChart.tsx";

/**
 * The hero "featured creator" card. Static markup wrapping the shared
 * {@link MiniChart} island (the only interactive piece), so it matches the
 * trend-coloured gradient charts used everywhere else.
 */
export default function FeaturedCreator({ creator }: { creator: CreatorDto }) {
  const change = calculatePercentChange(creator.history);
  const up = change >= 0;
  const live = creator.stream_status?.is_live;

  return (
    <a
      href={`/creators/${creator.slug}`}
      class="group border-base-content/10 bg-base-200/40 hover:border-purple-500/30 flex h-full min-h-80 w-full flex-col gap-4 rounded-2xl border p-5 transition-colors"
    >
      <div class="flex items-start justify-between gap-3">
        <div class="flex items-center gap-3">
          <div class="relative shrink-0">
            <img
              src={creator.avatar_url}
              alt=""
              class={`size-12 rounded-full object-cover ring-2 ${
                live ? "ring-red-500/60" : "ring-base-content/10"
              }`}
            />
            {live && (
              <span class="border-base-100 absolute -right-0.5 -bottom-0.5 size-3 rounded-full border-2 bg-red-500" />
            )}
          </div>
          <div class="flex flex-col">
            <span class="flex items-center gap-2">
              <span class="font-semibold transition-colors group-hover:text-purple-500">
                {creator.name}
              </span>
              {live && (
                <span class="badge badge-sm gap-1 border-none bg-red-500/15 font-semibold text-red-500">
                  <span class="inline-block size-2 animate-pulse rounded-full bg-red-500" />
                  LIVE
                </span>
              )}
            </span>
            <span class="font-mono text-xs opacity-60">
              {formatTicker(creator.ticker)}
            </span>
          </div>
        </div>
        <span class="border-base-content/10 shrink-0 rounded-full border px-2 py-0.5 text-[10px] font-semibold tracking-widest uppercase opacity-50">
          Featured
        </span>
      </div>

      <div class="min-h-0 flex-1">
        <MiniChart value={creator.value} history={creator.history} />
      </div>

      <div class="flex items-end justify-between">
        <div class="flex flex-col">
          <span class="text-xs tracking-widest uppercase opacity-50">
            Current Price
          </span>
          <span class="font-display text-2xl">
            {formatValue(creator.value)}
          </span>
        </div>
        <span
          class={`badge gap-1 border-none font-semibold ${
            up ? "bg-green-500/15 text-green-500" : "bg-red-500/15 text-red-500"
          }`}
        >
          {up ? "▲" : "▼"} {Math.abs(change).toFixed(1)}%
        </span>
      </div>
    </a>
  );
}
