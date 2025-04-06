<script lang="ts">
	import type { ShareDto } from '$lib/api';
	import { formatShareAmount, formatValue, formatCreatorString } from '$lib/util';
	import Card from '$lib/components/shared/Card.svelte';

	let { shares }: { shares: ShareDto[] } = $props();
	shares = shares.sort((a, b) => b.creator.value * b.quantity - a.creator.value * a.quantity);
</script>

<Card title="Shares">
	{#if shares.length !== 0}
		<table class="table">
			<tbody>
				{#each shares as share (share.creator.id)}
					<tr class="flex flex-row justify-between rounded-md py-1 md:p-2">
						<td class="flex items-center justify-center gap-3">
							<a href="/channels/{share.creator.slug}" class="flex flex-col">
								<img
									alt={share.creator.name}
									src={share.creator.avatar_url}
									class="size-10 rounded-full"
								/>
							</a>
							<div class="flex flex-col">
								<a
									href="/channels/{share.creator.slug}"
									class="text-lg font-semibold text-violet-500 hover:underline"
								>
									{formatCreatorString(share.creator.name)}
								</a>
								<span class="text-sm">
									{formatShareAmount(share.quantity)} @ {formatValue(share.creator.value)}
								</span>
							</div>
						</td>
						<td class="flex flex-col items-center justify-center p-2 font-bold">
							<div class="w-full text-right text-lg opacity-55">
								{formatValue(share.quantity * share.creator.value)}
							</div>
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	{:else}
		<div class="flex items-center justify-center p-6">
			<b>No shares yet</b>
		</div>
	{/if}
</Card>
