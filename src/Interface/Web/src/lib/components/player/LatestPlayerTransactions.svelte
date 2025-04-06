<script lang="ts">
	import type { TransactionDto } from '$lib/api';
	import TimeStamp from '$lib/components/shared/TimeStamp.svelte';
	import { formatCreatorString, formatShareAmount, formatValue } from '$lib/util';
	import Card from '../shared/Card.svelte';

	let { transactions }: { transactions: TransactionDto[] } = $props();
</script>

<Card title="Latest Transactions">
	{#if transactions.length === 0}
		<div class="flex items-center justify-center p-6">
			<b>No transactions yet</b>
		</div>
	{:else}
		<table class="table">
			<tbody>
				{#each transactions as tx (tx.id)}
					<tr class="flex flex-row justify-between rounded-md p-2 py-1">
						<td class="flex items-center justify-center gap-3">
							<a href="/channels/{tx.creator.slug}" class="flex flex-col">
								<img
									alt={tx.creator.name}
									src={tx.creator.avatar_url}
									class="size-10 rounded-full"
								/>
							</a>
							<div class="flex flex-col">
								<span class="text-xl font-semibold">{tx.action == 0 ? 'Bought' : 'Sold'}</span>
								<a
									href="/channels/{tx.creator.slug}"
									class="text-sm text-violet-500 hover:underline"
								>
									{formatCreatorString(tx.creator.name)}
								</a>
							</div>
						</td>
						<td class="flex flex-col items-center justify-end p-2 text-right font-bold">
							<span class="text-md md:text-xl">
								{formatShareAmount(tx.quantity)} @ {formatValue(tx.value)}
							</span>
							<div class="w-full text-right opacity-55">
								<TimeStamp date={tx.created_at} />
							</div>
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	{/if}
</Card>
