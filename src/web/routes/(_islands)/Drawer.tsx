import { useComputed, useSignal, useSignalEffect } from "@preact/signals";
import { PlayerDto, PlayerShareDto } from "../../lib/api.ts";
import { AnimatePresence, motion } from "motion/react";
import { State } from "../../utils.ts";
import { getApiClient } from "../../lib/index.ts";
import {
  formatShareAmount,
  formatTicker,
  formatValue,
} from "../../lib/formatting.ts";
import type { ComponentChildren } from "preact";

function StatCard(
  { label, value, class: accent, valueClass }: {
    label: string;
    value: ComponentChildren;
    class?: string;
    valueClass?: string;
  },
) {
  return (
    <div
      class={`flex flex-col gap-0.5 rounded-xl border p-2.5 ${
        accent ?? "border-base-content/10 bg-base-200/40"
      }`}
    >
      <span class="text-[10px] font-medium tracking-wider uppercase opacity-60">
        {label}
      </span>
      <span class={`truncate font-bold ${valueClass ?? ""}`}>{value}</span>
    </div>
  );
}

function HoldingRow(
  { share, dimmed = false }: { share: PlayerShareDto; dimmed?: boolean },
) {
  const c = share.creator;
  return (
    <a
      href={`/creators/${c.slug}`}
      class="group border-base-content/10 bg-base-200/40 hover:border-purple-500/30 hover:bg-base-200/70 flex items-center gap-3 rounded-xl border p-2.5 transition-colors"
    >
      <div class="relative shrink-0">
        <img
          src={c.avatar_url}
          alt=""
          class={`size-10 rounded-full object-cover ${
            dimmed ? "opacity-40 grayscale" : ""
          }`}
        />
        {!dimmed && (
          <span class="border-base-100 absolute -right-0.5 -bottom-0.5 size-3 rounded-full border-2 bg-red-500" />
        )}
      </div>
      <div class="flex min-w-0 flex-col">
        <span
          class={`truncate font-semibold transition-colors group-hover:text-purple-500 ${
            dimmed ? "opacity-60" : ""
          }`}
        >
          {c.name}
        </span>
        <span class="truncate font-mono text-xs opacity-50">
          {formatTicker(c.ticker)} · {formatValue(c.value)}
        </span>
      </div>
      <div class="ml-auto flex shrink-0 flex-col items-end">
        <span class="font-semibold">
          {formatValue(share.quantity * c.value)}
        </span>
        <span class="text-xs opacity-50">
          {formatShareAmount(share.quantity)} shares
        </span>
      </div>
    </a>
  );
}

function GiftIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      class="size-6"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <rect x="3" y="8" width="18" height="4" rx="1" />
      <path d="M12 8v13" />
      <path d="M19 12v7a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-7" />
      <path d="M7.5 8a2.5 2.5 0 0 1 0-5A4.8 8 0 0 1 12 8a4.8 8 0 0 1 4.5-5 2.5 2.5 0 0 1 0 5" />
    </svg>
  );
}

export default function Drawer({ state }: { state: State }) {
  const showDrawer = useSignal(false);
  const self = useSignal<PlayerDto | null>(null);
  const apiClient = getApiClient(state.token);

  const shares = useComputed(() => {
    const online: PlayerShareDto[] = [];
    const offline: PlayerShareDto[] = [];
    if (self.value === null) {
      return { online, offline, total: 0 };
    }

    self.value.shares.forEach((s) => {
      (s.creator.stream_status.is_live ? online : offline).push(s);
    });

    const byValue = (a: PlayerShareDto, b: PlayerShareDto) =>
      b.quantity * b.creator.value - a.quantity * a.creator.value;

    return {
      online: online.sort(byValue),
      offline: offline.sort(byValue),
      total: self.value.shares.length,
    };
  });

  const unopened = useComputed(() =>
    self.value?.loot_boxes.filter((b) => !b.is_open).length ?? 0
  );

  const totalShares = useComputed(() =>
    self.value?.shares.reduce((sum, s) => sum + s.quantity, 0) ?? 0
  );

  useSignalEffect(() => {
    if (!state.token || self.value) {
      return;
    }

    apiClient.getSelf().then((u) => {
      self.value = u;
    });
  });

  return (
    <>
      <AnimatePresence>
        {!showDrawer.value && state.user && (
          <motion.button
            class="fixed right-4 bottom-4 z-40 flex size-14 cursor-pointer items-center justify-center rounded-full bg-purple-600 text-white shadow-lg ring-1 ring-white/10 transition-colors hover:bg-purple-700"
            type="button"
            onClick={() => showDrawer.value = true}
            aria-label="Open your wallet"
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            exit={{ scale: 0 }}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.92 }}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="size-6"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
              aria-hidden="true"
            >
              <path d="M21 12V7H5a2 2 0 0 1 0-4h14v4" />
              <path d="M3 5v14a2 2 0 0 0 2 2h16v-5" />
              <path d="M18 12a2 2 0 0 0 0 4h4v-4Z" />
            </svg>
            {unopened.value > 0 && (
              <span class="absolute -top-1 -right-1 flex size-5">
                <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-red-500 opacity-75" />
                <span class="border-base-100 relative inline-flex size-5 items-center justify-center rounded-full border-2 bg-red-500 text-[10px] font-bold text-white">
                  {unopened.value}
                </span>
              </span>
            )}
          </motion.button>
        )}
      </AnimatePresence>

      {/* Click-outside backdrop */}
      <AnimatePresence>
        {showDrawer.value && (
          <motion.div
            class="fixed inset-0 z-40 bg-black/30 backdrop-blur-sm"
            onClick={() => showDrawer.value = false}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          />
        )}
      </AnimatePresence>

      {/* Drawer panel */}
      <AnimatePresence>
        {showDrawer.value && (
          <motion.div
            initial={{ y: 500 }}
            animate={{ y: 0 }}
            transition={{ duration: 0.25 }}
            exit={{ y: 500 }}
            class="bg-base-100/90 border-base-content/10 fixed right-0 bottom-0 z-50 flex h-[85vh] w-full max-w-md flex-col overflow-hidden rounded-t-2xl border shadow-2xl backdrop-blur md:h-[55vh]"
          >
            {/* Header */}
            <div class="border-base-content/10 border-b p-4">
              <div class="flex items-start justify-between">
                <h2 class="text-xl font-semibold">Wallet</h2>
                <button
                  class="hover:bg-base-content/10 cursor-pointer rounded-full p-2 opacity-60 transition-colors"
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

              <div class="mt-3 grid grid-cols-3 gap-2">
                <StatCard
                  label="Balance"
                  class="border-green-500/30 bg-green-500/10"
                  valueClass="text-green-500"
                  value={self.value !== null
                    ? formatValue(self.value.credits)
                    : (
                      <span class="loading loading-spinner loading-xs text-green-500" />
                    )}
                />
                <StatCard
                  label="Portfolio"
                  class="border-purple-500/30 bg-purple-500/10"
                  valueClass="text-purple-500"
                  value={self.value !== null
                    ? formatValue(self.value.portfolio)
                    : (
                      <span class="loading loading-spinner loading-xs text-purple-500" />
                    )}
                />
                <StatCard
                  label="Shares"
                  value={self.value !== null
                    ? formatShareAmount(totalShares.value)
                    : <span class="loading loading-spinner loading-xs" />}
                />
              </div>
            </div>

            {/* Body */}
            <div class="flex-1 overflow-y-auto p-4">
              {self.value === null
                ? (
                  <div class="flex h-full items-center justify-center">
                    <span class="loading loading-spinner text-purple-500" />
                  </div>
                )
                : (
                  <div class="flex flex-col gap-4">
                    {unopened.value > 0 && (
                      <a
                        href="/gamba"
                        class="group flex items-center gap-3 rounded-xl border border-amber-500/30 bg-gradient-to-r from-amber-500/15 to-yellow-500/10 p-3 transition-colors hover:border-amber-500/50"
                      >
                        <div class="flex size-11 shrink-0 items-center justify-center rounded-lg bg-amber-500/20 text-amber-400">
                          <GiftIcon />
                        </div>
                        <div class="flex min-w-0 flex-col">
                          <span class="font-semibold text-amber-400">
                            {unopened.value} Unopened{" "}
                            {unopened.value === 1 ? "Lootbox" : "Lootboxes"}
                          </span>
                          <span class="text-xs opacity-60">
                            Open them for a shot at rare creators
                          </span>
                        </div>
                        <span class="ml-auto flex shrink-0 items-center gap-1 text-sm font-semibold text-amber-400 transition-transform group-hover:translate-x-0.5">
                          Open
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            class="size-4"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            aria-hidden="true"
                          >
                            <path d="M5 12h14" />
                            <path d="m12 5 7 7-7 7" />
                          </svg>
                        </span>
                      </a>
                    )}

                    {shares.value.total === 0 && (
                      <div class="flex flex-col items-center gap-2 py-10 text-center">
                        <p class="font-semibold">No shares yet</p>
                        <p class="text-sm opacity-60">
                          Buy into a creator to start your portfolio.
                        </p>
                        <a
                          href="/creators"
                          class="mt-2 font-semibold text-purple-500 hover:underline"
                        >
                          Browse creators →
                        </a>
                      </div>
                    )}

                    {shares.value.total > 0 && (
                      <div class="flex flex-col gap-2">
                        <h3 class="flex items-center gap-2 font-semibold text-purple-500">
                          Currently Live
                          <span class="text-xs opacity-60">
                            {shares.value.online.length}
                          </span>
                        </h3>
                        {shares.value.online.length === 0
                          ? (
                            <p class="text-sm opacity-50">
                              None of your creators are live right now.
                            </p>
                          )
                          : (
                            shares.value.online.map((share) => (
                              <HoldingRow
                                key={`live-${share.creator.id}`}
                                share={share}
                              />
                            ))
                          )}

                        {shares.value.offline.length > 0 && (
                          <div class="mt-4 flex flex-col gap-2">
                            <h3 class="flex items-center gap-2 font-semibold opacity-60">
                              Offline
                              <span class="text-xs">
                                {shares.value.offline.length}
                              </span>
                            </h3>
                            {shares.value.offline.map((share) => (
                              <HoldingRow
                                key={`offline-${share.creator.id}`}
                                share={share}
                                dimmed
                              />
                            ))}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}
