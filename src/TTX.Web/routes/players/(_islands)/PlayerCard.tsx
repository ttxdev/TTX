import { Placement } from "../(_components)/PlayerPlacement.tsx";
import { PlayerDto, PortfolioSnapshotDto } from "@/lib/api.ts";
import { formatValue } from "@/lib/formatting.ts";
import BigChart from "./BigChart.tsx";
import { useSignal, useSignalEffect } from "@preact/signals";
import { createHub } from "@/lib/signalr.ts";
import CurrentValue from "@/islands/CurrentValue.tsx";
import { State } from "@/utils.ts";
import { HubConnection } from "@microsoft/signalr";

export default function PlayerCard(
  { player, isStreamer, state }: {
    placement: Placement;
    player: PlayerDto;
    isStreamer: boolean;
    state: State;
  },
) {
  const hub = useSignal<HubConnection | null>(null);
  const value = useSignal(player.value);
  const history = useSignal<PortfolioSnapshotDto[]>(player.history);

  const addSnapshot = (snapshot: PortfolioSnapshotDto) => {
    value.value = snapshot.value;
    history.value = [...history.value, snapshot];
  };

  useSignalEffect(() => {
    if (hub.value !== null) {
      return;
    }

    const newHub = createHub("portfolios", state.token);
    newHub.on("UpdatePlayerPortfolioEvent", addSnapshot);
    newHub.start()
      .then(() => {
        hub.value = newHub;
        return hub.value.invoke("SetPlayer", player.id);
      }).catch(console.error);
  });

  return (
    <div class="player-card bg-base-200/50 w-full rounded-lg p-2 shadow-md backdrop-blur
             backdrop-contrast-100 backdrop-saturate-100 sm:p-4">
      <div class="m-2 flex flex-col gap-2 sm:m-3 sm:flex-row sm:items-center sm:justify-between">
        <div class="bg-base-300 rounded-lg px-2 py-1 text-base font-medium sm:text-xl">
          Portfolio Value
        </div>
        {isStreamer && (
          <a
            href={`/creators/${player.slug}`}
            class="bg-primary hover:bg-primary/80 inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium text-white transition-colors sm:text-sm"
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
      <div class="chart-container relative mb-3 h-36 w-full sm:mb-4 sm:h-48">
        <BigChart value={value.value} history={history.value} />
      </div>

      <div class="flex items-center justify-between px-1 sm:px-2">
        <div class="flex items-center gap-1.5 sm:gap-3">
          <img
            src={player.avatar_url}
            alt="Avatar"
            class="h-8 w-8 rounded-full border-2 border-white object-cover shadow-lg sm:h-12 sm:w-12"
          />
          <div class="flex flex-col">
            <span class="text-sm font-semibold sm:text-lg">
              {player.name}
            </span>
          </div>
        </div>

        <div class="flex flex-row gap-8 text-right">
          <div class="flex flex-col text-center">
            <CurrentValue value={value.value} />
            <p class="text-sm">Portfolio Value</p>
          </div>
          <div class="flex flex-col text-center">
            <h1 class="text-xl font-bold">
              {formatValue(player.credits)}
            </h1>
            <p class="text-sm">Credits</p>
          </div>
        </div>
      </div>
    </div>
  );
}
