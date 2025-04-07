<script lang="ts">
	import { formatTicker } from '$lib/util';
	import CreatorValueChart from './CreatorValueChart.svelte';
	import CreatorCurrentValue from './CreatorCurrentValue.svelte';
	import type { ICreatorDto, Vote } from '$lib/api';

	let props: {
		creator: ICreatorDto;
		history: Vote[];
	} = $props();
	let creator = $derived(props.creator);
	let history = $derived<Vote[]>(props.history);
	let currentValue = $derived(history[history.length - 1]?.value ?? 0);
</script>

<div
	class="bg-base-200/50 w-full rounded-lg bg-clip-padding p-4 shadow-md backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter"
>
	<div class="mb-4 flex flex-col items-center justify-between sm:flex-row">
		<div class="flex w-full flex-row items-center justify-between px-3">
			<div class="flex flex-row gap-3">
				<div class="flex flex-col items-center">
					<a href={creator.url} target="_blank">
						<img
							src={creator.avatar_url}
							alt=""
							class="h-12 w-12 rounded-full border-2 border-white object-cover shadow-lg"
						/>
					</a>
					{#if creator.stream_status.is_live}
						<span
							class="-mt-2.5 h-fit w-fit rounded-full bg-red-400 px-2 text-xs font-bold text-white"
						>
							LIVE
						</span>
					{:else}
						<span
							class="-mt-2.5 h-fit w-fit rounded-full bg-gray-600 px-2 text-xs font-bold text-white"
						>
							OFFLINE
						</span>
					{/if}
				</div>
				<div class="flex flex-col">
					<a
						href={creator.url}
						target="_blank"
						class="text-lg font-semibold text-purple-500 hover:underline">{creator.name}</a
					>
					<span class="text-opacity-60 font-mono text-sm">{formatTicker(creator.ticker)}</span>
				</div>
			</div>
			<div class="relative flex flex-col text-center">
				<CreatorCurrentValue value={currentValue} />
				<p class="w-24 text-sm">Current Price</p>
			</div>
		</div>
	</div>
	<div class="relative min-h-[400px] w-full">
		<div class="absolute h-3/4 w-full rounded-lg border border-gray-200/15 p-4">
			<CreatorValueChart {history} />
		</div>
	</div>
</div>
