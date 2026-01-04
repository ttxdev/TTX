import { useComputed, useSignal, useSignalEffect } from "@preact/signals";
import { CreatorPartialDto, PlayerDto } from "../../lib/api.ts";
import { motion } from "motion/react";
import { State } from "../../utils.ts";
import { getApiClient } from "../../lib/index.ts";

export default function Drawer({ state }: { state: State }) {
  const showDrawer = useSignal(false);
  const self = useSignal<PlayerDto | null>(null);
  const apiClient = getApiClient(state.token);
  const shares = useComputed(() => {
    const offline: CreatorPartialDto[] = [];
    const online: CreatorPartialDto[] = [];
    const all: CreatorPartialDto[] = [];
    if (self.value === null) {
      return { online, offline, all };
    }

    self.value.shares.forEach((s) => {
      if (s.creator.stream_status.is_live) {
        online.push(s.creator);
      } else {
        offline.push(s.creator);
      }

      all.push(s.creator);
    });

    return { all, online, offline };
  });

  useSignalEffect(() => {
    if (!state.token) {
      return;
    }

    apiClient.getSelf().then((u) => {
      self.value = u;
    });
  });

  return (
    <>
      <button
        class="fixed right-4 bottom-4 cursor-pointer rounded-full bg-purple-600 p-4 text-white shadow-lg transition-colors hover:bg-purple-700"
        type="button"
        onClick={() => showDrawer.value = true}
        aria-label="Open Drawer"
      >
        <div class="relative">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M5 15l7-7 7 7"
            />
          </svg>
          {self.value && shares.value.online.length >= 1 && (
            <span class="absolute -top-2 -right-2 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-xs font-bold">
              {shares.value.offline.length + self.value.loot_boxes.length}
            </span>
          )}
        </div>
      </button>
      {showDrawer.value && (
        <motion.div class="fixed right-0 bottom-0 z-50 h-[85vh] w-full max-w-md overflow-hidden rounded-tl-2xl bg-white/80 shadow-2xl backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter md:h-[50vh] dark:bg-black/80">
          <div class="flex h-full flex-col">
            <div class="flex items-center justify-between border-b border-gray-200 p-4 dark:border-gray-700">
              <h2 class="text-xl font-semibold">Your Holdings</h2>
              <button
                class="rounded-full p-2 text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700"
                type="button"
                onClick={() => showDrawer.value = false}
                aria-label="Close drawer"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </button>
            </div>
            <div class="flex-1 overflow-y-auto p-4">
              {self.value === null && (
                <div class="flex flex-col items-center justify-center">
                  <p>Loading...</p>
                </div>
              )}
              {self.value !== null && (
                <div class="flex flex-col gap-4">
                  {self.value.loot_boxes.length > 0 && (
                    <a
                      href="/gamba"
                      class="flex items-center justify-between rounded-lg bg-purple-600 p-4 text-white transition-colors hover:bg-purple-700"
                    >
                      <span class="text-lg font-semibold">
                        Unopened Lootboxes
                      </span>
                      <span class="flex h-8 w-8 items-center justify-center rounded-full bg-white/20 text-lg font-bold">
                        {self.value.loot_boxes.length}
                      </span>
                    </a>
                  )}

                  <h3 class="text-xl font-semibold text-purple-500">
                    Currently Live
                  </h3>
                  {shares.value.online.length === 0 && (
                    <p class="text-gray-500">No streamers are currently live</p>
                  )}
                  {shares.value.online.length > 0 && (
                    <div class="flex flex-col gap-2">
                      {shares.value.online.map((creator) => (
                        <a
                          href={`/creators/${creator.slug}`}
                          class="flex items-center gap-3 rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
                        >
                          <div class="relative">
                            <img
                              src={creator.avatar_url}
                              alt=""
                              class="h-10 w-10 rounded-full"
                            />
                            <span class="absolute -top-1 -right-1 h-3 w-3 rounded-full bg-red-500">
                            </span>
                          </div>
                          <span class="font-medium">{creator.name}</span>
                        </a>
                      ))}
                    </div>
                  )}

                  <div class="mt-6 flex flex-col gap-4">
                    <h3 class="text-xl font-semibold text-gray-500">Offline</h3>
                    {shares.value.offline.length > 0 && (
                      <div class="flex flex-col gap-2">
                        {shares.value.offline.map((creator) => (
                          <a
                            href={`/creators/${creator.slug}`}
                            class="flex items-center gap-3 rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
                          >
                            <div class="relative">
                              <img
                                src={creator.avatar_url}
                                alt=""
                                class="h-10 w-10 rounded-full opacity-50"
                              />
                            </div>
                            <span class="font-medium text-gray-500">
                              {creator.name}
                            </span>
                          </a>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        </motion.div>
      )}
    </>
  );
}
