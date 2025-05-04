<script lang="ts">
	import type { CreatorPartialDto, PlayerPartialDto } from '$lib/api';
	import type { LinkableUser } from '$lib/types';
	import { formatValue } from '$lib/util';

	let {
		data,
		type
	}: {
		data: LinkableUser<CreatorPartialDto | PlayerPartialDto>[];
		type: keyof typeof headers;
	} = $props();

	let sortedData = $derived([...data].sort((a, b) => b.value - a.value).slice(0, 3));

	const heights = {
		first: 'h-64',
		second: 'h-48',
		third: 'h-32'
	};

	const headers = {
		creators: 'Top Creators',
		users: 'Top Players',
		portfolio: 'Top Investments'
	};
</script>

<div class="bg-base-200/50 relative rounded-xl p-6 {type === 'portfolio' ? 'h-[32rem]' : ''}">
	<div class="bg-base-300 absolute top-4 left-4 rounded-lg px-3 py-1 text-sm font-medium">
		{headers[type]}
	</div>

	<div
		class="{type === 'portfolio' ? '' : 'mt-8'}
		mx-auto flex h-full max-w-3xl items-end justify-center gap-8 max-md:gap-4"
	>
		<!-- Second Place -->
		<div class="flex w-1/3 flex-col items-center max-md:w-full">
			{#if sortedData[1]}
				<a href={sortedData[1].url}>
					<img src={sortedData[1].avatar_url} alt="" class="mb-2 size-12 rounded-full" />
				</a>
				<div class="indicator w-full">
					<span class="indicator-item indicator-center badge font-semibold text-[#C0C0C0]">2nd</span
					>
					<div class={`${heights.second} relative w-full rounded-t-lg bg-[#C0C0C0]/10`}>
						<div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
							<a href={sortedData[1].url} class="font-medium hover:underline max-md:text-sm">
								{sortedData[1].name}
							</a>
							<span class="text-sm opacity-75">
								{formatValue(sortedData[1].value)}
							</span>
						</div>
					</div>
				</div>
			{/if}
		</div>

		<!-- First Place -->
		<div class="flex w-1/3 flex-col items-center max-md:w-full">
			{#if sortedData[0]}
				<a href={sortedData[0].url}>
					<img src={sortedData[0].avatar_url} alt="" class="mb-2 size-12 rounded-full" />
				</a>
				<div class="indicator w-full">
					<span class="indicator-item indicator-center badge font-semibold text-[#FFD700]">1st</span
					>
					<div class={`${heights.first} relative w-full rounded-t-lg bg-[#FFD700]/10`}>
						<div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
							<a href={sortedData[0].url} class="font-medium hover:underline max-md:text-sm">
								{sortedData[0].name}
							</a>
							<span class="text-sm opacity-75 max-md:text-xs">
								{formatValue(sortedData[0].value)}
							</span>
						</div>
					</div>
				</div>
			{/if}
		</div>

		<!-- Third Place -->
		<div class="flex w-1/3 flex-col items-center max-md:w-full">
			{#if sortedData[2]}
				<a href={sortedData[2].url}>
					<img src={sortedData[2].avatar_url} alt="" class="mb-2 size-12 rounded-full" />
				</a>
				<div class="indicator w-full">
					<span class="indicator-item indicator-center badge font-semibold text-[#CD7F32]">3rd</span
					>
					<div class={`${heights.third} relative w-full rounded-t-lg bg-[#CD7F32]/10`}>
						<div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
							<a href={sortedData[2].url} class="font-medium hover:underline max-md:text-sm"
								>{sortedData[2].name}</a
							>
							<span class="text-sm opacity-75">
								{formatValue(sortedData[2].value)}
							</span>
						</div>
					</div>
				</div>
			{/if}
		</div>
	</div>
</div>
