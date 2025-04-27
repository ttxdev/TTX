<script lang="ts">
	import type { CreatorShareDto } from '$lib/api';
	import { formatShareAmount, formatValue } from '$lib/util';
	import Card from '../shared/Card.svelte';
	import PlayerPlacement from '../shared/PlayerPlacement.svelte';

	let { shares, price }: { shares: CreatorShareDto[]; price: number } = $props();
	const sortedHolders = $derived(shares.sort((a, b) => b.quantity - a.quantity));

	let total = $derived(
		Object.fromEntries(
			shares.map((holder) => [holder.player.name, formatValue(holder.quantity * price)])
		)
	);
</script>

<Card title="Largest Holders">
	{#if shares.length !== 0}
		<table class="table">
			<tbody>
				{#each sortedHolders as holder, index (holder.player.id)}
					<tr class="flex flex-row justify-between rounded-md py-1 md:p-2">
						<td class="flex items-center justify-center gap-3">
							<a href="/players/{holder.player.slug}" class="flex flex-col">
								<img
									alt={holder.player.name}
									src={holder.player.avatar_url}
									class="size-10 rounded-full"
								/>
							</a>
							<div class="flex flex-col">
								<div class="flex scale-90">
									<PlayerPlacement place={index + 1} />
								</div>
								<a
									href="/players/{holder.player.slug}"
									class="text-sm text-violet-500 hover:underline"
								>
									{holder.player.name}
								</a>
							</div>
						</td>
						<td class="flex flex-col items-center justify-center p-2 text-right font-bold">
							<span class="text-md md:text-xl">
								{formatShareAmount(holder.quantity)} @ {formatValue(price)}
							</span>
							<div class="w-full text-right opacity-55">
								{total[holder.player.name]}
							</div>
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	{:else}
		<div class="flex items-center justify-center p-6">
			<b>No holders yet</b>
		</div>
	{/if}
</Card>
