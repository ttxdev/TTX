<script lang="ts">
	import { onMount } from 'svelte';
	import Chart from 'chart.js/auto';
	import { goto } from '$app/navigation';
	import { formatTicker, formatValue } from '$lib/util';
	import type { ICreatorDto } from '$lib/api';

	type TableProps = {
		channels: ICreatorDto[];
		total: number;
		currentPage: number;
		sortField: string;
		sortDirection: string;
	};

	let {
		channels = [],
		total = 0,
		currentPage = 1,
		sortField = 'name',
		sortDirection = 'asc'
	}: TableProps = $props();

	let chartElements: { [key: string]: HTMLCanvasElement } = {};

	// Update sort function to use server-side sorting
	function sortChannels(field: 'Name' | 'Value' | 'IsLive') {
		const newDirection = field === sortField && sortDirection === 'asc' ? 'desc' : 'asc';
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
	let sortedChannels = $derived(channels);

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
		channels.forEach((channel) => {
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

<div class="">
	<table class="table w-full max-md:text-sm">
		<thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
			<tr class="border-b text-sm max-md:text-xs">
				<th
					class="cursor-pointer rounded-tl-2xl py-4 text-center max-md:px-2"
					onclick={() => sortChannels('Name')}
				>
					Creator
					{#if sortField === 'Name'}
						<span class="ml-1">{sortDirection === 'asc' ? '↑' : '↓'}</span>
					{/if}
				</th>
				<th
					class="cursor-pointer py-4 text-center max-md:px-2"
					onclick={() => sortChannels('Value')}
				>
					Price
					{#if sortField === 'Value'}
						<span class="ml-1">{sortDirection === 'asc' ? '↑' : '↓'}</span>
					{/if}
				</th>
				<th
					class="cursor-pointer py-4 text-center max-md:px-2"
					onclick={() => sortChannels('IsLive')}
				>
					Live
					{#if sortField === 'IsLive'}
						<span class="ml-1">{sortDirection === 'asc' ? '↑' : '↓'}</span>
					{/if}
				</th>
				<th class="py-4 text-center max-md:hidden">Chart</th>
				<th class="py-4 text-center max-md:hidden">Change</th>
				<th class="rounded-tr-2xl py-4 text-center max-md:px-2">Action</th>
			</tr>
		</thead>
		<tbody>
			{#each sortedChannels as channel (channel.id)}
				<tr class="hover:bg-gray-50/30 dark:hover:bg-gray-800/30">
					<td class="py-4 max-md:w-32 max-md:px-2">
						<a href="/channels/{channel.slug}" rel="noopener noreferrer">
							<div class="items-left justify-left flex gap-3 max-md:gap-2">
								<div class="h-8 w-8 max-md:h-6 max-md:w-6">
									<img
										src={channel.avatar_url}
										alt={channel.name}
										class="h-full w-full rounded-full"
									/>
								</div>
								<div class="max-md:truncate">
									<div class="font-medium max-md:truncate max-md:text-xs">{channel.name}</div>
									<div class="text-sm text-gray-500 max-md:truncate max-md:text-xs">
										{formatTicker(channel.ticker)}
									</div>
								</div>
							</div>
						</a>
					</td>
					<td class="py-4 text-center max-md:w-12 max-md:px-2 max-md:text-xs">
						<a href="/channels/{channel.slug}" rel="noopener noreferrer">
							{formatValue(channel.value)}
						</a>
					</td>
					<td class="py-4 text-center max-md:w-12 max-md:px-2 max-md:text-xs">
						<a href="/channels/{channel.slug}" rel="noopener noreferrer" aria-label="Channel Chart">
							{#if channel.is_live}
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
						</a>
					</td>
					<td class="flex items-center justify-center py-4 max-md:hidden">
						<a href="/channels/{channel.slug}" rel="noopener noreferrer" aria-label="Channel Chart">
							<div class="h-16 w-32">
								<canvas class="w-full" bind:this={chartElements[channel.name]}></canvas>
							</div>
						</a>
					</td>
					<td class="py-4 text-center max-md:hidden">
						<a href="/channels/{channel.slug}" rel="noopener noreferrer">
							<span
								class="{calculatePercentChange(channel.history) > 0
									? 'text-green-500'
									: calculatePercentChange(channel.history) < 0
										? 'text-red-500'
										: 'text-gray-500'} text-nowrap"
							>
								{#if calculatePercentChange(channel.history) > 0}↗{/if}
								{#if calculatePercentChange(channel.history) < 0}↘{/if}
								{calculatePercentChange(channel.history).toFixed(2)}%
							</span>
						</a>
					</td>
					<td class="py-4 text-center max-md:w-12 max-md:px-2">
						<a href="/channels/{channel.slug}">
							<button class="btn btn-ghost max-md:btn-sm rounded-lg max-md:px-2"> Trade </button>
						</a>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

{#if totalPages > 1}
	<div class="mt-4 flex flex-col items-center gap-4 max-md:gap-2">
		<div class="text-sm max-md:text-xs">
			Showing {(currentPage - 1) * 20 + 1} to {Math.min(currentPage * 20, total)} of {total} creators
		</div>
		<div class="join max-md:scale-90">
			<button
				class="join-item btn rounded-l-2xl"
				disabled={currentPage === 1}
				onclick={() => changePage(currentPage - 1)}
				aria-label="Previous page"
			>
				«
			</button>
			{#each pageNumbers as page (page)}
				<button
					class="join-item btn {currentPage === page
						? 'border-purple-400 bg-purple-400/80 text-white hover:bg-[#8f44fb]'
						: ''}"
					onclick={() => changePage(page)}
				>
					{page}
				</button>
			{/each}
			<button
				class="join-item btn rounded-r-2xl"
				disabled={currentPage === totalPages}
				onclick={() => changePage(currentPage + 1)}
			>
				»
			</button>
		</div>
	</div>
{/if}
