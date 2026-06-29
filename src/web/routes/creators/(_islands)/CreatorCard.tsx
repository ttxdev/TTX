import { CreatorDto, VoteDto } from "../../../lib/api.ts";
import ExternalLink from "@/components/ExternalLink.tsx";
import { formatTicker } from "../../../lib/formatting.ts";
import { calculatePercentChange } from "../../../lib/math.ts";
import CurrentValue from "@/islands/CurrentValue.tsx";
import BigChart from "@/islands/BigChart.tsx";
import { State } from "../../../utils.ts";

export default function CreatorCard({ state, creator, value, history }: {
  state: State;
  creator: CreatorDto;
  value: number;
  history: VoteDto[];
}) {
  const change = calculatePercentChange(history);
  const changeClass = change > 0
    ? "bg-green-500/15 text-green-500"
    : change < 0
    ? "bg-red-500/15 text-red-500"
    : "bg-gray-500/15 text-gray-500";

  return (
    <div class="bg-base-200/50 w-full rounded-2xl bg-clip-padding p-4 shadow-md backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter">
      <div class="mb-4 flex flex-col items-center justify-between sm:flex-row">
        <div class="flex w-full flex-row items-center justify-between px-3">
          <div class="flex flex-row gap-3">
            <div class="flex flex-col items-center">
              <ExternalLink
                clientId={state.discordId}
                href={creator.platform_url}
                target="_blank"
              >
                <img
                  src={creator.avatar_url}
                  alt=""
                  class="h-12 w-12 rounded-full border-2 border-white object-cover shadow-lg"
                />
              </ExternalLink>
              {creator.stream_status.is_live && (
                <span class="-mt-2.5 h-fit w-fit rounded-full bg-red-400 px-2 text-xs font-bold text-white">
                  LIVE
                </span>
              )}
              {!creator.stream_status.is_live && (
                <span class="-mt-2.5 h-fit w-fit rounded-full bg-gray-600 px-2 text-xs font-bold text-white">
                  OFFLINE
                </span>
              )}
            </div>
            <div class="flex flex-col">
              <ExternalLink
                clientId={state.discordId}
                href={creator.platform_url}
                target="_blank"
                class="text-lg font-semibold text-purple-500 hover:underline"
              >
                {creator.name}
              </ExternalLink>
              <span class="font-mono text-sm text-opacity-60">
                {formatTicker(creator.ticker)}
              </span>
            </div>
          </div>
          <div class="relative flex flex-col items-center text-center">
            <CurrentValue value={value} />
            <p class="text-xs opacity-60">Current Price</p>
            <span
              class={`mt-1 inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold ${changeClass}`}
            >
              {change > 0 ? "▲" : change < 0 ? "▼" : "—"}{" "}
              {Math.abs(change).toFixed(2)}%
            </span>
          </div>
        </div>
      </div>
      <div class="h-[400px] w-full rounded-2xl border border-gray-200/15 p-4">
        <BigChart value={value} history={history} />
      </div>
    </div>
  );
}
