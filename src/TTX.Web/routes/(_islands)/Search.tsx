import { useSignal } from "@preact/signals";
import { debounce } from "@std/async/debounce";
import Modal from "@/islands/Modal.tsx";
import { useRef } from "preact/hooks";
import { getRecentCreators, SearchResult } from "../../lib/search.ts";
import { getApiClient } from "../../lib/index.ts";
import { State } from "../../utils.ts";

export default function Search({ state }: { state: State }) {
  const isSearchOpen = useSignal(false);
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

  return (
    <>
      <button
        type="button"
        onClick={() => {
          isSearchOpen.value = true;
        }}
        class="relative z-10 rounded-2xl"
      >
        Search
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
      <Modal isOpen={isSearchOpen.value}>
        <button type="button" onClick={close} aria-label="close">
          <div class="fixed inset-0 bg-black/10 backdrop-blur-sm"></div>
        </button>
        <div class="modal-box bg-base-200/50 relative z-10 flex h-[28rem] w-full max-w-md flex-col rounded-xl p-6 shadow-2xl shadow-purple-500/20">
          <div class="join flex">
            <input
              type="text"
              placeholder="Search users and creators..."
              class="input-bordered input custom-number-input flex-1 rounded-l-2xl rounded-r-none border-purple-400 focus:outline-none"
              onInput={(e) => {
                // @ts-ignore: ignore
                query.value = e.target!.value;
                search();
              }}
              onKeyDown={handleKeydown}
            />
            <button
              type="button"
              aria-label="search"
              onClick={search}
              class="btn rounded-r-2xl border-purple-400 text-purple-400"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                stroke-width="1.5"
                stroke="currentColor"
                class="size-6"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
                />
              </svg>
            </button>
          </div>

          <div class="rounded-box mt-2 flex-1 overflow-y-auto shadow-lg">
            {isLoading.value && (
              <div class="p-4 text-center text-purple-400">
                <span class="loading loading-spinner loading-md"></span>
              </div>
            )}
            {result.value.length > 0 && (
              <table class="table w-full">
                <tbody>
                  {result.value.map((item, index) => {
                    return (
                      <tr
                        class={`w-full rounded-md transition-all duration-200 ${
                          index === selectedIndex.value
                            ? "border-l-4 border-purple-500 bg-gradient-to-r from-purple-500/10 to-purple-500/5"
                            : "hover:bg-base-200/30"
                        }`}
                        key={`search-${item.type}-${item.id}`}
                      >
                        <td class="w-full p-0">
                          <a
                            href={item.type === "player"
                              ? `/players/${item.slug}`
                              : `/creators/${item.slug}`}
                            class="flex w-full cursor-pointer items-center gap-4 p-4"
                          >
                            <img
                              src={item.avatar_url}
                              alt={item.name}
                              class="size-10 rounded-full {index === selectedIndex
                  														? 'ring-2 ring-purple-500'
                  														: ''}"
                            />
                            <div class="flex flex-col items-start">
                              <span class="text-xl font-semibold {index === selectedIndex
                  														? 'text-purple-500'
                  														: ''}">
                                {result.name}
                              </span>
                              {item.ticker && (
                                <span
                                  class={`text-sm ${
                                    index === selectedIndex.value
                                      ? "text-purple-400"
                                      : "opacity-70"
                                  }`}
                                >
                                  {item.ticker}
                                </span>
                              )}
                              {!item.ticker && (
                                <span
                                  class={`text-sm ${
                                    index === selectedIndex.value
                                      ? "text-purple-400"
                                      : "opacity-70"
                                  }`}
                                >
                                  PLAYER
                                </span>
                              )}
                            </div>
                          </a>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
            {(query.value.length > 0 && !isLoading.value &&
              result.value.length === 0) && (
              <span class="p-4 text-center text-purple-400">
                No results found
              </span>
            )}
            {(query.value.length === 0 && !isLoading.value &&
              result.value.length === 0) && (
              <div class="p-4">
                <h3 class="mb-4 text-lg font-semibold text-purple-400">
                  Recently Visited
                </h3>
                <table class="table w-full">
                  <tbody>
                    {recent.map((creator, index) => {
                      return (
                        <tr
                          key={`recent-creator-${creator.id}`}
                          class="w-full rounded-md"
                        >
                          <td class="w-full p-0">
                            <a
                              href={`/creators/${creator.slug}`}
                              class={`flex w-full cursor-pointer items-center gap-4 p-4 ${
                                index === selectedIndex.value
                                  ? "border-l-4 border-purple-500 bg-gradient-to-r from-purple-500/10 to-purple-500/5"
                                  : "hover:bg-base-200/30"
                              }`}
                            >
                              <img
                                src={creator.avatar_url}
                                alt=""
                                class="size-10 rounded-full {index === selectedIndex
             														? 'ring-2 ring-purple-500'
             														: ''}"
                              />
                              <div class="flex flex-col items-start">
                                <span class="text-xl font-semibold {index === selectedIndex
             														? 'text-purple-500'
             														: ''}">
                                  {creator.name}
                                </span>
                                <div class="text-sm {index === selectedIndex
             															? 'text-purple-400'
             															: 'opacity-70'}">
                                  {creator.ticker}
                                </div>
                              </div>
                            </a>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
            {recent.length === 0 && query.value.length === 0 && (
              <span class="p-4 text-center text-purple-400">
                Start typing to search
              </span>
            )}
          </div>
        </div>
      </Modal>
    </>
  );
}
