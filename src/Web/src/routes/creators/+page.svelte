<script lang="ts">
	import ChannelTable from '$lib/components/ChannelTable.svelte';
	import type { PageProps } from './$types';
	import { afterNavigate } from '$app/navigation';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { fade, fly } from 'svelte/transition';

	let { data }: PageProps = $props();
	let searchQuery = $state('');
	let showPopup = $state(false);

	onMount(() => {
		const hasClosedPopup = localStorage.getItem('creatorPopupClosed');
		if (!hasClosedPopup) {
			showPopup = true;
		}
	});

	function closePopup() {
		showPopup = false;
		localStorage.setItem('creatorPopupClosed', 'true');
	}

	afterNavigate(({ from }) => {
		if (from?.url.pathname === '/creators') {
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
	<title>TTX - Channels</title>
	<meta
		name="description"
		content="View the vast array of channels that you can pump and dump... I mean.... INVEST your hard earned tokens in on TTX"
	/>
</svelte:head>

{#if showPopup}
	<div
		class="fixed right-4 bottom-4 z-50 max-w-sm"
		in:fly={{ y: 20, duration: 300, delay: 200 }}
		out:fade={{ duration: 200 }}
	>
		<div
			class="rounded-lg border border-purple-200 bg-white/95 p-3 shadow-lg backdrop-blur-sm dark:border-purple-800 dark:bg-gray-900/95"
			in:fly={{ y: 10, duration: 200 }}
		>
			<div class="flex items-center gap-3">
				<div class="flex-1">
					<h3 class="text-sm font-medium text-purple-600 dark:text-purple-400">Become a Creator</h3>
					<p class="mt-0.5 text-xs text-gray-600 dark:text-gray-400"></p>
				</div>
				<div class="flex items-center gap-2">
					<a
						href="/apply"
						class="btn btn-xs rounded-full border-purple-400 bg-purple-400 px-3 text-white transition-all duration-200 hover:scale-105 hover:bg-purple-500 dark:border-purple-500 dark:bg-purple-500 dark:hover:bg-purple-600"
					>
						Apply
					</a>
					<button
						onclick={closePopup}
						class="btn btn-ghost btn-xs rounded-full p-1 text-gray-400 transition-all duration-200 hover:scale-110 hover:bg-gray-100 dark:text-gray-500 dark:hover:bg-gray-800"
						aria-label="Close"
					>
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="h-4 w-4"
							viewBox="0 0 20 20"
							fill="currentColor"
						>
							<path
								fill-rule="evenodd"
								d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
								clip-rule="evenodd"
							/>
						</svg>
					</button>
				</div>
			</div>
		</div>
	</div>
{/if}

<div
	class="mx-auto flex w-full max-w-[1000px] flex-col space-y-12 p-4 max-md:my-2 max-md:space-y-6"
>
	<div class="flex items-center justify-between gap-4 max-md:flex-col max-md:gap-2">
		<p class="font-display self-start text-center text-5xl max-md:text-3xl">Creators</p>
		<div class="flex flex-col gap-2 max-md:flex-col max-md:items-start max-md:justify-start">
			<form class="join max-w-md" onsubmit={handleSearch}>
				<label
					class="input-bordered input flex-1 rounded-l-2xl border-purple-400 focus:outline-none"
				>
					<svg class="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
						><g
							stroke-linejoin="round"
							stroke-linecap="round"
							stroke-width="2.5"
							fill="none"
							stroke="currentColor"
							><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></g
						></svg
					>
					<input
						type="text"
						bind:value={searchQuery}
						class=" focus:outline-none"
						placeholder="Search by Channel Name"
					/>
				</label>
				<button type="submit" class="btn rounded-r-2xl border-purple-400 bg-purple-400 text-white">
					Search
				</button>
			</form>
			<span class="w-full text-right text-xs opacity-50">
				{data.total}
				{data.total === 1 ? 'Creator' : 'Creators'}
			</span>
		</div>
	</div>

	{#if data.channels && data.channels.length > 0}
		<ChannelTable
			creators={data.channels}
			total={data.total}
			currentPage={data.currentPage}
			sortField={data.sortField}
			sortDirection={data.sortDirection}
		/>
	{:else}
		<p>No channels available</p>
	{/if}
</div>

<style scoped>
	* {
		scroll-behavior: smooth;
	}
</style>
