<script lang="ts">
	import CreatorCard from '$lib/components/channel/CreatorCard.svelte';
	import BiggestHolders from '$lib/components/channel/LargestCreatorHolders.svelte';
	import LatestTransactions from '$lib/components/channel/LatestCreatorTransactions.svelte';
	import type { PageProps } from './$types';
	import BuySellModal from '$lib/components/channel/BuySellModal.svelte';
	import { onDestroy, onMount } from 'svelte';
	import { getApiClient } from '$lib';
	import { TimeStep, Vote } from '$lib/api';
	import { addRecentStreamer } from '$lib/utils/recentStreamers';

	let { data }: PageProps = $props();
	let creator = $state(data.creator);

	let history = $state<Vote[]>(data.creator.history);
	let buySellModal: string = $state('none');
	let pullTask: number | null = null;

	function setModal(modal: string) {
		buySellModal = modal;
	}

	onDestroy(() => {
		if (pullTask) {
			clearInterval(pullTask);
		}
	});

	onMount(() => {
		addRecentStreamer({
			id: creator.id,
			name: creator.name,
			slug: creator.slug,
			avatar_url: creator.avatar_url,
			ticker: creator.ticker
		});

		const client = getApiClient('');
		let last = new Date();

		pullTask = setInterval(async () => {
			const data = await client
				.getCreatorValueHistory(creator.slug, TimeStep._0, last)
				.then((res) => res.history);
			last = new Date();
			if (data.length === 0) return;

			history = [...history, ...data];
			creator.value = data[data.length - 1].value;
		}, 1_000);
	});

	$effect(() => {
		if (data.creator.slug === creator.slug) return;
		creator = data.creator;
		history = data.creator.history;
	});
</script>

<svelte:head>
	<title>TTX - {creator.name}</title>
	<meta name="description" content="TTX Creator Page" />
</svelte:head>

{#if buySellModal !== 'none'}
	<BuySellModal
		bind:type={buySellModal}
		slug={creator.slug}
		ticker={creator.ticker}
		price={creator.value}
	/>
{/if}

<section class="mx-auto flex w-full max-w-[1000px] flex-col gap-4 p-4">
	<CreatorCard {creator} {history} />
	<div class="flex flex-col gap-4 md:flex-row">
		<div class="divider divider-vertical md:hidden"></div>
		<div class="join flex w-full flex-col items-center justify-center md:flex-row">
			<div class="join mt-2 md:mt-0">
				<button
					class="btn btn-lg h-10 rounded-l-2xl border-2 p-4 text-green-400"
					onclick={() => setModal('buy')}
				>
					Buy
				</button>
				<button
					class="btn btn-lg h-10 rounded-r-2xl p-4 text-red-400"
					onclick={() => setModal('sell')}
				>
					Sell
				</button>
			</div>
		</div>
	</div>
	<div class="divider divider-vertical md:hidden"></div>
	<div class="flex w-full items-center justify-between">
		<h2 class="text-2xl font-bold">Investors</h2>
	</div>
</section>

<section class="mx-auto flex w-full max-w-[1000px] flex-col gap-4 px-4">
	<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
		<BiggestHolders holders={data.holders} price={creator.value} />
		<LatestTransactions transactions={data.transactions} />
	</div>
</section>

<style>
	input[type='number']::-webkit-outer-spin-button,
	input[type='number']::-webkit-inner-spin-button {
		-webkit-appearance: none;
		margin: 0;
	}

	input[type='number'] {
		appearance: textfield;
		-moz-appearance: textfield;
	}
</style>
