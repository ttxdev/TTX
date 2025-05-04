<script lang="ts">
	import CreatorCard from '$lib/components/channel/CreatorCard.svelte';
	import BiggestHolders from '$lib/components/channel/LargestCreatorHolders.svelte';
	import LatestTransactions from '$lib/components/channel/LatestCreatorTransactions.svelte';
	import BuySellModal from '$lib/components/channel/BuySellModal.svelte';
	import IntervalSelector from './IntervalSelector.svelte';
	import { onDestroy, onMount } from 'svelte';
	import { setVotes, voteStore } from '$lib/stores/votes';
	import {
		TransactionAction,
		type ICreatorShareDto
	} from '$lib/api';
	import { addRecentStreamer } from '$lib/utils/recentStreamers';
	import { discordSdk } from '$lib/discord';
	import type { PageProps } from './$types';
	import { setTransactions, transactionStore } from '$lib/stores/transactions';

	let { data }: PageProps = $props();
	let creator = $derived(data.creator);
	let buySellModal: TransactionAction | null = $state(null);
	let interval = $derived(data.interval);

	$effect(() => {
		setTransactions(data.creator.id, data.transactions);
		setVotes(data.creator.id, data.creator.history);
	});

	let history = $derived.by(() => $voteStore.get(creator.id) ?? data.creator.history);
	let transactions = $derived.by(() => {
		return ($transactionStore.get(creator.id) ?? data.transactions).toSorted(
			(a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime()
		);
	});

	let shares = $derived.by(() => {
		const storeTxs = ($transactionStore.get(creator.id) ?? data.transactions)
			.toSorted((a, b) => new Date(a.created_at).getTime() - new Date(b.created_at).getTime());
		const newShares = new Map<number, ICreatorShareDto>();

		storeTxs.forEach((t) => {
			const share: ICreatorShareDto = newShares.get(t.player_id) ?? {
				player: t.player,
				quantity: 0
			};

			if (t.action === TransactionAction.Buy) {
				share.quantity += t.quantity;
			} else if (t.action === TransactionAction.Sell) {
				share.quantity -= t.quantity;
			}

			newShares.set(t.player.id, share);
		});

		return Array.from(newShares.values())
			.filter((share) => share.quantity > 0);
	});

	function setModal(modal: TransactionAction) {
		buySellModal = modal;
	}

	onDestroy(() => {
		if (discordSdk) {
			void discordSdk.commands.setActivity({
				activity: {
					type: 0,
					state: 'TTX',
					assets: {
						large_image: 'ttx',
						large_text: 'TTX'
					}
				}
			});
		}
	});

	onMount(async () => {
		addRecentStreamer({
			id: creator.id,
			name: creator.name,
			slug: creator.slug,
			avatar_url: creator.avatar_url,
			ticker: creator.ticker
		});

		if (discordSdk) {
			await discordSdk.commands.setActivity({
				activity: {
					type: 0,
					state: 'TTX',
					assets: {
						large_image: 'ttx',
						large_text: 'TTX',
						small_image: creator.avatar_url,
						small_text: creator.name
					}
				}
			});
		}
	});
</script>

<svelte:head>
	<title>TTX - {creator.name}</title>
	<meta name="description" content="TTX Creator Page" />
</svelte:head>

{#if buySellModal !== null}
	<BuySellModal
		bind:type={buySellModal}
		slug={creator.slug}
		ticker={creator.ticker}
		price={creator.value}
	/>
{/if}

<section class="mx-auto flex w-full max-w-[1000px] flex-col gap-4 p-4">
	<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between">
		<h1 class="text-2xl font-bold">{creator.name}</h1>
		{#await data.isPlayer then isPlayer}
			{#if isPlayer}
				<a
					href="/players/{creator.slug}"
					class="bg-primary hover:bg-primary/80 inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium text-white transition-colors sm:text-sm"
				>
					<svg
						xmlns="http://www.w3.org/2000/svg"
						class="h-3 w-3 sm:h-4 sm:w-4"
						viewBox="0 0 20 20"
						fill="currentColor"
					>
						<path
							fill-rule="evenodd"
							d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-8.707l-3-3a1 1 0 00-1.414 0l-3 3a1 1 0 001.414 1.414L9 9.414V13a1 1 0 102 0V9.414l1.293 1.293a1 1 0 001.414-1.414z"
							clip-rule="evenodd"
						/>
					</svg>
					<p>Switch to player profile</p>
				</a>
			{/if}
		{/await}
		<div class="flex justify-end">
			<IntervalSelector {interval} />
		</div>
	</div>
	<CreatorCard {creator} {history} />

	<div class="flex flex-col gap-4 md:flex-row">
		<div class="divider divider-vertical md:hidden"></div>
		<div class="join flex w-full flex-col items-center justify-center md:flex-row">
			<div class="join mt-2 md:mt-0">
				<button
					class="btn btn-lg h-10 rounded-l-2xl border-2 p-4 text-green-400"
					onclick={() => setModal(TransactionAction.Buy)}
				>
					Buy
				</button>
				<button
					class="btn btn-lg h-10 rounded-r-2xl p-4 text-red-400"
					onclick={() => setModal(TransactionAction.Sell)}
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
		<BiggestHolders {shares} price={creator.value} />
		<LatestTransactions {transactions} />
	</div>
</section>
