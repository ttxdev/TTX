import { State } from "@/utils.ts";
import { getApiClient } from "../../lib/index.ts";
import { useSignal } from "@preact/signals";
import { LootBoxDto, LootBoxResultDto } from "../../lib/api.ts";
import { toast } from "@/lib/toast.ts";
import Spinner from "./Gamba/Spinner.tsx";
import LootBoxCard from "./Gamba/LootBoxCard.tsx";
import WinnerModal from "./Gamba/WinnerModal.tsx";

type GambaState = "idle" | "spinning" | "modal";

export default function Gamba(
  { boxes, state }: {
    boxes: LootBoxDto[];
    state: State;
  },
) {
  const client = getApiClient(state.token);
  const result = useSignal<LootBoxResultDto | null>(null);
  const gState = useSignal<GambaState>("idle");
  const unopened = useSignal<LootBoxDto[]>(boxes);
  const openedId = useSignal<number | null>(null);

  async function open(id: number) {
    if (gState.value !== "idle") return;
    openedId.value = id;
    try {
      result.value = await client.gamba(id);
      gState.value = "spinning";
    } catch (e) {
      console.error(e);
      toast.error("Couldn't open that lootbox. Try again.");
      openedId.value = null;
    }
  }

  function finish() {
    unopened.value = unopened.value.filter((b) => b.id !== openedId.value);
    result.value = null;
    openedId.value = null;
    gState.value = "idle";
  }

  return (
    <div class="mx-auto flex max-w-4xl flex-col items-center p-4 md:p-8">
      {gState.value === "modal" && result.value && (
        <WinnerModal result={result.value} onClose={finish} />
      )}

      {/* Header */}
      <div class="mb-8 flex flex-col items-center gap-2 text-center">
        <h1 class="bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-4xl font-black text-transparent md:text-6xl">
          TTX GAMBA
        </h1>
        <p class="max-w-md text-sm opacity-60">
          Open your lootboxes for a shot at rare creators. Pure luck, zero skill
          — and definitely not real money.
        </p>
      </div>

      <div class="flex w-full flex-col items-center">
        {result.value
          ? (
            <div class="mb-8 flex w-full flex-col items-center gap-6">
              <Spinner
                result={result.value}
                onComplete={() => gState.value = "modal"}
              />
            </div>
          )
          : unopened.value.length > 0
          ? (
            <div class="w-full">
              <div class="mb-5 flex items-center justify-between">
                <h2 class="text-xl font-bold text-amber-400">Your Lootboxes</h2>
                <span class="rounded-full border border-amber-500/40 bg-amber-500/10 px-3 py-0.5 text-sm font-semibold text-amber-400">
                  {unopened.value.length} available
                </span>
              </div>
              <div class="grid grid-cols-2 justify-items-center gap-6 sm:grid-cols-3 md:grid-cols-4">
                {unopened.value.map((box) => (
                  <LootBoxCard
                    id={box.id}
                    key={`box-${box.id}`}
                    onOpen={() => open(box.id)}
                  />
                ))}
              </div>
            </div>
          )
          : (
            <div class="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-amber-500/20 bg-amber-500/5 p-12 text-center">
              <div class="text-4xl">🎰</div>
              <p class="text-xl font-bold text-amber-400">
                No lootboxes available
              </p>
              <p class="max-w-xs text-sm opacity-60">
                Earn lootboxes by playing TTX. Come back later to try your luck!
              </p>
              <a
                href="/creators"
                class="btn mt-2 rounded-lg border-none bg-gradient-to-r from-amber-500 to-yellow-500 font-bold text-black shadow transition-transform hover:scale-105"
              >
                Browse creators
              </a>
            </div>
          )}
      </div>
    </div>
  );
}
