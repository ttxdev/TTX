import { Signal, useSignal } from "@preact/signals";
import { debounce } from "@std/async/debounce";
import Modal from "@/islands/Modal.tsx";
import { useRef } from "preact/hooks";
import { getRecentCreators, SearchResult } from "../../lib/search.ts";
import { getApiClient } from "../../lib/index.ts";
import { formatTicker } from "../../lib/formatting.ts";
import { State } from "../../utils.ts";

function ResultRow(
  { item, index, selected }: {
    item: SearchResult;
    index: number;
    selected: boolean;
  },
) {
  const href = item.type === "player"
    ? `/players/${item.slug}`
    : `/creators/${item.slug}`;

  return (
    <a
      href={href}
      data-index={index}
      class={`flex items-center gap-3 rounded-xl border p-3 transition-colors ${
        selected
          ? "border-purple-500/40 bg-purple-500/10"
          : "hover:border-base-content/10 hover:bg-base-200/60 border-transparent"
      }`}
    >
      <img
        src={item.avatar_url}
        alt=""
        class={`size-10 shrink-0 rounded-full object-cover ring-2 transition-all ${
          selected ? "ring-purple-500" : "ring-base-content/10"
        }`}
      />
      <div class="flex min-w-0 flex-col">
        <span
          class={`truncate font-semibold ${selected ? "text-purple-500" : ""}`}
        >
          {item.name}
        </span>
        <span class="truncate font-mono text-xs opacity-60">
          {item.ticker ? formatTicker(item.ticker) : "Player"}
        </span>
      </div>
      <span class="border-base-content/10 ml-auto shrink-0 rounded-full border px-2 py-0.5 text-[10px] font-semibold tracking-wider uppercase opacity-50">
        {item.type}
      </span>
    </a>
  );
}

export default function SearchModal(
  { state, isSearchOpen }: { state: State; isSearchOpen: Signal<boolean> },
) {
  const apiClient = getApiClient(state.token);
  const recent = getRecentCreators();
  const result = useSignal<SearchResult[]>([]);
  const isLoading = useSignal(false);
  const selectedIndex = useSignal(-1);
  const query = useSignal("");
  const resultContainer = useRef<HTMLDivElement | null>(null);
  const search = debounce(async () => {
    if (query.value.length === 0) {
      result.value = [];
      return;
    }

    isLoading.value = true;

    try {
      result.value = await Promise.all([
        apiClient.getPlayers(1, 3, query.value).then((players) =>
          players.data.map<SearchResult>((p) => ({
            id: p.id,
            name: p.name,
            slug: p.name.toLowerCase(),
            avatar_url: p.avatar_url,
            type: "player",
          }))
        ),
        apiClient.getCreators(1, 5, query.value).then((creators) =>
          creators.data.map<SearchResult>((c) => ({
            id: c.id,
            name: c.name,
            slug: c.slug,
            avatar_url: c.avatar_url,
            type: "creator",
            ticker: c.ticker,
          }))
        ),
      ]).then((r) => r.flat());
    } finally {
      isLoading.value = false;
    }
  }, 300);

  function close() {
    isLoading.value = false;
    query.value = "";
    result.value = [];
    selectedIndex.value = -1;
    isSearchOpen.value = false;
  }

  function scrollSelectedIntoView() {
    if (selectedIndex.value >= 0 && resultContainer.current) {
      const selectedElement = resultContainer.current.querySelector(
        `[data-index="${selectedIndex.value}"]`,
      );
      if (selectedElement) {
        selectedElement.scrollIntoView({
          block: "nearest",
          behavior: "smooth",
        });
      }
    }
  }

  function handleKeydown(event: KeyboardEvent) {
    const items = result.value.length > 0 ? result.value : recent;
    if (!items.length) return;

    switch (event.key) {
      case "ArrowDown":
        event.preventDefault();
        selectedIndex.value = Math.min(
          selectedIndex.value + 1,
          items.length - 1,
        );
        scrollSelectedIntoView();
        break;
      case "ArrowUp":
        event.preventDefault();
        selectedIndex.value = Math.max(selectedIndex.value - 1, -1);
        scrollSelectedIntoView();
        break;
      case "Enter":
        event.preventDefault();
        if (selectedIndex.value >= 0 && selectedIndex.value < items.length) {
          const item = items[selectedIndex.value];
          const url = item.type === "player"
            ? `/players/${item.slug}`
            : `/creators/${item.slug}`;

          globalThis.location.replace(url);
        }
        break;
      case "Escape":
        event.preventDefault();
        close();
        break;
    }
  }

  const showResults = result.value.length > 0;
  const noResults = query.value.length > 0 && !isLoading.value && !showResults;
  const showRecent = query.value.length === 0 && !isLoading.value &&
    !showResults;

  return (
    <Modal isOpen={isSearchOpen.value} onClose={close}>
      <button
        type="button"
        onClick={close}
        aria-label="close"
        className="modal-backdrop absolute size-full"
      >
        <div class="fixed inset-0 size-full bg-black/30 backdrop-blur-sm"></div>
      </button>
      <div class="modal-box bg-base-100/95 border-base-content/10 z-10 flex h-[28rem] w-11/12 max-w-md cursor-default flex-col rounded-2xl border p-4 shadow-2xl shadow-purple-500/20 backdrop-blur md:w-full">
        <div class="join flex">
          <input
            type="text"
            ref={(self) => {
              if (self) {
                requestAnimationFrame(() => {
                  self.focus();
                });
              }
            }}
            autoFocus
            placeholder="Search players and creators..."
            class="input join-item flex-1 rounded-l-2xl border-purple-500/40 focus-within:border-purple-500 focus:outline-none"
            value={query.value}
            onInput={(e) => {
              query.value = (e.currentTarget as HTMLInputElement).value;
              selectedIndex.value = -1;
              search();
            }}
            onKeyDown={handleKeydown}
          />
          <button
            type="button"
            aria-label="search"
            onClick={search}
            class="btn join-item rounded-r-2xl border-purple-600 bg-purple-600 text-white hover:bg-purple-700"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              stroke-width="1.5"
              stroke="currentColor"
              class="size-5"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
              />
            </svg>
          </button>
        </div>

        <div
          ref={resultContainer}
          class="mt-3 flex flex-1 flex-col gap-1 overflow-y-auto"
        >
          {isLoading.value && (
            <div class="flex flex-1 items-center justify-center text-purple-500">
              <span class="loading loading-spinner loading-md"></span>
            </div>
          )}

          {showResults &&
            result.value.map((item, index) => (
              <ResultRow
                key={`search-${item.type}-${item.id}`}
                item={item}
                index={index}
                selected={index === selectedIndex.value}
              />
            ))}

          {noResults && (
            <div class="flex flex-1 flex-col items-center justify-center gap-1 text-center">
              <p class="font-semibold">No results found</p>
              <p class="text-sm opacity-50">
                Nothing matches “{query.value}”.
              </p>
            </div>
          )}

          {showRecent && recent.length > 0 && (
            <>
              <h3 class="px-1 pb-1 text-xs font-semibold tracking-widest text-purple-500 uppercase">
                Recently Visited
              </h3>
              {recent.map((item, index) => (
                <ResultRow
                  key={`recent-${item.type}-${item.id}`}
                  item={item}
                  index={index}
                  selected={index === selectedIndex.value}
                />
              ))}
            </>
          )}

          {showRecent && recent.length === 0 && (
            <div class="flex flex-1 flex-col items-center justify-center gap-1 text-center opacity-50">
              <p class="font-semibold">Start typing to search</p>
              <p class="text-sm">Find any player or creator on TTX.</p>
            </div>
          )}
        </div>

        <div class="border-base-content/10 mt-3 flex items-center gap-3 border-t pt-2 text-[11px] opacity-50">
          <span class="flex items-center gap-1">
            <kbd class="kbd kbd-xs">↑</kbd>
            <kbd class="kbd kbd-xs">↓</kbd>
            navigate
          </span>
          <span class="flex items-center gap-1">
            <kbd class="kbd kbd-xs">↵</kbd>
            open
          </span>
          <span class="flex items-center gap-1">
            <kbd class="kbd kbd-xs">esc</kbd>
            close
          </span>
        </div>
      </div>
    </Modal>
  );
}
