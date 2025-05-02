<script lang="ts">
	import { getApiClient } from '$lib';
	import { debounce } from 'lodash-es';
	import { token } from '$lib/stores/data';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { getRecentStreamers, type RecentStreamer } from '$lib/utils/recentStreamers';
	import type { CreatorPartialDto, PlayerPartialDto } from '$lib/api';

	let searchQuery: string = $state('');
	let searchResults: Array<{
		id: number;
		name: string;
		ticker?: string;
		type: 'player' | 'creator';
		slug: string;
		avatar_url: string;
	}> = $state([]);
	let isLoading = $state(false);
	let recentStreamers: RecentStreamer[] = $state([]);
	let selectedIndex = $state(-1);
	let resultsContainer: HTMLDivElement | null = $state(null);

	let { searchModal = $bindable() }: { searchModal: boolean } = $props();

	const client = getApiClient(String($token || ''));

	let searchInput: HTMLInputElement | null = null;

	const performSearch = debounce(async (query: string) => {
		if (!query) {
			searchResults = [];
			selectedIndex = -1;
			return;
		}

		isLoading = true;
		try {
			const [players, creators] = await Promise.all([
				client.getPlayers(1, 3, query),
				client.getCreators(1, 5, query)
			]);

			searchResults = [
				...creators.data.map((creator: CreatorPartialDto) => ({
					id: creator.id,
					name: creator.name,
					ticker: creator.ticker,
					type: 'creator' as const,
					slug: creator.slug,
					avatar_url: creator.avatar_url
				})),
				...players.data.map((player: PlayerPartialDto) => ({
					id: player.id,
					name: player.name,
					type: 'player' as const,
					slug: player.name.toLowerCase(),
					avatar_url: player.avatar_url
				}))
			];
			selectedIndex = -1;
		} catch (error) {
			console.error('Search error:', error);
		} finally {
			isLoading = false;
		}
	}, 300);

	function close() {
		searchModal = false;
		searchQuery = '';
		searchResults = [];
		isLoading = false;
		selectedIndex = -1;
	}

	function handleLinkClick() {
		searchModal = false;
	}

	function scrollSelectedIntoView() {
		if (selectedIndex >= 0 && resultsContainer) {
			const selectedElement = resultsContainer.querySelector(`[data-index="${selectedIndex}"]`);
			if (selectedElement) {
				selectedElement.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
			}
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		const items = searchQuery ? searchResults : recentStreamers;
		if (!items.length) return;

		switch (event.key) {
			case 'ArrowDown':
				event.preventDefault();
				selectedIndex = Math.min(selectedIndex + 1, items.length - 1);
				scrollSelectedIntoView();
				break;
			case 'ArrowUp':
				event.preventDefault();
				selectedIndex = Math.max(selectedIndex - 1, -1);
				scrollSelectedIntoView();
				break;
			case 'Enter':
				event.preventDefault();
				if (selectedIndex >= 0 && selectedIndex < items.length) {
					const result = items[selectedIndex];
					let url: string;

					if (searchQuery) {
						// Handle search results
						const searchResult = result as (typeof searchResults)[0];
						url =
							searchResult.type === 'player'
								? `/players/${searchResult.slug}`
								: `/creators/${searchResult.slug}`;
					} else {
						// Handle recent streamers
						const streamer = result as RecentStreamer;
						url = `/creators/${streamer.slug}`;
					}

					goto(url);
					close();
				}
				break;
			case 'Escape':
				event.preventDefault();
				close();
				break;
		}
	}

	$effect(() => {
		if (searchQuery) {
			performSearch(searchQuery);
		} else {
			recentStreamers = getRecentStreamers();
		}
	});

	onMount(() => {
		$effect(() => {
			if (searchModal && searchInput !== null) {
				searchInput.focus();
			}
		});
		recentStreamers = getRecentStreamers();
	});
</script>

<div class="modal modal-open">
	<button onclick={close} aria-label="close">
		<div class="fixed inset-0 bg-black/10 backdrop-blur-sm"></div>
	</button>
	<div
		class="modal-box bg-base-200/50 relative z-10 flex h-[28rem] w-full max-w-md flex-col rounded-xl p-6 shadow-2xl shadow-purple-500/20"
	>
		<div class="join flex">
			<input
				bind:this={searchInput}
				type="text"
				placeholder="Search users and creators..."
				class="input-bordered input custom-number-input flex-1 rounded-l-2xl rounded-r-none border-purple-400 focus:outline-none"
				bind:value={searchQuery}
				onkeydown={handleKeydown}
			/>
			<button class="btn rounded-r-2xl border-purple-400 text-purple-400" aria-label="search">
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

		<div class="rounded-box mt-2 flex-1 overflow-y-auto shadow-lg" bind:this={resultsContainer}>
			{#if searchQuery}
				{#if isLoading}
					<div class="p-4 text-center text-purple-400">
						<span class="loading loading-spinner loading-md"></span>
					</div>
				{:else if searchResults.length > 0}
					<table class="table w-full">
						<tbody>
							{#each searchResults as result, index (result.type + result.id)}
								<tr
									class="w-full rounded-md transition-all duration-200 {index === selectedIndex
										? 'border-l-4 border-purple-500 bg-gradient-to-r from-purple-500/10 to-purple-500/5'
										: 'hover:bg-base-200/30'}"
									data-index={index}
								>
									<td class="w-full p-0">
										<a
											href={result.type === 'player'
												? `/players/${result.slug}`
												: `/creators/${result.slug}`}
											onclick={handleLinkClick}
											class="flex w-full cursor-pointer items-center gap-4 p-4"
										>
											{#if result.avatar_url}
												<img
													src={result.avatar_url}
													alt={result.name}
													class="size-10 rounded-full {index === selectedIndex
														? 'ring-2 ring-purple-500'
														: ''}"
												/>
											{:else}
												<span class={index === selectedIndex ? 'text-purple-500' : ''}
													>{result.name[0]}</span
												>
											{/if}
											<div class="flex flex-col items-start">
												<span
													class="text-xl font-semibold {index === selectedIndex
														? 'text-purple-500'
														: ''}">{result.name}</span
												>
												{#if result.ticker}
													<div
														class="text-sm {index === selectedIndex
															? 'text-purple-400'
															: 'opacity-70'}"
													>
														{result.ticker}
													</div>
												{:else}
													<div
														class="text-sm {index === selectedIndex
															? 'text-purple-400'
															: 'opacity-70'}"
													>
														PLAYER
													</div>
												{/if}
											</div>
										</a>
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				{:else}
					<div class="p-4 text-center text-purple-400">No results found</div>
				{/if}
			{:else if recentStreamers.length > 0}
				<div class="p-4">
					<h3 class="mb-4 text-lg font-semibold text-purple-400">Recently Visited</h3>
					<table class="table w-full">
						<tbody>
							{#each recentStreamers as streamer, index (streamer.id)}
								<tr class="w-full rounded-md">
									<td class="w-full p-0">
										<a
											href={`/creators/${streamer.slug}`}
											onclick={handleLinkClick}
											class="flex w-full cursor-pointer items-center gap-4 p-4 {index ===
											selectedIndex
												? 'border-l-4 border-purple-500 bg-gradient-to-r from-purple-500/10 to-purple-500/5'
												: 'hover:bg-base-200/30'}"
											data-index={index}
										>
											{#if streamer.avatar_url}
												<img
													src={streamer.avatar_url}
													alt={streamer.name}
													class="size-10 rounded-full {index === selectedIndex
														? 'ring-2 ring-purple-500'
														: ''}"
												/>
											{:else}
												<span class={index === selectedIndex ? 'text-purple-500' : ''}
													>{streamer.name[0]}</span
												>
											{/if}
											<div class="flex flex-col items-start">
												<span
													class="text-xl font-semibold {index === selectedIndex
														? 'text-purple-500'
														: ''}">{streamer.name}</span
												>
												{#if streamer.ticker}
													<div
														class="text-sm {index === selectedIndex
															? 'text-purple-400'
															: 'opacity-70'}"
													>
														{streamer.ticker}
													</div>
												{/if}
											</div>
										</a>
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{:else}
				<div class="p-4 text-center text-purple-400">Start typing to search</div>
			{/if}
		</div>
	</div>
</div>
