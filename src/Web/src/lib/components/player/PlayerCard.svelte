<script lang="ts">
	import { onMount } from 'svelte';
	import { Chart, registerables } from 'chart.js/auto';
	import { formatValue } from '$lib/util';
	import type { PlayerDto } from '$lib/api';
	import type { LinkableUser } from '$lib/types';

	let { player }: { player: LinkableUser<PlayerDto>; place: number } = $props();

	let canvas: HTMLCanvasElement | null = null;
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	let chart: Chart | null = null;

	onMount(async () => {
		const { default: zoomPlugin } = await import('chartjs-plugin-zoom');
		Chart.register(zoomPlugin, ...registerables);

		const ctx = canvas!.getContext('2d');
		chart = new Chart(ctx!, {
			type: 'line',
			data: {
				labels: player.history.map((d) => d.time),
				datasets: [
					{
						label: 'Price',
						data: player.history.map((d) => d.value),
						segment: {
							borderColor: (ctx) => {
								const difference = ctx.p0DataIndex > 0 ? ctx.p0.parsed.y - ctx.p1.parsed.y : 0;

								return difference <= 0 ? '#22c55e' : '#ef4444';
							}
						},
						tension: 0,
						fill: false,
						borderWidth: 2,
						pointRadius: 0,
						pointHoverRadius: 4
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				plugins: {
					legend: {
						display: false
					},
					tooltip: {
						enabled: true,
						mode: 'index',
						intersect: false,
						callbacks: {
							title: (tooltipItems) => {
								const date = new Date(tooltipItems[0].label);
								return date.toLocaleString();
							},
							label: (tooltipItem) => {
								return `Value: $${tooltipItem.parsed.y.toLocaleString()}`;
							},
							labelColor: (tooltipItem) => {
								const currentValue = tooltipItem.parsed.y;
								const previousIndex = tooltipItem.dataIndex > 0 ? tooltipItem.dataIndex - 1 : 0;
								const previousValue =
									typeof tooltipItem.dataset.data[previousIndex] === 'object'
										? // eslint-disable-next-line @typescript-eslint/no-explicit-any
											(tooltipItem.dataset.data[previousIndex] as any).value
										: Number(tooltipItem.dataset.data[previousIndex]);
								const difference = currentValue - previousValue;
								const color = difference >= 0 ? '#22c55e' : '#ef4444';

								return {
									backgroundColor: color,
									borderColor: color
								};
							}
						}
					},
					zoom: {
						zoom: {
							wheel: {
								enabled: true
							},
							pinch: {
								enabled: true
							},
							drag: {
								enabled: true,
								modifierKey: 'ctrl'
							},
							mode: 'x'
						},
						pan: {
							enabled: true,
							mode: 'x'
						}
					}
				},
				scales: {
					x: {
						ticks: {
							display: false
						},
						grid: {
							display: false
						},
						border: {
							display: false
						}
					},
					y: {
						beginAtZero: false,
						grid: {
							display: false
						},
						border: {
							display: false
						},
						ticks: {
							display: true,
							callback: function (value: number) {
								return formatValue(value);
							}
						}
					}
				}
			}
		});

		// TODO: replace with player portfolio
		// setInterval(async () => {
		// 	const data = await client
		// 		.getCreatorValueHistory(player.slug, TimeStep._0, last)
		// 		.then((res) => res.history);
		// 	last = new Date();
		// 	if (data.length === 0) {
		// 		return;
		// 	}

		// 	chart!.data.labels = [...player.history.map((d) => d.time), ...data.map((d) => d.time)];
		// 	chart!.data.datasets[0].data = [
		// 		...player.history.map((d) => d.value),
		// 		...data.map((d) => d.value)
		// 	];
		// 	chart!.update();
		// }, 1_000);
	});
</script>

<!-- Player Card Layout -->

<div
	class="player-card bg-base-200/50 w-full rounded-lg p-4 shadow-md
         backdrop-blur backdrop-contrast-100 backdrop-saturate-100"
>
	<!-- Chart Section -->
	<div class="m-3 flex">
		<div class="bg-base-300 left-4 top-4 rounded-lg px-3 py-1 text-xl font-medium">
			Portfolio Value
		</div>
	</div>
	<div class="chart-container relative mb-4 h-48 w-full">
		<canvas bind:this={canvas}></canvas>
	</div>

	<!-- Bottom Info Section -->
	<div class="flex items-center justify-between px-2">
		<!-- Left: Avatar + Name/#Place -->
		<div class="flex items-center gap-3">
			<img
				src={player.avatar_url}
				alt="Avatar"
				class="h-12 w-12 rounded-full border-2 border-white object-cover shadow-lg"
			/>
			<div class="flex flex-col">
				<span class="text-lg font-semibold">
					{player.name}
				</span>
				<div class="flex">
					<!-- <PlayerPlacement {place} /> -->
				</div>
			</div>
		</div>

		<!-- Right: Net Value + Change -->
		<div class="flex flex-row gap-8 text-right">
			<div class="flex flex-col text-center">
				<h1 class="text-xl font-bold">
					{formatValue(player.portfolio)}
				</h1>
				<p class="text-sm">Portfolio Value</p>
			</div>
			<div class="flex flex-col text-center">
				<h1 class="text-xl font-bold">
					{formatValue(player.credits)}
				</h1>
				<p class="text-sm">Credits</p>
			</div>
		</div>
	</div>
</div>
