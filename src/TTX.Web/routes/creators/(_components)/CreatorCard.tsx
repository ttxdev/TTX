import { CreatorDto, VoteDto } from "../../../lib/api.ts";
import ExternalLink from "@/components/ExternalLink.tsx";
import { formatTicker } from "../../../lib/formatting.ts";
import CurrentValue from "../(_islands)/CurrentValue.tsx";
import BigChart from "../(_islands)/BigChart.tsx";
import { State } from "../../../utils.ts";
import { Signal } from "@preact/signals";

export default function CreatorCard(
  { state, creator, history }: {
    state: State;
    creator: CreatorDto;
    history: Signal<VoteDto[]>;
  },
) {
  const currentValue = creator.history[creator.history.length - 1]?.value ??
    creator.value;

  return (
    <div class="bg-base-200/50 w-full rounded-lg bg-clip-padding p-4 shadow-md backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter">
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
          <div class="relative flex flex-col text-center">
            <CurrentValue value={currentValue} />
            <p class="w-24 text-sm">Current Price</p>
          </div>
        </div>
      </div>
      <div class="relative min-h-[400px] w-full">
        <div class="absolute h-3/4 w-full rounded-lg border border-gray-200/15 p-4">
          <BigChart history={history} />
        </div>
      </div>
    </div>
  );
}
