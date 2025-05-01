<script lang="ts">
	import '../app.css';
	import Navbar from '$lib/components/Navbar.svelte';
	import Footer from '$lib/components/Footer.svelte';
	import type { LayoutProps } from './$types';
	import { onMount } from 'svelte';
	import { ProgressBar } from '@prgm/sveltekit-progress-bar';
	import { Toaster } from 'svelte-french-toast';
	import Search from '$lib/components/Search.svelte';
	import Drawer from '$lib/components/shared/Drawer.svelte';
	import { PUBLIC_API_BASE_URL as apiBaseUrl } from '$env/static/public';
	import { patchUrlMappings } from '@discord/embedded-app-sdk';
	import { discordSdk } from '$lib/discord';
	import { token, user } from '$lib/stores/data';
	import { startConnection } from '$lib/signalr';
	import type { CreatorTransactionDto, VoteDto } from '$lib/api';
	import { addVote } from '$lib/stores/votes';
	import { addTransaction } from '$lib/stores/transactions';

	let { data, children }: LayoutProps = $props();

	user.set(data.user);
	token.set(data.token);

	let searchModal = $state(false);
	let showDrawer = $state(false);

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			searchModal = false;
			return;
		}

		if (event.key.toLowerCase() !== 'k') return;

		const isMac = navigator.userAgent.includes('Mac');
		const hasModifier = isMac ? event.metaKey : event.ctrlKey;

		if (hasModifier) {
			event.preventDefault();
			searchModal = true;
		}
	}

	onMount(async () => {
		if (discordSdk) {
			const url = new URL(apiBaseUrl);

			patchUrlMappings([{ prefix: '/external-api', target: url.hostname }]);

			patchUrlMappings(
				[
					{ prefix: '/twitch-cdn', target: 'static-cdn.jtvnw.net' },
					{ prefix: '/github-cdn', target: 'avatars.githubusercontent.com' }
				],
				{
					patchSrcAttributes: true
				}
			);
		}

		const conn = await startConnection('events');
		conn.on('UpdateCreatorValue', (message: VoteDto) => addVote(message));
		conn.on('CreateTransaction', (message: CreatorTransactionDto) => addTransaction(message));
	});
</script>

<svelte:window onkeydown={handleKeydown} />

<ProgressBar color="#C09DF3" />

{#if searchModal}
	<Search bind:searchModal></Search>
{/if}

<Toaster />

<Navbar bind:searchModal />
<main class="container mx-auto flex w-full grow flex-col pb-24 md:p-24">
	{@render children()}
</main>
<Footer />
{#if data.liveHoldings}
	<button
		class="fixed right-4 bottom-4 cursor-pointer rounded-full bg-purple-600 p-4 text-white shadow-lg transition-colors hover:bg-purple-700"
		onclick={() => (showDrawer = true)}
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
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
			</svg>
			{#await data.liveHoldings then holdings}
				{#if holdings.filter((h) => h.isLive).length > 1}
					<span
						class="absolute -top-2 -right-2 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-xs font-bold"
					>
						{holdings.filter((h) => h.isLive).length}
					</span>
				{/if}
			{/await}
		</div>
	</button>
{/if}

{#if showDrawer && data.liveHoldings}
	<Drawer closeDrawer={() => (showDrawer = false)}>
		{#await data.liveHoldings}
			<div class="flex flex-col items-center justify-center">
				<p>Loading...</p>
			</div>
		{:then holdings}
			<div class="flex flex-col gap-4">
				<h3 class="text-xl font-semibold text-purple-500">Currently Live</h3>
				{#if holdings.filter((h) => h.isLive).length === 0}
					<p class="text-gray-500">No streamers are currently live</p>
				{:else}
					<div class="flex flex-col gap-2">
						{#each holdings.filter((h) => h.isLive) as holding}
							<a
								href="/creators/{holding.slug}"
								class="flex items-center gap-3 rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
								onclick={() => (showDrawer = false)}
							>
								<div class="relative">
									<img src={holding.avatar} alt={holding.creator} class="h-10 w-10 rounded-full" />
									<span class="absolute -top-1 -right-1 h-3 w-3 rounded-full bg-red-500"></span>
								</div>
								<span class="font-medium">{holding.creator}</span>
							</a>
						{/each}
					</div>
				{/if}

				<div class="mt-6 flex flex-col gap-4">
					<h3 class="text-xl font-semibold text-gray-500">Offline</h3>
					{#if holdings.filter((h) => !h.isLive).length > 0}
						<div class="flex flex-col gap-2">
							{#each holdings.filter((h) => !h.isLive) as holding}
								<a
									href="/creators/{holding.slug}"
									class="flex items-center gap-3 rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
									onclick={() => (showDrawer = false)}
								>
									<div class="relative">
										<img
											src={holding.avatar}
											alt={holding.creator}
											class="h-10 w-10 rounded-full opacity-50"
										/>
									</div>
									<span class="font-medium text-gray-500">{holding.creator}</span>
								</a>
							{/each}
						</div>
					{/if}
				</div>
			</div>
		{/await}
	</Drawer>
{/if}
