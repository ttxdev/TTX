<script lang="ts">
	import Card from './../shared/Card.svelte';
	import type { TransactionDto } from '$lib/api';
	import TimeStamp from '$lib/components/shared/TimeStamp.svelte';
	import { formatShareAmount, formatValue } from '$lib/util';

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
					<tr class="flex flex-row justify-between rounded-md py-1 md:p-2">
						<td class="flex items-center justify-center gap-3">
							<a href="/players/{tx.user.name}" class="flex flex-col">
								<img alt={tx.user.name} src={tx.user.avatar_url} class="size-10 rounded-full" />
							</a>
							<div class="flex flex-col">
								<span class="text-xl font-semibold">{tx.action == 0 ? 'Bought' : 'Sold'}</span>
								<a href="/players/{tx.user.name}" class="text-sm text-violet-500 hover:underline">
									{tx.user.name}
								</a>
							</div>
						</td>
						<td class="flex flex-col items-center justify-center p-2 text-right font-bold">
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
