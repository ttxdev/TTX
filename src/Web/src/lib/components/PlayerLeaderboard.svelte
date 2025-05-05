<script lang="ts">
	import { goto } from '$app/navigation';
	import Chart from 'chart.js/auto';
	import type { PlayerDto } from '$lib/api';
	import { formatValue } from '$lib/util';

	type TableProps = {
		players: PlayerDto[];
		total: number;
		currentPage: number;
		showPlace: boolean;
	};

	let { players = [], total = 0, currentPage = 1, showPlace = true }: TableProps = $props();

	// Pagination
	let totalPages = $derived(Math.max(1, Math.ceil(total / 20)));
	let pageNumbers = $derived(Array.from({ length: totalPages }, (_, i) => i + 1));

	function changePage(page: number) {
		goto(`?page=${page}`, {
			keepFocus: true,
			noScroll: true
		});
	}

	// Chart management
	let chartElements: { [key: string]: HTMLCanvasElement } = {};

	function createChart(canvas: HTMLCanvasElement, history: PlayerDto['history']) {
		const isUpward = history[history.length - 1]?.value > history[0]?.value;
		const lineColor = isUpward ? '#22c55e' : '#ef4444';

		return new Chart(canvas, {
			type: 'line',
			data: {
				labels: Array(history.length).fill(''), // Empty labels for each data point
				datasets: [
					{
						data: history.map((d) => d.value),
						borderColor: lineColor,
						borderWidth: 2,
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

	function destroyCharts() {
		Object.values(chartElements).forEach((canvas) => {
			const chart = Chart.getChart(canvas);
			if (chart) {
				chart.destroy();
			}
		});
	}

	function calculatePercentChange(history: PlayerDto['history']) {
		const oldest = history[0]?.value;
		const current = history[history.length - 1]?.value;
		if (!oldest) return 0;
		const change = ((current - oldest) / oldest) * 100;
		if (!isFinite(change)) return 99999;
		return isNaN(change) ? 0 : change;
	}

	// Cleanup charts on component destroy
	$effect(() => {
		destroyCharts();
		players.forEach((player) => {
			if (chartElements[player.slug]) {
				createChart(chartElements[player.slug], player.history);
			}
		});
	});
</script>

<div class="overflow-x-auto rounded-2xl border border-gray-200 shadow-sm dark:border-gray-800">
	<table class="table w-full max-md:text-base">
		<thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
			<tr class="border-b text-sm max-md:text-base">
				<th class="rounded-tl-2xl py-6 text-center max-md:px-4">Player</th>
				<th class="py-6 text-center max-md:px-4">Portfolio</th>
				<th class="py-6 text-center max-md:hidden max-md:px-4">Chart</th>
				<th class="rounded-tr-2xl py-6 text-center max-md:hidden max-md:px-4">Action</th>
			</tr>
		</thead>
		<tbody>
			{#each players as player, index (player.id)}
				<tr
					class="group cursor-pointer border-b border-gray-100 transition-colors hover:bg-gray-50/50 dark:border-gray-800 dark:hover:bg-gray-800/50"
					onclick={() => goto(`/players/${player.slug}`)}
				>
					<td class="py-6 text-center max-md:px-4">
						<div class="items-left justify-left flex flex-col gap-3">
							<div class="flex flex-row items-center gap-4">
								<div class="flex flex-col items-center">
									<div class="relative">
										<img
											src={player.avatar_url}
											alt={player.name}
											class="size-12 rounded-full object-cover ring-2 ring-gray-200 transition-all group-hover:ring-purple-400 max-md:size-14 dark:ring-gray-700"
										/>
										{#if showPlace && (currentPage - 1) * 20 + index === 0}
											<span
												class="absolute -top-1 -right-1 rounded-full bg-yellow-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-yellow-500"
											>
												1st
											</span>
										{:else if showPlace && (currentPage - 1) * 20 + index === 1}
											<span
												class="absolute -top-1 -right-1 rounded-full bg-gray-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-gray-500"
											>
												2nd
											</span>
										{:else if showPlace && (currentPage - 1) * 20 + index === 2}
											<span
												class="absolute -top-1 -right-1 rounded-full bg-orange-400 px-2 py-0.5 text-xs font-bold text-white dark:bg-orange-500"
											>
												3rd
											</span>
										{:else if showPlace}
											<span
												class="absolute -top-1 -right-1 rounded-full bg-gray-200 px-2 py-0.5 text-xs font-bold text-gray-700 dark:bg-gray-700 dark:text-gray-300"
											>
												{(() => {
													const place = (currentPage - 1) * 20 + index + 1;
													const suffix =
														place % 10 === 1 && place !== 11
															? 'st'
															: place % 10 === 2 && place !== 12
																? 'nd'
																: place % 10 === 3 && place !== 13
																	? 'rd'
																	: 'th';
													return `${place}${suffix}`;
												})()}
											</span>
										{/if}
									</div>
								</div>
								<div class="flex min-w-0 flex-col">
									<span class="max-md:text-base max-md:font-semibold">{player.name}</span>
									<span
										class="{calculatePercentChange(player.history) > 0
											? 'text-green-500'
											: calculatePercentChange(player.history) < 0
												? 'text-red-500'
												: 'text-gray-500'} text-sm font-semibold text-nowrap"
									>
										{#if calculatePercentChange(player.history) > 0}↗{/if}
										{#if calculatePercentChange(player.history) < 0}↘{/if}
										{calculatePercentChange(player.history).toFixed(2)}%
									</span>
								</div>
							</div>
						</div>
					</td>
					<td class="py-6 text-center max-md:px-4 max-md:text-base max-md:font-semibold"
						>{formatValue(player.portfolio)}</td
					>
					<td class="flex items-center justify-center py-6 text-center max-md:hidden max-md:px-4">
						<div class="h-16 w-32">
							<canvas bind:this={chartElements[player.slug]}></canvas>
						</div>
					</td>
					<td class="py-6 text-center max-md:hidden max-md:px-4">
						<button class="btn btn-ghost rounded-lg text-purple-400 hover:bg-purple-400/10"
							>View</button
						>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

{#if totalPages > 1}
	<div class="mt-8 flex flex-col items-center gap-4 max-md:gap-3">
		<div class="text-sm text-gray-500 max-md:text-center max-md:text-base dark:text-gray-400">
			Showing {(currentPage - 1) * 20 + 1} to {Math.min(currentPage * 20, total)} of {total} players
		</div>
		<div class="join max-md:scale-100">
			<button
				class="join-item btn rounded-l-2xl border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700"
				disabled={currentPage === 1}
				onclick={() => changePage(currentPage - 1)}
				aria-label="Previous page"
			>
				«
			</button>
			{#each pageNumbers as page (page)}
				<button
					class="join-item btn border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700 {currentPage ===
					page
						? 'border-purple-400 bg-purple-400/80 text-white hover:bg-[#8f44fb]'
						: 'hover:bg-gray-100 dark:hover:bg-gray-800'}"
					onclick={() => changePage(page)}
				>
					{page}
				</button>
			{/each}
			<button
				class="join-item btn rounded-r-2xl border-gray-200 max-md:px-4 max-md:text-base dark:border-gray-700"
				disabled={currentPage === totalPages}
				onclick={() => changePage(currentPage + 1)}
			>
				»
			</button>
		</div>
	</div>
{/if}
