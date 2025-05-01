<script lang="ts">
	import CreatorCard from '$lib/components/channel/CreatorCard.svelte';
	import BiggestHolders from '$lib/components/channel/LargestCreatorHolders.svelte';
	import LatestTransactions from '$lib/components/channel/LatestCreatorTransactions.svelte';
	import BuySellModal from '$lib/components/channel/BuySellModal.svelte';
	import IntervalSelector from './IntervalSelector.svelte';
	import { onDestroy, onMount } from 'svelte';
	import { setVotes, voteStore } from '$lib/stores/votes';
	import {
		CreatorTransactionDto,
		TransactionAction,
		VoteDto,
		type ICreatorShareDto
	} from '$lib/api';
	import { addRecentStreamer } from '$lib/utils/recentStreamers';
	import { discordSdk } from '$lib/discord';
	import type { PageProps } from './$types';
	import { setTransactions, transactionStore } from '$lib/stores/transactions';

	let { data }: PageProps = $props();
	let creator = $state.raw(data.creator);
	let history = $state.raw<VoteDto[]>([]);
	let transactions = $state.raw<CreatorTransactionDto[]>(data.transactions);
	let shares = $state.raw<ICreatorShareDto[]>(data.creator.shares);
	let buySellModal: TransactionAction | null = $state(null);
	let interval = $state(data.interval);
	function setModal(modal: TransactionAction) {
		buySellModal = modal;
	}

	voteStore.subscribe((store) => {
		const votes = store.get(creator.id);
		if (votes) {
			history = votes;
		}
	});

	transactionStore.subscribe((store) => {
		const storeTxs = store.get(creator.id);
		if (storeTxs) {
    		transactions = storeTxs.toSorted((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime());
		}
	});

	transactionStore.subscribe((store) => {
		const storeTxs = store.get(data.creator.id)
		  ?.toSorted((a, b) => new Date(a.created_at).getTime() - new Date(b.created_at).getTime());
		if (!storeTxs) {
			return;
		}

		const newShares = new Map<number, ICreatorShareDto>();
		storeTxs.forEach((t) => {
    		const share: ICreatorShareDto = newShares.get(t.player_id)
                      ?? { player: t.player, quantity: 0 };

			if (t.action === TransactionAction.Buy) {
				share.quantity += t.quantity;
			} else if (t.action === TransactionAction.Sell) {
				share.quantity -= t.quantity;
			}

			newShares.set(share.player.id, share);
		});

		shares = newShares.values().filter((share) => share.quantity > 0).toArray();
	});

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

	onMount(() => {
		setTransactions(data.creator.id, data.transactions);
		setVotes(data.creator.id, data.creator.history);
	})

	$effect(() => {
		if (data.creator.slug === creator.slug && data.interval === interval) return;

        creator = data.creator;
        interval = data.interval;
        shares = data.creator.shares;
        setTransactions(data.creator.id, data.transactions);
		setVotes(data.creator.id, data.creator.history);
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
