<script lang="ts">
	import type { VoteDto } from '$lib/api';
	import { formatValue } from '$lib/util';
	import { Chart, registerables } from 'chart.js';
	import { onMount } from 'svelte';

	let canvas: HTMLCanvasElement | null = null;
	const props: { history: VoteDto[] } = $props();
	let history = $derived<VoteDto[]>(props.history);
	let chart: Chart | null = $state(null);

	$effect(() => {
		if (chart) {
			chart.data.labels = history.map((d) => d.time);
			chart.data.datasets[0].data = history.map((d) => d.value);
			chart.update();
		}
	});

	onMount(async () => {
		await import('chartjs-plugin-zoom').then(({ default: zoomPlugin }) => {
			Chart.register(zoomPlugin, ...registerables);
		});

		const ctx = canvas!.getContext('2d');
		chart = new Chart(ctx!, {
			type: 'line',
			data: {
				labels: history.map((d) => d.time),
				datasets: [
					{
						label: 'Price',
						data: history.map((d) => d.value),
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
								return `Value: ${formatValue(tooltipItem.parsed.y)}`;
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
							display: true
						}
					}
				}
			}
		});
	});
</script>

<canvas bind:this={canvas}></canvas>
