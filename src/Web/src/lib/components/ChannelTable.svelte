<script lang="ts">
	import { onMount } from 'svelte';
	import Chart from 'chart.js/auto';
	import { goto } from '$app/navigation';
	import { formatTicker, formatValue } from '$lib/util';
	import { OrderDirection, type ICreatorDto, type ICreatorPartialDto } from '$lib/api';

	type TableProps = {
		creators: ICreatorPartialDto[];
		total: number;
		currentPage: number;
		sortField: string;
		sortDirection: OrderDirection;
	};

	let {
		creators = [],
		total = 0,
		currentPage = 1,
		sortField = 'name',
		sortDirection = OrderDirection.Ascending
	}: TableProps = $props();

	let chartElements: { [key: string]: HTMLCanvasElement } = {};

	// Update sort function to use server-side sorting
	function sortChannels(field: 'Name' | 'Value' | 'IsLive') {
		const newDirection =
			field === sortField && sortDirection === OrderDirection.Ascending ? 'desc' : 'asc';
		const searchParams = new URLSearchParams(window.location.search);
		const search = searchParams.get('search') || '';

		// Use goto to trigger server-side reload with sort parameters
		goto(
			`?page=${currentPage}&orderBy=${field}&order=${newDirection}${search ? `&search=${search}` : ''}`,
			{
				keepFocus: true,
				invalidateAll: true,
				noScroll: true // Prevent automatic scrolling
			}
		);
	}

	// Remove client-side sorting
	let sortedChannels = $derived(creators);

	function calculatePercentChange(history: ICreatorDto['history']) {
		const oldest = history[0]?.value;
		const current = history[history.length - 1]?.value;
		if (!oldest) return 0;
		const change = ((current - oldest) / oldest) * 100;
		if (!isFinite(change)) return 99999;
		return isNaN(change) ? 0 : change;
	}

	function createChart(canvas: HTMLCanvasElement, history: ICreatorDto['history']) {
		const isUpward = history[history.length - 1]?.value > history[0]?.value;
		const lineColor = isUpward ? '#22c55e' : '#ef4444';

		return new Chart(canvas, {
			type: 'line',
			data: {
				labels: Array(history.length).fill(''), // Create empty labels
				datasets: [
					{
						data: history.map((d) => d.value),
						borderColor: lineColor,
						borderWidth: 3,
						fill: false,
						tension: 0,
						pointRadius: 0
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: true,
				plugins: {
					legend: {
						display: false
					}
				},
				scales: {
					x: {
						display: false
					},
					y: {
						display: false
					}
				}
			}
		});
	}

	// Add a function to destroy all charts
	function destroyCharts() {
		Object.values(chartElements).forEach((canvas) => {
			const chart = Chart.getChart(canvas);
			if (chart) {
				chart.destroy();
			}
		});
	}

	// Update charts when sorted channels change
	$effect(() => {
		if (sortedChannels) {
			// Wait for next tick to ensure DOM is updated
			setTimeout(() => {
				destroyCharts();
				sortedChannels.forEach((channel) => {
					if (chartElements[channel.name]) {
						createChart(chartElements[channel.name], channel.history);
					}
				});
			}, 0);
		}
	});

	// Update pagination calculation
	let totalPages = $derived(Math.max(1, Math.floor((total - 1) / 20) + 1));
	let pageNumbers = $derived(Array.from({ length: totalPages }, (_, i) => i + 1));

	// Handle page change
	function changePage(page: number) {
		const searchParams = new URLSearchParams(window.location.search);
		const search = searchParams.get('search') || '';
		const currentOrderBy = searchParams.get('orderBy') || sortField;
		const currentOrder = searchParams.get('order') || sortDirection;

		goto(
			`?page=${page}&orderBy=${currentOrderBy}&order=${currentOrder}${
				search ? `&search=${search}` : ''
			}`,
			{
				keepFocus: true,
				noScroll: true // Prevent automatic scrolling
			}
		);
	}

	onMount(() => {
		// Create charts for each channel
		creators.forEach((channel) => {
			if (chartElements[channel.name]) {
				createChart(chartElements[channel.name], channel.history);
			}
		});

		return () => {
			// Cleanup charts on component destroy
			destroyCharts();
		};
	});
</script>

<div class="overflow-x-auto rounded-2xl border border-gray-200 shadow-sm dark:border-gray-800">
	<table class="table w-full max-md:text-sm">
		<thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
			<tr class="border-b text-sm max-md:text-sm">
				<th
					class="cursor-pointer rounded-tl-2xl py-4 text-center max-md:px-2"
					onclick={() => sortChannels('Name')}
				>
					Creator
					{#if sortField === 'Name'}
						<span class="ml-1">{sortDirection === OrderDirection.Ascending ? '↑' : '↓'}</span>
					{/if}
				</th>
				<th
					class="cursor-pointer py-4 text-center max-md:px-2"
					onclick={() => sortChannels('Value')}
				>
					Price
					{#if sortField === 'Value'}
						<span class="ml-1">{sortDirection === OrderDirection.Ascending ? '↑' : '↓'}</span>
					{/if}
				</th>
				<th class="py-4 text-center max-md:hidden">Chart</th>
				<th
					class="cursor-pointer rounded-tr-2xl py-4 text-center max-md:px-2"
					onclick={() => sortChannels('IsLive')}
				>
					Status
					{#if sortField === 'IsLive'}
						<span class="ml-1">{sortDirection === OrderDirection.Ascending ? '↑' : '↓'}</span>
					{/if}
				</th>
			</tr>
		</thead>
		<tbody>
			{#each sortedChannels as channel (channel.id)}
				<tr
					class="group cursor-pointer border-b border-gray-100 transition-colors hover:bg-gray-50/50 dark:border-gray-800 dark:hover:bg-gray-800/50"
				>
					<td class="py-4 max-md:px-2">
						<a href="/creators/{channel.slug}" rel="noopener noreferrer" class="block">
							<div class="flex items-center gap-2">
								<div class="relative flex-shrink-0">
									<img
										src={channel.avatar_url}
										alt={channel.name}
										class="size-12 rounded-full object-cover ring-2 ring-gray-200 transition-all group-hover:ring-purple-400 max-md:size-8 dark:ring-gray-700"
									/>
									{#if channel.stream_status.is_live}
										<span
											class="absolute -top-1 -right-1 rounded-full bg-red-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-red-500"
										>
											LIVE
										</span>
									{/if}
								</div>
								<div class="flex min-w-0 flex-col">
									<div class="truncate font-semibold max-md:text-sm">{channel.name}</div>
									<div class="flex items-center gap-1">
										<span class="text-xs text-gray-500">
											{formatTicker(channel.ticker)}
										</span>
										<span
											class="{calculatePercentChange(channel.history) > 0
												? 'text-green-500'
												: calculatePercentChange(channel.history) < 0
													? 'text-red-500'
													: 'text-gray-500'} text-xs font-semibold text-nowrap"
										>
											{#if calculatePercentChange(channel.history) > 0}↗{/if}
											{#if calculatePercentChange(channel.history) < 0}↘{/if}
											{calculatePercentChange(channel.history).toFixed(2)}%
										</span>
									</div>
								</div>
							</div>
						</a>
					</td>
					<td class="py-4 text-center max-md:px-2 max-md:text-sm max-md:font-semibold">
						<a href="/creators/{channel.slug}" rel="noopener noreferrer" class="block">
							{formatValue(channel.value)}
						</a>
					</td>
					<td class="flex items-center justify-center py-4 max-md:hidden">
						<a
							href="/creators/{channel.slug}"
							rel="noopener noreferrer"
							class="block"
							aria-label="View Chart"
						>
							<div class="h-16 w-32">
								<canvas class="w-full" bind:this={chartElements[channel.name]}></canvas>
							</div>
						</a>
					</td>
					<td class="py-4 text-center max-md:px-2">
						<a href="/creators/{channel.slug}" rel="noopener noreferrer" class="block">
							{#if channel.stream_status.is_live}
								<span
									class="inline-flex items-center rounded-full bg-red-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-red-500"
								>
									LIVE
								</span>
							{:else}
								<span
									class="inline-flex items-center rounded-full bg-gray-400 px-2 py-0.5 text-[10px] font-bold text-white dark:bg-gray-500"
								>
									OFFLINE
								</span>
							{/if}
						</a>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

{#if totalPages > 1}
	<div class="mt-8 flex flex-col items-center gap-4 max-md:gap-3">
		<div class="text-sm text-gray-500 max-md:text-center max-md:text-sm dark:text-gray-400">
			Showing {(currentPage - 1) * 20 + 1} to {Math.min(currentPage * 20, total)} of {total} creators
		</div>
		<div class="join max-md:scale-90 max-md:flex-wrap max-md:justify-center">
			<button
				class="join-item btn rounded-l-2xl border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700"
				disabled={currentPage === 1}
				onclick={() => changePage(currentPage - 1)}
				aria-label="Previous page"
			>
				«
			</button>
			{#each pageNumbers as page (page)}
				<button
					class="join-item btn border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700 {currentPage ===
					page
						? 'border-purple-400 bg-purple-400/80 text-white hover:bg-[#8f44fb]'
						: 'hover:bg-gray-100 dark:hover:bg-gray-800'}"
					onclick={() => changePage(page)}
				>
					{page}
				</button>
			{/each}
			<button
				class="join-item btn rounded-r-2xl border-gray-200 max-md:px-2 max-md:text-sm dark:border-gray-700"
				disabled={currentPage === totalPages}
				onclick={() => changePage(currentPage + 1)}
			>
				»
			</button>
		</div>
	</div>
{/if}
