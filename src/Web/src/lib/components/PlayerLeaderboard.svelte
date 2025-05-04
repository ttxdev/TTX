<script lang="ts">
	import { goto } from '$app/navigation';
	import Chart from 'chart.js/auto';
	import type { PlayerDto } from '$lib/api';
	import { formatValue } from '$lib/util';

	type TableProps = {
		players: PlayerDto[];
		total: number;
		currentPage: number;
	};

	let { players = [], total = 0, currentPage = 1 }: TableProps = $props();

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

<div class="overflow-x-auto">
	<table class="table w-full max-md:text-sm">
		<thead class="sticky top-0 z-10 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-50">
			<tr class="border-b text-sm max-md:text-xs">
				<th class="rounded-tl-2xl py-4 text-center max-md:px-2">Player</th>
				<th class="py-4 text-center max-md:px-2">Portfolio Value</th>
				<th class="py-4 text-center max-md:hidden max-md:px-2">Chart</th>
				<th class="rounded-tr-2xl py-4 text-center max-md:hidden max-md:px-2">Action</th>
			</tr>
		</thead>
		<tbody>
			{#each players as player, index (player.id)}
				<tr
					class="cursor-pointer hover:bg-gray-50/30 dark:hover:bg-gray-800/30"
					onclick={() => goto(`/players/${player.slug}`)}
				>
					<td class="py-4 text-center max-md:px-2">
						<div class="items-left justify-left flex flex-col gap-3">
							<div class="flex flex-row items-center gap-3">
								<div class="flex flex-col items-center">
									<img
										src={player.avatar_url}
										alt={player.name}
										class="size-10 rounded-full max-md:h-6 max-md:w-6"
									/>
									{#if (currentPage - 1) * 20 + index === 0}
										<span
											class="badge dark:bg-neutral -mt-2 w-10 rounded-full bg-gray-100 px-3 py-1 text-xs text-yellow-400 dark:text-yellow-500"
										>
											1st
										</span>
									{:else if (currentPage - 1) * 20 + index === 1}
										<span
											class="badge dark:bg-neutral -mt-2 w-10 rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-400 dark:text-gray-500"
										>
											2nd
										</span>
									{:else if (currentPage - 1) * 20 + index === 2}
										<span
											class="badge dark:bg-neutral -mt-2 w-10 rounded-full bg-gray-100 px-3 py-1 text-xs text-orange-400 dark:text-orange-500"
										>
											3rd
										</span>
									{:else}
										<span
											class="badge dark:bg-neutral light:text-gray-900 -mt-2 w-10 rounded-full px-3 py-1 text-xs max-md:truncate max-md:text-xs dark:text-gray-300"
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

								<span class="">{player.name}</span>
							</div>
						</div>
					</td>
					<td class="py-4 text-center max-md:px-2">{formatValue(player.portfolio)}</td>
					<td class="flex items-center justify-center py-4 text-center max-md:hidden max-md:px-2">
						<div class="h-16 w-32">
							<canvas bind:this={chartElements[player.slug]}></canvas>
						</div>
					</td>
					<td class="py-4 text-center max-md:w-12 max-md:px-2">
						<button class="btn btn-ghost max-md:btn-sm rounded-lg max-md:px-2"> View </button>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

{#if totalPages > 1}
	<div class="mt-4 flex flex-col items-center gap-4 max-md:gap-2">
		<div class="text-sm max-md:text-xs">
			Showing {(currentPage - 1) * 20 + 1} to {Math.min(currentPage * 20, total)} of {total} players
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
