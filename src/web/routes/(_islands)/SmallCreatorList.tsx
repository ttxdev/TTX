import { useEffect, useRef } from "preact/hooks";
import type { CreatorPartialDto } from "../../lib/api.ts";
import { Chart } from "@/lib/chart.ts";
import {
  formatTicker,
  formatToChart,
  formatValue,
} from "../../lib/formatting.ts";
import { calculatePercentChange } from "../../lib/math.ts";

function Item({ creator }: { creator: CreatorPartialDto }) {
  const canvas = useRef<HTMLCanvasElement | null>(null);
  const href = `/creators/${creator.slug}`;
  const data = formatToChart(creator.value, creator.history);
  const isLive = creator.stream_status?.is_live;
  const change = calculatePercentChange(creator.history);
  const isUp = change >= 0;
  const lineColor = isUp ? "#22c55e" : "#ef4444";

  useEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: Array(data.labels.length).fill(""),
        datasets: [
          {
            data: data.values,
            borderColor: lineColor,
            borderWidth: 2,
            fill: false,
            tension: 0,
            pointRadius: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false,
          },
        },
        scales: {
          x: {
            display: false,
          },
          y: {
            display: false,
          },
        },
      },
    });

    return () => chart.destroy();
  }, [canvas]);

  return (
    <a
      href={href}
      class="group border-base-content/10 bg-base-200/40 hover:border-purple-500/30 flex flex-col gap-3 rounded-2xl border p-4 transition-colors"
    >
      {/* Header: who they are + live status */}
      <div class="flex items-center justify-between gap-2">
        <div class="flex min-w-0 items-center gap-3">
          <img
            src={creator.avatar_url}
            alt=""
            class={`size-11 shrink-0 rounded-full ${
              isLive ? "ring-2 ring-red-500/60" : ""
            }`}
          />
          <div class="flex min-w-0 flex-col">
            <span class="truncate font-semibold group-hover:underline">
              {creator.name}
            </span>
            <span class="font-mono text-xs opacity-60">
              {formatTicker(creator.ticker)}
            </span>
          </div>
        </div>
        {isLive
          ? (
            <span class="badge badge-sm shrink-0 gap-1 border-none bg-red-500/15 font-semibold text-red-500">
              <span class="inline-block size-2 animate-pulse rounded-full bg-red-500" />
              LIVE
            </span>
          )
          : (
            <span class="badge badge-sm bg-base-content/10 shrink-0 border-none font-semibold opacity-50">
              OFFLINE
            </span>
          )}
      </div>

      {/* Sparkline */}
      <div class="h-[56px] w-full">
        <canvas ref={canvas}></canvas>
      </div>

      {/* Footer: price + trend */}
      <div class="flex items-center justify-between">
        <span class="font-semibold">{formatValue(creator.value)}</span>
        <span
          class={`text-sm font-medium ${
            isUp ? "text-green-500" : "text-red-500"
          }`}
        >
          {isUp ? "▲" : "▼"} {Math.abs(change).toFixed(2)}%
        </span>
      </div>
    </a>
  );
}

export default function SmallCreatorList(
  { creators, viewMoreHref = "/creators" }: {
    creators: CreatorPartialDto[];
    viewMoreHref?: string;
  },
) {
  return (
    <div class="grid w-full gap-4 sm:grid-cols-[repeat(3,minmax(0,1fr))_auto]">
      {creators.map((creator) => (
        <Item key={`leaderboard-${creator.id}`} creator={creator} />
      ))}
      <a
        href={viewMoreHref}
        class="group border-base-content/10 hover:border-purple-500/30 flex flex-col items-center justify-center gap-2 rounded-2xl border border-dashed p-4 text-center transition-colors hover:bg-purple-500/5 max-sm:p-6"
      >
        <span class="flex size-10 items-center justify-center rounded-full bg-purple-500/10 text-purple-500 transition-transform group-hover:translate-x-0.5">
          <svg
            class="size-5"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M5 12h14" />
            <path d="m12 5 7 7-7 7" />
          </svg>
        </span>
        <span class="text-sm font-semibold whitespace-nowrap text-purple-500">
          View more
        </span>
      </a>
    </div>
  );
}
