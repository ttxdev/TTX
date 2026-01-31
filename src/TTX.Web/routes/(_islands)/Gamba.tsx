import { State } from "@/utils.ts";
import { getApiClient } from "../../lib/index.ts";
import { useSignal } from "@preact/signals";
import { LootBoxDto, LootBoxResultDto } from "../../lib/api.ts";
import Spinner from "./Gamba/Spinner.tsx";
import LootBoxCard from "./Gamba/LootBoxCard.tsx";
import WinnerModal from "./Gamba/WinnerModal.tsx";

type GambaState = "idle" | "spinning" | "modal" | "complete";

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

  async function open(id: number) {
    result.value = await client.gamba(id);
    gState.value = "spinning";
  }

  return (
    <div>
      {gState.value === "modal" && result.value && (
        <WinnerModal
          result={result.value}
          onClose={() => gState.value = "complete"}
        />
      )}
      <div class="my-auto flex flex-col items-center p-4 md:p-8">
        <h1 class="mb-8 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-3xl font-bold text-transparent md:text-5xl">
          TTX GAMBA
        </h1>

        <div class="flex w-full flex-col items-center md:max-w-4xl">
          {result.value && (
            <div class="mb-8 flex w-full flex-col items-center gap-6">
              <Spinner
                result={result.value}
                onComplete={() => gState.value = "modal"}
              />
            </div>
          )}
          {!result.value && (
            <div>
              {unopened.value.length > 0 && (
                <div>
                  <h2 class="mb-4 text-xl font-bold text-amber-400">
                    Available Lootboxes
                  </h2>
                  <div class="mb-12 grid grid-cols-1 gap-6 sm:grid-cols-2 md:grid-cols-3">
                    {unopened.value.map((box) => (
                      <LootBoxCard
                        id={box.id}
                        key={`box-${box.id}`}
                        onOpen={() => open(box.id)}
                      />
                    ))}
                  </div>
                </div>
              )}
              {unopened.value.length === 0 && (
                <div class="text-center text-gray-400">
                  <p class="text-xl">No lootboxes available</p>
                  <p class="mt-2">Come back later to try your luck!</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
