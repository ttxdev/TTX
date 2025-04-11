<script lang="ts">
	import { goto } from '$app/navigation';
	import type { Interval } from './+page.server';

	const { interval }: { interval: Interval } = $props();

	const intervals: { label: string; value: Interval }[] = [
		{ label: '24h', value: '24h' },
		{ label: '12h', value: '12h' },
		{ label: '6h', value: '6h' },
		{ label: '1h', value: '1h' }
	];

	function handleIntervalChange(newInterval: Interval) {
		goto(`?interval=${newInterval}`, {
			keepFocus: true,
			invalidateAll: true,
			noScroll: true,
			replaceState: true
		});
	}
</script>

<div class="join bg-base-200 rounded-lg p-1">
	{#each intervals as { label, value }}
		<button
			class="join-item btn btn-sm min-w-[4rem] border-none {interval === value
				? 'bg-purple-600 font-medium text-white shadow-md'
				: 'hover:bg-base-300 text-base-content/70 hover:text-base-content bg-transparent'}"
			onclick={() => handleIntervalChange(value)}
		>
			{label}
		</button>
	{/each}
</div>
