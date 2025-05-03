<script lang="ts">
	import PlayerLeaderboard from '$lib/components/PlayerLeaderboard.svelte';
	import type { PageProps } from './$types';
	import { afterNavigate } from '$app/navigation';
	import { goto } from '$app/navigation';

	let { data }: PageProps = $props();
	let searchQuery = '';

	afterNavigate(({ from }) => {
		if (from?.url.pathname === '/leaderboard') {
			window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
		}
	});

	function handleSearch(event: Event) {
		event.preventDefault();
		if (searchQuery.trim()) {
			goto(`?page=1&search=${encodeURIComponent(searchQuery.trim())}`, {
				keepFocus: true,
				invalidateAll: true,
				noScroll: true
			});
		} else {
			goto(`?page=1`, {
				keepFocus: true,
				invalidateAll: true,
				noScroll: true
			});
		}
	}
</script>

<svelte:head>
	<title>TTX - Leaderboard</title>
	<meta
		name="description"
		content="Check out the top players ranked by their portfolio value on the leaderboard."
	/>
</svelte:head>

<div
	class="mx-auto flex w-full max-w-[1000px] flex-col space-y-12 p-4 max-md:my-2 max-md:space-y-6"
>
	<div class="flex items-center justify-between gap-4 max-md:flex-col max-md:gap-2">
		<p class="font-display self-start text-center text-5xl max-md:text-3xl">Leaderboard</p>
		<div class="flex flex-col gap-2 max-md:flex-col max-md:items-start max-md:justify-start">
			<form class="join max-w-md" onsubmit={handleSearch}>
				<label
					class="input-bordered input flex-1 rounded-l-2xl border-purple-400 focus:outline-none"
				>
					<svg class="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
						<g
							stroke-linejoin="round"
							stroke-linecap="round"
							stroke-width="2.5"
							fill="none"
							stroke="currentColor"
						>
							<circle cx="11" cy="11" r="8"></circle>
							<path d="m21 21-4.3-4.3"></path>
						</g>
					</svg>
					<input
						type="text"
						bind:value={searchQuery}
						class="focus:outline-none"
						placeholder="Search by Player Name"
					/>
				</label>
				<button type="submit" class="btn rounded-r-2xl border-purple-400 bg-purple-400 text-white">
					Search
				</button>
			</form>
			<span class="w-full text-right text-xs opacity-50">
				{data.total}
				{data.total === 1 ? 'Player' : 'Players'}
			</span>
		</div>
	</div>

	{#if data.players && data.players.length > 0}
		<PlayerLeaderboard players={data.players} total={data.total} currentPage={data.currentPage} />
	{:else}
		<p>No players available</p>
	{/if}
</div>

<style scoped>
	* {
		scroll-behavior: smooth;
	}
</style>
