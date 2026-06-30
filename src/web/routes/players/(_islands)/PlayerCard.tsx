import { PlayerDto, PortfolioSnapshotDto } from "@/lib/api.ts";
import { calculatePercentChange } from "@/lib/math.ts";
import BigChart from "@/islands/BigChart.tsx";
import { useSignal } from "@preact/signals";
import { useEffect } from "preact/hooks";
import { createHub } from "@/lib/ws.ts";
import CurrentValue from "@/islands/CurrentValue.tsx";
import { State } from "@/utils.ts";

export default function PlayerCard(
  { player, state }: {
    player: PlayerDto;
    state: State;
  },
) {
  const value = useSignal(player.value);
  const history = useSignal<PortfolioSnapshotDto[]>(player.history);

  const addSnapshot = ({ snapshot }: { snapshot: PortfolioSnapshotDto }) => {
    value.value = snapshot.value;
    history.value = [...history.value, snapshot];
  };

  useEffect(() => {
    const hub = createHub("portfolios", state.token);
    hub.on("UpdatePlayerPortfolioEvent", addSnapshot);
    hub.invoke("SetPlayer", player.id);
    hub.start().catch(console.error);
    return () => {
      hub.stop();
    };
  }, [player.id, state.token]);

  const change = calculatePercentChange(history.value);
  const changeClass = change > 0
    ? "bg-green-500/15 text-green-500"
    : change < 0
    ? "bg-red-500/15 text-red-500"
    : "bg-gray-500/15 text-gray-500";

  return (
    <div class="bg-base-200/50 w-full rounded-2xl bg-clip-padding p-4 shadow-md backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter">
      <div class="mb-4 flex flex-col items-center justify-between sm:flex-row">
        <div class="flex w-full flex-row items-center justify-between px-3">
          <div class="flex flex-row items-center gap-3">
            <img
              src={player.avatar_url}
              alt=""
              class="h-12 w-12 shrink-0 rounded-full border-2 border-white object-cover shadow-lg"
            />
            <div class="flex flex-col">
              <span class="text-lg font-semibold">{player.name}</span>
              <span class="text-xs opacity-60">Investor</span>
            </div>
          </div>
          <div class="relative flex flex-col items-center text-center">
            <CurrentValue value={value.value} />
            <p class="text-xs opacity-60">Portfolio Value</p>
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
        <BigChart value={value.value} history={history.value} />
      </div>
    </div>
  );
}
